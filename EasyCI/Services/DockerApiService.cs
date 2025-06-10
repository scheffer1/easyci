using Docker.DotNet;
using Docker.DotNet.Models;
using EasyCI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EasyCI.Services
{
    public class DockerApiService
    {
        private readonly Dictionary<int, DockerClient> _clientCache = new();

        private DockerClient GetOrCreateClient(DockerContainer container)
        {
            if (_clientCache.TryGetValue(container.Id, out var cachedClient))
            {
                return cachedClient;
            }

            var client = CreateClient(container);
            _clientCache[container.Id] = client;
            return client;
        }

        private DockerClient CreateClient(DockerContainer container)
        {
            var endpoint = new Uri($"tcp://{container.Host}:{container.Port}");

            if (container.UseTLS && !string.IsNullOrEmpty(container.CertificatePath))
            {
                var credentials = new DockerClientConfiguration(
                    endpoint,
                    new AnonymousCredentials()
                );
                return credentials.CreateClient();
            }
            else
            {
                var credentials = new DockerClientConfiguration(
                    endpoint,
                    new AnonymousCredentials()
                );
                return credentials.CreateClient();
            }
        }

        public async Task<(bool Success, string Message)> TestConnectionAsync(DockerContainer container)
        {
            try
            {
                var client = GetOrCreateClient(container);
                var info = await client.System.GetSystemInfoAsync();
                return (true, $"Conexão bem-sucedida. Docker versão: {info.ServerVersion}");
            }
            catch (Exception ex)
            {
                return (false, $"Falha na conexão: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se uma imagem Docker está disponível
        /// </summary>
        private async Task<bool> IsImageAvailableAsync(DockerClient client, string imageName)
        {
            try
            {
                // Listar todas as imagens para verificar se a imagem existe
                var allImages = await client.Images.ListImagesAsync(new ImagesListParameters());

                // Verificar se a imagem já existe (verificar tanto o nome quanto as tags)
                bool imageExists = allImages.Any(img =>
                    img.RepoTags != null &&
                    img.RepoTags.Any(tag =>
                        tag.StartsWith(imageName) ||
                        tag.StartsWith($"{imageName}:latest") ||
                        tag.Equals(imageName) ||
                        tag.Equals($"{imageName}:latest")));

                return imageExists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao verificar imagem {imageName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tenta fazer pull de uma imagem Docker
        /// </summary>
        private async Task<bool> PullImageAsync(DockerClient client, string imageName)
        {
            try
            {
                var progress = new Progress<JSONMessage>(message =>
                {
                    // Log do progresso do pull
                    if (!string.IsNullOrEmpty(message.Status))
                    {
                        System.Diagnostics.Debug.WriteLine($"Pull {imageName}: {message.Status}");
                    }
                });

                await client.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = imageName.Contains(":") ? imageName.Split(':')[0] : imageName,
                        Tag = imageName.Contains(":") ? imageName.Split(':')[1] : "latest"
                    },
                    null,
                    progress);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao fazer pull da imagem {imageName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Garante que uma imagem Docker está disponível, fazendo pull se necessário
        /// </summary>
        private async Task<(bool Success, string Message)> EnsureImageAsync(DockerClient client, string imageName)
        {
            try
            {
                // Verificar se a imagem já existe
                if (await IsImageAvailableAsync(client, imageName))
                {
                    return (true, $"Imagem {imageName} já está disponível");
                }

                // Tentar fazer pull da imagem
                if (await PullImageAsync(client, imageName))
                {
                    return (true, $"Imagem {imageName} baixada com sucesso");
                }

                return (false, $"Não foi possível baixar a imagem {imageName}. Execute manualmente: docker pull {imageName}");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao garantir imagem {imageName}: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> CreateDirectoryAsync(DockerContainer container, string path)
        {
            try
            {
                var client = GetOrCreateClient(container);

                // Garantir que a imagem Alpine está disponível
                var alpineResult = await EnsureImageAsync(client, "alpine");
                if (!alpineResult.Success)
                {
                    return (false, alpineResult.Message);
                }

                // Usar uma imagem Alpine para criar o diretório sem necessidade de montar volumes
                var createResponse = await client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = "alpine",
                    Cmd = new[] { "mkdir", "-p", path }
                    // Removemos o HostConfig com Binds que estava causando problemas
                });

                await client.Containers.StartContainerAsync(createResponse.ID, null);
                var waitResponse = await client.Containers.WaitContainerAsync(createResponse.ID);
                await client.Containers.RemoveContainerAsync(createResponse.ID, new ContainerRemoveParameters());

                if (waitResponse.StatusCode == 0)
                {
                    return (true, $"Diretório criado com sucesso: {path}");
                }
                else
                {
                    return (false, $"Falha ao criar diretório. Código de status: {waitResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao criar diretório: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ExecuteDockerComposeAsync(
            DockerContainer container,
            string workingDirectory,
            string composeFilePath)
        {
            try
            {
                // Usar uma imagem que tenha docker-compose instalado
                var client = GetOrCreateClient(container);

                // Usar imagem Alpine com docker-compose instalado
                var composeResult = await EnsureImageAsync(client, "alpine:latest");
                if (!composeResult.Success)
                {
                    return (false, composeResult.Message);
                }

                // Primeiro, listar o conteúdo do diretório para debug
                var listResult = await ListDirectoryContentsAsync(client, workingDirectory);

                // Procurar pelo arquivo docker-compose
                var findComposeResult = await FindDockerComposeFileAsync(client, workingDirectory);

                string composeCommand;
                string composeFile = "";

                if (findComposeResult.Success)
                {
                    // Usar o arquivo encontrado
                    composeFile = findComposeResult.Message;
                    composeCommand = $"docker-compose -f {composeFile} up -d";
                }
                else if (!string.IsNullOrEmpty(composeFilePath))
                {
                    // Usar o arquivo especificado
                    composeFile = composeFilePath;
                    composeCommand = $"docker-compose -f {composeFilePath} up -d";
                }
                else
                {
                    // Tentar arquivo padrão
                    composeCommand = "docker-compose up -d";
                }

                // Verificar se o arquivo existe antes de tentar executar
                if (!string.IsNullOrEmpty(composeFile))
                {
                    var fileCheckResult = await CheckFileExistsAsync(client, workingDirectory, composeFile);
                    if (!fileCheckResult.Success)
                    {
                        return (false, $"Arquivo docker-compose não encontrado: {composeFile}. {fileCheckResult.Message}");
                    }
                }

                // Instalar docker-compose e executar o comando
                var installAndRunCommand = $"apk add --no-cache docker-compose && {composeCommand}";

                var createResponse = await client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = "alpine:latest",
                    WorkingDir = workingDirectory,
                    Cmd = new[] { "sh", "-c", installAndRunCommand },
                    HostConfig = new HostConfig
                    {
                        Binds = new[]
                        {
                            "/var/run/docker.sock:/var/run/docker.sock",
                            $"{workingDirectory}:{workingDirectory}"
                        }
                    }
                });

                await client.Containers.StartContainerAsync(createResponse.ID, null);
                var waitResponse = await client.Containers.WaitContainerAsync(createResponse.ID);

                var logs = await client.Containers.GetContainerLogsAsync(
                    createResponse.ID,true,
                    new ContainerLogsParameters { ShowStdout = true, ShowStderr = true });

                var (stdout, stderr) = await logs.ReadOutputToEndAsync(cancellationToken: CancellationToken.None);

                // Obter o código de saída real do container
                var inspectResponse = await client.Containers.InspectContainerAsync(createResponse.ID);
                var exitCode = inspectResponse.State.ExitCode;

                await client.Containers.RemoveContainerAsync(createResponse.ID, new ContainerRemoveParameters());

                if (exitCode == 0)
                {
                    return (true, $"Docker Compose executado com sucesso. Output: {stdout}");
                }
                else
                {
                    return (false, $"Falha ao executar Docker Compose. Código de saída: {exitCode}. Directory listing: {listResult}. Logs: {stdout} {stderr}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao executar Docker Compose: {ex.Message}");
            }
        }

        /// <summary>
        /// Executa um comando shell em um container Docker
        /// </summary>
        /// <param name="container">Container Docker onde o comando será executado</param>
        /// <param name="workingDirectory">Diretório de trabalho para o comando</param>
        /// <param name="command">Comando a ser executado</param>
        /// <returns>Resultado da operação</returns>
        public async Task<(bool Success, string Message)> ExecuteCommandAsync(
            DockerContainer container,
            string workingDirectory,
            string command)
        {
            try
            {
                var client = GetOrCreateClient(container);

                // Determinar se é um comando git ou shell genérico
                bool isGitCommand = command.Contains("git ") || command.Contains("clone") || command.Contains("pull") || command.Contains("push");
                bool isShellCommand = command.StartsWith("/bin/sh");

                string imageName;
                string[] cmdArray;

                if (isShellCommand || !isGitCommand)
                {
                    // Para comandos shell genéricos, usar Alpine padrão
                    var alpineResult = await EnsureImageAsync(client, "alpine:latest");
                    if (!alpineResult.Success)
                    {
                        return (false, alpineResult.Message);
                    }
                    imageName = "alpine:latest";

                    if (command.StartsWith("/bin/sh -c "))
                    {
                        // Extrair o comando real entre aspas
                        var startIndex = command.IndexOf('"') + 1;
                        var endIndex = command.LastIndexOf('"');
                        if (startIndex > 0 && endIndex > startIndex)
                        {
                            var actualCommand = command.Substring(startIndex, endIndex - startIndex);
                            cmdArray = new[] { "sh", "-c", actualCommand };
                        }
                        else
                        {
                            // Fallback: remover apenas "/bin/sh -c "
                            cmdArray = new[] { "sh", "-c", command.Substring(12) };
                        }
                    }
                    else
                    {
                        cmdArray = new[] { "sh", "-c", command };
                    }
                }
                else
                {
                    // Para comandos git, usar alpine/git
                    var gitResult = await EnsureImageAsync(client, "alpine/git");
                    if (!gitResult.Success)
                    {
                        return (false, gitResult.Message);
                    }
                    imageName = "alpine/git";
                    cmdArray = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                }

                var createResponse = await client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = imageName,
                    WorkingDir = workingDirectory,
                    Cmd = cmdArray,
                    HostConfig = new HostConfig
                    {
                        Binds = new[] { $"{workingDirectory}:{workingDirectory}" }
                    }
                });

                await client.Containers.StartContainerAsync(createResponse.ID, null);

                bool isGitClone = command.Contains("clone");

                if (isGitClone)
                {
                    await WaitForGitCloneCompletion(client, createResponse.ID);
                }
                else
                {
                    await client.Containers.WaitContainerAsync(createResponse.ID);
                }

                var logs = await client.Containers.GetContainerLogsAsync(
                    createResponse.ID,true,
                    new ContainerLogsParameters { ShowStdout = true, ShowStderr = true });

                var (stdout, stderr) = await logs.ReadOutputToEndAsync(cancellationToken: CancellationToken.None);

                var inspectResponse = await client.Containers.InspectContainerAsync(createResponse.ID);
                var exitCode = inspectResponse.State.ExitCode;

                await client.Containers.RemoveContainerAsync(createResponse.ID, new ContainerRemoveParameters());

                if (exitCode == 0)
                {
                    return (true, $"Comando executado com sucesso: {command}. Output: {stdout}");
                }
                else
                {
                    return (false, $"Falha ao executar comando. Código de status: {exitCode}. Stdout: {stdout} Stderr: {stderr}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao executar comando: {ex.Message}");
            }
        }

        /// <summary>
        /// Lista o conteúdo de um diretório para debug
        /// </summary>
        private async Task<string> ListDirectoryContentsAsync(DockerClient client, string directory)
        {
            try
            {
                // Usar imagem Alpine padrão para comandos shell
                var alpineResult = await EnsureImageAsync(client, "alpine:latest");
                if (!alpineResult.Success)
                {
                    return $"Erro ao garantir imagem Alpine: {alpineResult.Message}";
                }

                var createResponse = await client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = "alpine:latest",
                    WorkingDir = directory,
                    Cmd = new[] { "ls", "-la" },
                    HostConfig = new HostConfig
                    {
                        Binds = new[] { $"{directory}:{directory}" }
                    }
                });

                await client.Containers.StartContainerAsync(createResponse.ID, null);
                var waitResponse = await client.Containers.WaitContainerAsync(createResponse.ID);

                var logs = await client.Containers.GetContainerLogsAsync(
                    createResponse.ID, true,
                    new ContainerLogsParameters { ShowStdout = true, ShowStderr = true });

                await client.Containers.RemoveContainerAsync(createResponse.ID, new ContainerRemoveParameters());

                var (stdout, stderr) = await logs.ReadOutputToEndAsync(cancellationToken: CancellationToken.None);
                return stdout.ToString();
            }
            catch (Exception ex)
            {
                return $"Erro ao listar diretório: {ex.Message}";
            }
        }

        /// <summary>
        /// Procura por arquivos docker-compose no diretório, priorizando a raiz
        /// </summary>
        private async Task<(bool Success, string Message)> FindDockerComposeFileAsync(DockerClient client, string directory)
        {
            try
            {
                // Usar imagem Alpine padrão para comandos shell
                var alpineResult = await EnsureImageAsync(client, "alpine:latest");
                if (!alpineResult.Success)
                {
                    return (false, $"Erro ao garantir imagem Alpine: {alpineResult.Message}");
                }

                // Primeiro, verificar se existe na raiz (prioridade máxima)
                var rootFiles = new[] { "docker-compose.yml", "docker-compose.yaml", "compose.yml", "compose.yaml" };

                foreach (var fileName in rootFiles)
                {
                    var checkResult = await CheckFileExistsAsync(client, directory, fileName);
                    if (checkResult.Success)
                    {
                        return (true, fileName);
                    }
                }

                // Se não encontrou na raiz, procurar em subdiretórios
                var createResponse = await client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = "alpine:latest",
                    WorkingDir = directory,
                    Cmd = new[] { "sh", "-c", "find . -maxdepth 2 -name 'docker-compose.yml' -o -name 'docker-compose.yaml' -o -name 'compose.yml' -o -name 'compose.yaml' | head -1" },
                    HostConfig = new HostConfig
                    {
                        Binds = new[] { $"{directory}:{directory}" }
                    }
                });

                await client.Containers.StartContainerAsync(createResponse.ID, null);
                var waitResponse = await client.Containers.WaitContainerAsync(createResponse.ID);

                var logs = await client.Containers.GetContainerLogsAsync(
                    createResponse.ID, true,
                    new ContainerLogsParameters { ShowStdout = true, ShowStderr = true });

                await client.Containers.RemoveContainerAsync(createResponse.ID, new ContainerRemoveParameters());

                var (stdout, stderr) = await logs.ReadOutputToEndAsync(cancellationToken: CancellationToken.None);
                var foundFile = stdout.ToString().Trim();

                if (!string.IsNullOrEmpty(foundFile) && foundFile != ".")
                {
                    // Remover o "./" do início se presente
                    foundFile = foundFile.StartsWith("./") ? foundFile.Substring(2) : foundFile;
                    return (true, foundFile);
                }

                return (false, "Nenhum arquivo docker-compose encontrado");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao procurar arquivo docker-compose: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se um arquivo específico existe no diretório
        /// </summary>
        private async Task<(bool Success, string Message)> CheckFileExistsAsync(DockerClient client, string directory, string fileName)
        {
            try
            {
                // Usar imagem Alpine padrão para comandos shell
                var alpineResult = await EnsureImageAsync(client, "alpine:latest");
                if (!alpineResult.Success)
                {
                    return (false, $"Erro ao garantir imagem Alpine: {alpineResult.Message}");
                }

                var createResponse = await client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = "alpine:latest",
                    WorkingDir = directory,
                    Cmd = new[] { "ls", "-la", fileName },
                    HostConfig = new HostConfig
                    {
                        Binds = new[] { $"{directory}:{directory}" }
                    }
                });

                await client.Containers.StartContainerAsync(createResponse.ID, null);
                var waitResponse = await client.Containers.WaitContainerAsync(createResponse.ID);

                var logs = await client.Containers.GetContainerLogsAsync(
                    createResponse.ID, true,
                    new ContainerLogsParameters { ShowStdout = true, ShowStderr = true });

                await client.Containers.RemoveContainerAsync(createResponse.ID, new ContainerRemoveParameters());

                var (stdout, stderr) = await logs.ReadOutputToEndAsync(cancellationToken: CancellationToken.None);
                var result = stdout.ToString().Trim();

                if (waitResponse.StatusCode == 0 && !result.Contains("No such file or directory"))
                {
                    return (true, $"Arquivo {fileName} encontrado");
                }
                else
                {
                    return (false, $"Arquivo {fileName} não encontrado no diretório {directory}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao verificar arquivo: {ex.Message}");
            }
        }

        /// <summary>
        /// Aguarda especificamente o git clone terminar, verificando os logs
        /// </summary>
        private async Task WaitForGitCloneCompletion(DockerClient client, string containerId)
        {
            var maxWaitTime = TimeSpan.FromMinutes(5); // Timeout de 5 minutos
            var startTime = DateTime.Now;
            var checkInterval = TimeSpan.FromSeconds(2);

            while (DateTime.Now - startTime < maxWaitTime)
            {
                try
                {
                    // Verificar se o container ainda está rodando
                    var inspectResponse = await client.Containers.InspectContainerAsync(containerId);

                    if (!inspectResponse.State.Running)
                    {
                        // Container parou, verificar se foi por conclusão ou erro
                        break;
                    }

                    // Verificar os logs para ver se o clone terminou
                    var logs = await client.Containers.GetContainerLogsAsync(
                        containerId, true,
                        new ContainerLogsParameters { ShowStdout = true, ShowStderr = true });

                    var (stdout, stderr) = await logs.ReadOutputToEndAsync(cancellationToken: CancellationToken.None);
                    var logContent = stdout.ToString() + stderr.ToString();

                    // Verificar se há indicações de que o clone terminou
                    if (logContent.Contains("Resolving deltas:") && logContent.Contains("done.") ||
                        logContent.Contains("Already up to date") ||
                        logContent.Contains("fatal:") ||
                        logContent.Contains("error:"))
                    {
                        // Clone terminou (sucesso ou erro)
                        break;
                    }

                    // Aguardar antes da próxima verificação
                    await Task.Delay(checkInterval);
                }
                catch (Exception)
                {
                    // Se houver erro ao verificar, aguardar um pouco mais
                    await Task.Delay(checkInterval);
                }
            }

            // Aguardar um pouco mais para garantir que tudo foi escrito
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}
