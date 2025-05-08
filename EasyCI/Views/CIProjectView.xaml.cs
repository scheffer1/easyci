using EasyCI.Models;
using EasyCI.ViewModels;
using System.Windows;

namespace EasyCI.Views
{
    /// <summary>
    /// Interaction logic for CIProjectView.xaml
    /// </summary>
    public partial class CIProjectView : Window
    {
        private CIProjectViewModel _viewModel;

        public CIProjectView()
        {
            InitializeComponent();
        }

        public CIProjectView(CIProject project)
        {
            InitializeComponent();
            _viewModel = new CIProjectViewModel(project);
            DataContext = _viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null)
            {
                _viewModel = new CIProjectViewModel();
                DataContext = _viewModel;
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validação básica
            if (string.IsNullOrWhiteSpace(_viewModel.Name))
            {
                MessageBox.Show("O nome do projeto é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            if (_viewModel.SelectedGitRepository == null)
            {
                MessageBox.Show("Selecione um repositório Git.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                CmbGitRepository.Focus();
                return;
            }

            if (_viewModel.SelectedDockerContainer == null)
            {
                MessageBox.Show("Selecione um container Docker.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                CmbDockerContainer.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.ComposeFilePath))
            {
                MessageBox.Show("O caminho do arquivo Docker Compose é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtComposeFilePath.Focus();
                return;
            }

            // Salvar o projeto
            if (await _viewModel.SaveAsync())
            {
                MessageBox.Show("Projeto salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
