using EasyCI.Models;
using EasyCI.Services;
using EasyCI.Themes;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasyCI.Views
{
    /// <summary>
    /// Interaction logic for CIProjectListView.xaml
    /// </summary>
    public partial class CIProjectListView : Window
    {
        private readonly CIProjectService _ciProjectService;
        private readonly BuildService _buildService;
        private ObservableCollection<CIProject> _projects;

        public CIProjectListView()
        {
            InitializeComponent();
            _ciProjectService = new CIProjectService();
            _buildService = new BuildService();
            _projects = new ObservableCollection<CIProject>();

            // Registrar para eventos de mudança de tema
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object? sender, ThemeType theme)
        {
            // Forçar atualização dos estilos quando o tema mudar
            InvalidateVisual();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProjectsAsync();
        }

        private async System.Threading.Tasks.Task LoadProjectsAsync()
        {
            try
            {
                TxtStatus.Text = "Carregando projetos...";
                var projects = await _ciProjectService.GetAllAsync();

                _projects.Clear();
                foreach (var project in projects)
                {
                    _projects.Add(project);
                }

                DgProjects.ItemsSource = _projects;
                TxtStatus.Text = $"Projetos carregados: {_projects.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar projetos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Erro ao carregar projetos";
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var ciProjectView = new CIProjectView();
            if (ciProjectView.ShowDialog() == true)
            {
                // Recarregar a lista após adicionar
                _ = LoadProjectsAsync();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedProject();
        }

        private void DgProjects_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                EditSelectedProject();
            }
        }

        private void EditSelectedProject()
        {
            var selectedProject = DgProjects.SelectedItem as CIProject;
            if (selectedProject != null)
            {
                var ciProjectView = new CIProjectView(selectedProject);
                if (ciProjectView.ShowDialog() == true)
                {
                    // Recarregar a lista após editar
                    _ = LoadProjectsAsync();
                }
            }
            else
            {
                MessageBox.Show("Selecione um projeto para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedProject = DgProjects.SelectedItem as CIProject;
            if (selectedProject != null)
            {
                var result = MessageBox.Show($"Tem certeza que deseja excluir o projeto '{selectedProject.Name}'?",
                    "Confirmar Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        TxtStatus.Text = "Excluindo projeto...";
                        var success = await _ciProjectService.DeleteAsync(selectedProject.Id);

                        if (success)
                        {
                            // Recarregar a lista após excluir
                            await LoadProjectsAsync();
                            MessageBox.Show("Projeto excluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Não foi possível excluir o projeto.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                            TxtStatus.Text = "Erro ao excluir projeto";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao excluir projeto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        TxtStatus.Text = "Erro ao excluir projeto";
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecione um projeto para excluir.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadProjectsAsync();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Voltar para a tela principal
            Close();
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            var selectedProject = DgProjects.SelectedItem as CIProject;
            if (selectedProject != null)
            {
                var buildDetailsView = new BuildDetailsView(selectedProject.Id);
                buildDetailsView.ShowDialog();
            }
            else
            {
                MessageBox.Show("Selecione um projeto para visualizar os detalhes.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnBuild_Click(object sender, RoutedEventArgs e)
        {
            var selectedProject = DgProjects.SelectedItem as CIProject;
            if (selectedProject != null)
            {
                var result = MessageBox.Show($"Deseja executar o build do projeto '{selectedProject.Name}'?",
                    "Confirmar Build", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Desabilitar o botão durante o build
                        BtnBuild.IsEnabled = false;
                        TxtStatus.Text = $"Executando build do projeto '{selectedProject.Name}'...";

                        // Executar o build
                        var buildResult = await _buildService.ExecuteBuildAsync(selectedProject.Id);

                        // Recarregar a lista para atualizar o status
                        await LoadProjectsAsync();

                        // Exibir o resultado
                        if (buildResult.Success)
                        {
                            MessageBox.Show(buildResult.Message, "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(buildResult.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao executar build: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        TxtStatus.Text = "Erro ao executar build";
                    }
                    finally
                    {
                        // Reabilitar o botão
                        BtnBuild.IsEnabled = true;
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecione um projeto para executar o build.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
