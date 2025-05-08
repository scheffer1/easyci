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

                // TODO: Implementar clonagem de repositório via Docker API
                return (true, $"Repositório clonado com sucesso em {projectPath} via Docker API (simulado)");
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
                await Task.Delay(0);

                // TODO: Implementar atualização de repositório via Docker API
                return (true, $"Repositório atualizado com sucesso em {projectPath} via Docker API (simulado)");
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
    }
}
