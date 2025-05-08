using EasyCI.Models;
using EasyCI.Services;
using EasyCI.ViewModels;
using Microsoft.Win32;
using System.Windows;

namespace EasyCI.Views
{
    public partial class DockerContainerView : Window
    {
        private DockerContainerViewModel _viewModel;
        private readonly DockerApiService _dockerApiService = new();

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

            // Definir UseDockerApi como true por padrão
            _viewModel.UseDockerApi = true;
        }

        private void BtnSelectCertificate_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectCertificate();
        }

        private async void BtnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.Host))
            {
                MessageBox.Show("O host do container é obrigatório para testar a conexão.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtHost.Focus();
                return;
            }

            if (_viewModel.Port <= 0)
            {
                MessageBox.Show("A porta deve ser um número válido maior que zero.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPort.Focus();
                return;
            }

            var tempContainer = new DockerContainer
            {
                Host = _viewModel.Host,
                Port = _viewModel.Port,
                ApiVersion = _viewModel.ApiVersion,
                UseTLS = _viewModel.UseTLS,
                CertificatePath = _viewModel.CertificatePath,
                UseDockerApi = true,
                RemoteWorkspacePath = _viewModel.RemoteWorkspacePath
            };

            try
            {
                var result = await _dockerApiService.TestConnectionAsync(tempContainer);

                if (result.Success)
                {
                    MessageBox.Show($"Conexão bem-sucedida!\n\n{result.Message}", "Teste de Conexão", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Falha na conexão:\n\n{result.Message}", "Teste de Conexão", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erro ao testar conexão: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
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

            if (_viewModel.Port <= 0)
            {
                MessageBox.Show("A porta deve ser um número válido maior que zero.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPort.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.RemoteWorkspacePath))
            {
                MessageBox.Show("O caminho do workspace remoto é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtRemoteWorkspacePath.Focus();
                return;
            }

            _viewModel.UseDockerApi = true;

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

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
