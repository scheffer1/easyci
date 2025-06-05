using EasyCI.Models;
using EasyCI.ViewModels;
using EasyCI.Themes;
using Microsoft.Win32;
using System.Windows;

namespace EasyCI.Views
{
    /// <summary>
    /// Interaction logic for GitRepositoryView.xaml
    /// </summary>
    public partial class GitRepositoryView : Window
    {
        private GitRepositoryViewModel _viewModel;

        public GitRepositoryView()
        {
            InitializeComponent();

            // Registrar para eventos de mudança de tema
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        public GitRepositoryView(GitRepository repository)
        {
            InitializeComponent();
            _viewModel = new GitRepositoryViewModel(repository);
            DataContext = _viewModel;

            // Registrar para eventos de mudança de tema
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null)
            {
                _viewModel = new GitRepositoryViewModel();
                DataContext = _viewModel;
            }

            // Configurar a senha no PasswordBox
            TxtPassword.Password = _viewModel.Password;
        }

        private void BtnSelectSshKey_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectSshKey();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validação básica
            if (string.IsNullOrWhiteSpace(_viewModel.Name))
            {
                MessageBox.Show("O nome do repositório é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.Url))
            {
                MessageBox.Show("A URL do repositório é obrigatória.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtUrl.Focus();
                return;
            }

            // Capturar a senha do PasswordBox
            _viewModel.Password = TxtPassword.Password;

            // Salvar o repositório
            if (await _viewModel.SaveAsync())
            {
                MessageBox.Show("Repositório salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Voltar para a tela anterior (mesma ação do Cancelar)
            DialogResult = false;
            Close();
        }

        private void OnThemeChanged(object? sender, ThemeType theme)
        {
            // Forçar atualização dos estilos quando o tema mudar
            InvalidateVisual();
        }
    }
}
