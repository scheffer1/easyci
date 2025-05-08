using EasyCI.Models;
using EasyCI.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace EasyCI.Views
{
    /// <summary>
    /// Interaction logic for BuildDetailsView.xaml
    /// </summary>
    public partial class BuildDetailsView : Window
    {
        private readonly CIProjectService _ciProjectService;
        private readonly BuildService _buildService;
        private CIProject _project;
        private int _projectId;

        public BuildDetailsView(int projectId)
        {
            InitializeComponent();
            _ciProjectService = new CIProjectService();
            _buildService = new BuildService();
            _projectId = projectId;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Carregar o projeto
                _project = await _ciProjectService.GetByIdAsync(_projectId);
                if (_project == null)
                {
                    MessageBox.Show("Projeto não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // Atualizar a interface
                UpdateUI();

                // Carregar logs
                LoadLogs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar detalhes do projeto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUI()
        {
            // Atualizar título e descrição
            TxtTitle.Text = $"Detalhes do Build - {_project.Name}";
            TxtDescription.Text = $"Informações detalhadas sobre o build do projeto {_project.Name}.";

            // Atualizar informações do projeto
            TxtProjectName.Text = _project.Name;
            TxtStatus.Text = _project.Status;
            TxtRepository.Text = _project.GitRepository?.Name ?? "-";
            TxtContainer.Text = _project.DockerContainer?.Name ?? "-";
            TxtBranch.Text = _project.GitRepository?.Branch ?? "-";
            TxtLastBuild.Text = _project.LastBuildDate.Year > 1 ? _project.LastBuildDate.ToString("dd/MM/yyyy HH:mm:ss") : "Nunca executado";
        }

        private void LoadLogs()
        {
            try
            {
                string workspacePath = _buildService.GetProjectWorkspacePath(_projectId);
                string logPath = Path.Combine(workspacePath, "build.log");

                if (File.Exists(logPath))
                {
                    TxtLogs.Text = File.ReadAllText(logPath);
                    TxtLogs.ScrollToEnd();
                }
                else
                {
                    TxtLogs.Text = "Nenhum log disponível.";
                }
            }
            catch (Exception ex)
            {
                TxtLogs.Text = $"Erro ao carregar logs: {ex.Message}";
            }
        }

        private void BtnClearLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string workspacePath = _buildService.GetProjectWorkspacePath(_projectId);
                string logPath = Path.Combine(workspacePath, "build.log");

                if (File.Exists(logPath))
                {
                    File.WriteAllText(logPath, string.Empty);
                    TxtLogs.Text = "Logs limpos.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao limpar logs: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefreshLogs_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        private void BtnOpenWorkspace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string workspacePath = _buildService.GetProjectWorkspacePath(_projectId);

                if (Directory.Exists(workspacePath))
                {
                    Process.Start("explorer.exe", workspacePath);
                }
                else
                {
                    MessageBox.Show("O diretório de workspace ainda não foi criado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir workspace: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Voltar para a tela anterior (mesma ação do Fechar)
            Close();
        }
    }
}
