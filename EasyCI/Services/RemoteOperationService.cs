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
                    // Para SSH, ainda precisamos usar shell para definir variáveis de ambiente
                    gitCommand = $"/bin/sh -c \"export GIT_SSH_COMMAND='ssh -i {repository.SshKeyPath} -o StrictHostKeyChecking=no' && git pull\"";
                }
                else
                {
                    // Para HTTPS com credenciais, configurar via git config
                    if (!string.IsNullOrEmpty(repository.Username) && !string.IsNullOrEmpty(repository.Password))
                    {
                        // Usar shell para configurar credenciais temporariamente
                        gitCommand = $"/bin/sh -c \"git config credential.helper store && echo 'https://{repository.Username}:{repository.Password}@{GetHostFromUrl(repository.Url)}' > ~/.git-credentials && git pull && rm ~/.git-credentials\"";
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
    }
}
