using EasyCI.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EasyCI.Services
{
    /// <summary>
    /// Serviço para gerenciar builds de projetos CI
    /// </summary>
    public class BuildService
    {
        private readonly GitService _gitService;
        private readonly CIProjectService _ciProjectService;
        private readonly string _workspacePath;

        public BuildService()
        {
            _gitService = new GitService();
            _ciProjectService = new CIProjectService();

            // Definir o diretório de trabalho para os projetos
            _workspacePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EasyCI",
                "Workspace");

            // Garantir que o diretório existe
            Directory.CreateDirectory(_workspacePath);
        }

        /// <summary>
        /// Executa o build de um projeto CI
        /// </summary>
        /// <param name="projectId">ID do projeto CI</param>
        /// <returns>Resultado da operação de build</returns>
        public async Task<(bool Success, string Message)> ExecuteBuildAsync(int projectId)
        {
            try
            {
                // Obter o projeto
                var project = await _ciProjectService.GetByIdAsync(projectId);
                if (project == null)
                {
                    return (false, "Projeto não encontrado");
                }

                // Verificar se o projeto está ativo
                if (!project.IsActive)
                {
                    return (false, "O projeto está inativo");
                }

                // Verificar se o repositório Git está disponível
                if (project.GitRepository == null)
                {
                    return (false, "Repositório Git não configurado para este projeto");
                }

                // Verificar se o container Docker está disponível
                if (project.DockerContainer == null)
                {
                    return (false, "Container Docker não configurado para este projeto");
                }

                // Definir o caminho local para o repositório
                string projectPath = Path.Combine(_workspacePath, $"project_{project.Id}");
                string logPath = Path.Combine(projectPath, "build.log");

                // Garantir que o diretório do projeto existe
                Directory.CreateDirectory(projectPath);

                // Iniciar o log
                await LogAsync(logPath, $"[{DateTime.Now}] Iniciando build do projeto {project.Name}\n");
                await LogAsync(logPath, $"[{DateTime.Now}] Repositório Git: {project.GitRepository.Name} ({project.GitRepository.Url})\n");
                await LogAsync(logPath, $"[{DateTime.Now}] Branch: {project.GitRepository.Branch}\n");
                await LogAsync(logPath, $"[{DateTime.Now}] Container Docker: {project.DockerContainer.Name}\n");

                // Atualizar o status do projeto
                project.Status = "Clonando repositório...";
                await _ciProjectService.UpdateAsync(project);
                await LogAsync(logPath, $"[{DateTime.Now}] Status atualizado: {project.Status}\n");

                // Clonar o repositório
                await LogAsync(logPath, $"[{DateTime.Now}] Clonando repositório para {projectPath}...\n");
                var cloneResult = await _gitService.CloneRepositoryAsync(project.GitRepository, projectPath);
                await LogAsync(logPath, $"[{DateTime.Now}] Resultado do clone: {cloneResult.Message}\n");

                if (!cloneResult.Success)
                {
                    project.Status = $"Falha: {cloneResult.Message}";
                    await _ciProjectService.UpdateAsync(project);
                    await LogAsync(logPath, $"[{DateTime.Now}] Status atualizado: {project.Status}\n");
                    return cloneResult;
                }

                // Atualizar o status e a data do último build
                project.Status = "Repositório clonado com sucesso";
                project.LastBuildDate = DateTime.Now;
                await _ciProjectService.UpdateAsync(project);
                await LogAsync(logPath, $"[{DateTime.Now}] Status atualizado: {project.Status}\n");
                await LogAsync(logPath, $"[{DateTime.Now}] Data do último build atualizada: {project.LastBuildDate}\n");

                // Verificar se o arquivo docker-compose.yml existe
                string composeFilePath = Path.Combine(projectPath, project.ComposeFilePath);
                if (File.Exists(composeFilePath))
                {
                    await LogAsync(logPath, $"[{DateTime.Now}] Arquivo docker-compose encontrado: {composeFilePath}\n");
                    await LogAsync(logPath, $"[{DateTime.Now}] A execução do Docker Compose será implementada em uma etapa futura.\n");
                }
                else
                {
                    await LogAsync(logPath, $"[{DateTime.Now}] Arquivo docker-compose não encontrado: {composeFilePath}\n");
                }

                await LogAsync(logPath, $"[{DateTime.Now}] Build concluído com sucesso.\n");
                return (true, "Build iniciado com sucesso. Repositório clonado.");
            }
            catch (Exception ex)
            {
                string projectPath = Path.Combine(_workspacePath, $"project_{projectId}");
                string logPath = Path.Combine(projectPath, "build.log");

                try
                {
                    await LogAsync(logPath, $"[{DateTime.Now}] ERRO: {ex.Message}\n");
                    if (ex.StackTrace != null)
                    {
                        await LogAsync(logPath, $"[{DateTime.Now}] Stack Trace: {ex.StackTrace}\n");
                    }
                }
                catch
                {
                    // Ignorar erros ao tentar logar o erro
                }

                return (false, $"Erro ao executar build: {ex.Message}");
            }
        }

        /// <summary>
        /// Adiciona uma entrada ao arquivo de log
        /// </summary>
        /// <param name="logPath">Caminho do arquivo de log</param>
        /// <param name="message">Mensagem a ser registrada</param>
        private async Task LogAsync(string logPath, string message)
        {
            try
            {
                // Garantir que o diretório do log existe
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));

                // Adicionar a mensagem ao log
                await File.AppendAllTextAsync(logPath, message);
            }
            catch
            {
                // Ignorar erros de log para não interromper o processo principal
            }
        }

        /// <summary>
        /// Obtém o caminho do workspace para um projeto específico
        /// </summary>
        /// <param name="projectId">ID do projeto</param>
        /// <returns>Caminho do workspace</returns>
        public string GetProjectWorkspacePath(int projectId)
        {
            return Path.Combine(_workspacePath, $"project_{projectId}");
        }
    }
}
