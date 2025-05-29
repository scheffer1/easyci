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
    /// Interaction logic for GitRepositoryListView.xaml
    /// </summary>
    public partial class GitRepositoryListView : Window
    {
        private readonly GitRepositoryService _gitRepositoryService;
        private ObservableCollection<GitRepository> _repositories;

        public GitRepositoryListView()
        {
            InitializeComponent();
            _gitRepositoryService = new GitRepositoryService();
            _repositories = new ObservableCollection<GitRepository>();

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
            await LoadRepositoriesAsync();
        }

        private async System.Threading.Tasks.Task LoadRepositoriesAsync()
        {
            try
            {
                TxtStatus.Text = "Carregando repositórios...";
                var repositories = await _gitRepositoryService.GetAllAsync();

                _repositories.Clear();
                foreach (var repo in repositories)
                {
                    _repositories.Add(repo);
                }

                DgRepositories.ItemsSource = _repositories;
                TxtStatus.Text = $"Repositórios carregados: {_repositories.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar repositórios: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Erro ao carregar repositórios";
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var gitRepositoryView = new GitRepositoryView();
            if (gitRepositoryView.ShowDialog() == true)
            {
                // Recarregar a lista após adicionar
                _ = LoadRepositoriesAsync();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedRepository();
        }

        private void DgRepositories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                EditSelectedRepository();
            }
        }

        private void EditSelectedRepository()
        {
            var selectedRepository = DgRepositories.SelectedItem as GitRepository;
            if (selectedRepository != null)
            {
                var gitRepositoryView = new GitRepositoryView(selectedRepository);
                if (gitRepositoryView.ShowDialog() == true)
                {
                    // Recarregar a lista após editar
                    _ = LoadRepositoriesAsync();
                }
            }
            else
            {
                MessageBox.Show("Selecione um repositório para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedRepository = DgRepositories.SelectedItem as GitRepository;
            if (selectedRepository != null)
            {
                var result = MessageBox.Show($"Tem certeza que deseja excluir o repositório '{selectedRepository.Name}'?",
                    "Confirmar Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        TxtStatus.Text = "Excluindo repositório...";
                        var success = await _gitRepositoryService.DeleteAsync(selectedRepository.Id);

                        if (success)
                        {
                            // Recarregar a lista após excluir
                            await LoadRepositoriesAsync();
                            MessageBox.Show("Repositório excluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Não foi possível excluir o repositório.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                            TxtStatus.Text = "Erro ao excluir repositório";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao excluir repositório: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        TxtStatus.Text = "Erro ao excluir repositório";
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecione um repositório para excluir.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadRepositoriesAsync();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Voltar para a tela principal
            Close();
        }
    }
}
