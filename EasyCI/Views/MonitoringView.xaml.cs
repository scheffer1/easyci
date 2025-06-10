using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using EasyCI.Models;
using EasyCI.Services;

namespace EasyCI.Views
{
    /// <summary>
    /// Interaction logic for MonitoringView.xaml
    /// </summary>
    public partial class MonitoringView : Window
    {
        private readonly CIProjectService _ciProjectService;
        private readonly BuildService _buildService;
        private readonly ObservableCollection<CIProject> _projects;
        private readonly ObservableCollection<CIProject> _filteredProjects;
        private readonly DispatcherTimer _refreshTimer;

        public MonitoringView()
        {
            InitializeComponent();

            _ciProjectService = new CIProjectService();
            _buildService = new BuildService();
            _projects = new ObservableCollection<CIProject>();
            _filteredProjects = new ObservableCollection<CIProject>();

            // Configurar timer para atualização automática a cada 30 segundos
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;

            DgProjects.ItemsSource = _filteredProjects;

            // Configurar eventos
            TxtFilter.TextChanged += TxtFilter_TextChanged;
            CmbStatusFilter.SelectionChanged += CmbStatusFilter_SelectionChanged;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProjectsAsync();
            _refreshTimer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _refreshTimer?.Stop();
        }

        private async void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            await LoadProjectsAsync();
        }

        private async System.Threading.Tasks.Task LoadProjectsAsync()
        {
            try
            {
                var projects = await _ciProjectService.GetAllAsync();

                _projects.Clear();
                foreach (var project in projects)
                {
                    _projects.Add(project);
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar projetos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            var filteredList = _projects.AsEnumerable();

            // Filtro por texto
            if (!string.IsNullOrWhiteSpace(TxtFilter.Text))
            {
                var filterText = TxtFilter.Text.ToLower();
                filteredList = filteredList.Where(p =>
                    p.Name.ToLower().Contains(filterText) ||
                    (p.GitRepository?.Name?.ToLower().Contains(filterText) ?? false) ||
                    (p.DockerContainer?.Name?.ToLower().Contains(filterText) ?? false));
            }

            // Filtro por status
            if (CmbStatusFilter.SelectedIndex > 0)
            {
                var selectedStatus = ((ComboBoxItem)CmbStatusFilter.SelectedItem).Content.ToString();
                filteredList = filteredList.Where(p => GetStatusCategory(p.Status) == selectedStatus);
            }

            _filteredProjects.Clear();
            foreach (var project in filteredList)
            {
                _filteredProjects.Add(project);
            }
        }

        private string GetStatusCategory(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "Não Iniciado";

            status = status.ToLower();

            if (status.Contains("erro") || status.Contains("falha") || status.Contains("failed"))
                return "Falha";
            else if (status.Contains("executando") || status.Contains("running") || status.Contains("em andamento"))
                return "Em Execução";
            else if (status.Contains("sucesso") || status.Contains("concluído") || status.Contains("completed"))
                return "Concluído";
            else
                return "Não Iniciado";
        }

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Voltar para a tela principal
            Close();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadProjectsAsync();
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var project = button?.DataContext as CIProject;

            if (project != null)
            {
                var result = MessageBox.Show(
                    $"Deseja iniciar o build do projeto '{project.Name}'?",
                    "Confirmar Build",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        button!.IsEnabled = false;
                        button.Content = "Executando...";

                        var buildResult = await _buildService.ExecuteBuildAsync(project.Id);

                        await LoadProjectsAsync();

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
                    }
                    finally
                    {
                        button.IsEnabled = true;
                        button.Content = "Iniciar";
                    }
                }
            }
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var project = button?.DataContext as CIProject;

            if (project != null)
            {
                var buildDetailsView = new BuildDetailsView(project.Id);
                buildDetailsView.ShowDialog();
            }
        }
    }
}
