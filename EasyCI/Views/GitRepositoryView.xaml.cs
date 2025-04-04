using EasyCI.Models;
using EasyCI.ViewModels;
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
        }

        public GitRepositoryView(GitRepository repository)
        {
            InitializeComponent();
            _viewModel = new GitRepositoryViewModel(repository);
            DataContext = _viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null)
            {
                _viewModel = new GitRepositoryViewModel();
                DataContext = _viewModel;
            }
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
    }
}
