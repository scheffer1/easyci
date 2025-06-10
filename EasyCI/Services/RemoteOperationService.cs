using EasyCI.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EasyCI.Services
{
    public class RemoteOperationService
    {
        private readonly DockerApiService _dockerApiService;

        public RemoteOperationService()
        {
            _dockerApiService = new DockerApiService();
        }

        public async Task<(bool Success, string Message)> CloneRepositoryAsync(DockerContainer container, GitRepository repository, int projectId)
        {
            try
            {
                var projectPath = $"{container.RemoteWorkspacePath}/project_{projectId}";
                var createDirResult = await _dockerApiService.CreateDirectoryAsync(container, projectPath);

                if (!createDirResult.Success)
                {
                    return (false, $"Falha ao criar diretório remoto via Docker API: {createDirResult.Message}");
                }

                // Verificar se o diretório já contém um repositório git
                var gitCheckResult = await CheckIfGitRepositoryExistsAsync(container, projectPath);

                if (gitCheckResult.Success)
                {
                    // Diretório já contém um repositório git, fazer pull em vez de clone
                    return await UpdateRepositoryAsync(container, repository, projectId);
                }

                // Verificar se o diretório não está vazio
                var dirCheckResult = await CheckIfDirectoryIsEmptyAsync(container, projectPath);
                if (!dirCheckResult.Success)
                {
                    // Diretório não está vazio mas não é um repositório git, limpar primeiro
                    var cleanResult = await CleanDirectoryAsync(container, projectPath);
                    if (!cleanResult.Success)
                    {
                        return (false, $"Falha ao limpar diretório existente: {cleanResult.Message}");
                    }
                }

                string gitCommand;

                if (!string.IsNullOrEmpty(repository.SshKeyPath))
                {
                    // Para SSH, ainda precisamos usar shell para definir variáveis de ambiente
                    gitCommand = $"/bin/sh -c \"export GIT_SSH_COMMAND='ssh -i {repository.SshKeyPath} -o StrictHostKeyChecking=no' && git clone {repository.Url} --branch {repository.Branch} --single-branch .\"";
                }
                else
                {
                    // Para HTTPS, incluir credenciais na URL se disponíveis
                    string urlWithCredentials = repository.Url;
                    if (!string.IsNullOrEmpty(repository.Username) && !string.IsNullOrEmpty(repository.Password))
                    {
                        if (repository.Url.StartsWith("https://"))
                        {
                            urlWithCredentials = repository.Url.Replace("https://", $"https://{repository.Username}:{repository.Password}@");
                        }
                        else if (repository.Url.StartsWith("http://"))
                        {
                            urlWithCredentials = repository.Url.Replace("http://", $"http://{repository.Username}:{repository.Password}@");
                        }
                    }

                    gitCommand = $"clone {urlWithCredentials} --branch {repository.Branch} --single-branch .";
                }

                var cloneResult = await _dockerApiService.ExecuteCommandAsync(
                    container,
                    projectPath,
                    gitCommand);

                if (!cloneResult.Success)
                {
                    return (false, $"Falha ao clonar repositório: {cloneResult.Message}");
                }

                return (true, $"Repositório clonado com sucesso em {projectPath}");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao clonar repositório: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateRepositoryAsync(DockerContainer container, GitRepository repository, int projectId)
        {
            try
            {
                var projectPath = $"{container.RemoteWorkspacePath}/project_{projectId}";

                // Construir o comando Git para atualizar o repositório
                string gitCommand;

                if (!string.IsNullOrEmpty(repository.SshKeyPath))
                {
                    // Para SSH, usar variável de ambiente
                    gitCommand = $"GIT_SSH_COMMAND='ssh -i {repository.SshKeyPath} -o StrictHostKeyChecking=no' git pull";
                }
                else
                {
                    // Para HTTPS com credenciais, usar URL com credenciais
                    if (!string.IsNullOrEmpty(repository.Username) && !string.IsNullOrEmpty(repository.Password))
                    {
                        // Construir URL com credenciais
                        string urlWithCredentials = repository.Url;
                        if (repository.Url.StartsWith("https://"))
                        {
                            urlWithCredentials = repository.Url.Replace("https://", $"https://{repository.Username}:{repository.Password}@");
                        }
                        else if (repository.Url.StartsWith("http://"))
                        {
                            urlWithCredentials = repository.Url.Replace("http://", $"http://{repository.Username}:{repository.Password}@");
                        }

                        gitCommand = $"pull {urlWithCredentials}";
                    }
                    else
                    {
                        // Para HTTPS sem credenciais, usar comando simples
                        gitCommand = "pull";
                    }
                }

                // Executar o comando Git via Docker API
                var updateResult = await _dockerApiService.ExecuteCommandAsync(
                    container,
                    projectPath,
                    gitCommand);

                if (!updateResult.Success)
                {
                    return (false, $"Falha ao atualizar repositório: {updateResult.Message}");
                }

                return (true, $"Repositório atualizado com sucesso em {projectPath}");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao atualizar repositório: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ExecuteDockerComposeAsync(DockerContainer container, int projectId, string composeFilePath)
        {
            try
            {
                var projectPath = $"{container.RemoteWorkspacePath}/project_{projectId}";
                var composePath = Path.Combine(projectPath, composeFilePath).Replace("\\", "/");

                var composeResult = await _dockerApiService.ExecuteDockerComposeAsync(
                    container,
                    projectPath,
                    composeFilePath);

                return composeResult;
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao executar Docker Compose: {ex.Message}");
            }
        }

        /// <summary>
        /// Extrai o host de uma URL Git
        /// </summary>
        private string GetHostFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                // Se falhar, tentar extrair manualmente
                if (url.StartsWith("https://"))
                {
                    var hostPart = url.Substring(8);
                    var slashIndex = hostPart.IndexOf('/');
                    return slashIndex > 0 ? hostPart.Substring(0, slashIndex) : hostPart;
                }
                else if (url.StartsWith("http://"))
                {
                    var hostPart = url.Substring(7);
                    var slashIndex = hostPart.IndexOf('/');
                    return slashIndex > 0 ? hostPart.Substring(0, slashIndex) : hostPart;
                }
                return url;
            }
        }

        /// <summary>
        /// Verifica se o diretório já contém um repositório git
        /// </summary>
        private async Task<(bool Success, string Message)> CheckIfGitRepositoryExistsAsync(DockerContainer container, string directoryPath)
        {
            try
            {
                // Verificar se existe a pasta .git usando ls simples
                var gitCheckResult = await _dockerApiService.ExecuteCommandAsync(
                    container,
                    directoryPath,
                    "ls -la .git");

                if (gitCheckResult.Success && !gitCheckResult.Message.Contains("No such file or directory"))
                {
                    return (true, "Repositório git encontrado");
                }

                return (false, "Não é um repositório git");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao verificar repositório git: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se o diretório está vazio
        /// </summary>
        private async Task<(bool Success, string Message)> CheckIfDirectoryIsEmptyAsync(DockerContainer container, string directoryPath)
        {
            try
            {
                var listResult = await _dockerApiService.ExecuteCommandAsync(
                    container,
                    directoryPath,
                    "ls -A");

                if (listResult.Success)
                {
                    // Se não há output de arquivos, o diretório está vazio
                    var output = listResult.Message.Trim();
                    // Verificar se o output contém apenas a mensagem de sucesso sem listagem de arquivos
                    if (string.IsNullOrEmpty(output) ||
                        (output.Contains("Comando executado com sucesso") &&
                         !output.Contains(".") &&
                         !output.Contains("/")))
                    {
                        return (true, "Diretório está vazio");
                    }
                }

                return (false, "Diretório não está vazio");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao verificar se diretório está vazio: {ex.Message}");
            }
        }

        /// <summary>
        /// Limpa o conteúdo de um diretório
        /// </summary>
        private async Task<(bool Success, string Message)> CleanDirectoryAsync(DockerContainer container, string directoryPath)
        {
            try
            {
                // Primeiro, listar arquivos para ver o que existe
                var listResult = await _dockerApiService.ExecuteCommandAsync(
                    container,
                    directoryPath,
                    "ls -la");

                // Tentar remover arquivos visíveis
                var cleanResult = await _dockerApiService.ExecuteCommandAsync(
                    container,
                    directoryPath,
                    "rm -rf *");

                // Tentar remover arquivos ocultos (pode falhar, mas não é crítico)
                await _dockerApiService.ExecuteCommandAsync(
                    container,
                    directoryPath,
                    "rm -rf .*");

                return (true, "Diretório limpo com sucesso");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao limpar diretório: {ex.Message}");
            }
        }
    }
}
