using EasyCI.Models;
using EasyCI.ViewModels;
using Microsoft.Win32;
using System.Windows;

namespace EasyCI.Views
{
    /// <summary>
    /// Interaction logic for DockerContainerView.xaml
    /// </summary>
    public partial class DockerContainerView : Window
    {
        private DockerContainerViewModel _viewModel;

        public DockerContainerView()
        {
            InitializeComponent();
        }

        public DockerContainerView(DockerContainer container)
        {
            InitializeComponent();
            _viewModel = new DockerContainerViewModel(container);
            DataContext = _viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null)
            {
                _viewModel = new DockerContainerViewModel();
                DataContext = _viewModel;
            }
        }

        private void BtnSelectCertificate_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectCertificate();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validação básica
            if (string.IsNullOrWhiteSpace(_viewModel.Name))
            {
                MessageBox.Show("O nome do container é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.Host))
            {
                MessageBox.Show("O host do container é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtHost.Focus();
                return;
            }

            // Validar se a porta é um número válido
            if (_viewModel.Port <= 0)
            {
                MessageBox.Show("A porta deve ser um número válido maior que zero.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPort.Focus();
                return;
            }

            // Salvar o container
            if (await _viewModel.SaveAsync())
            {
                MessageBox.Show("Container salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
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
