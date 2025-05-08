using EasyCI.Models;
using EasyCI.Services;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace EasyCI.ViewModels
{
    public class DockerContainerViewModel : INotifyPropertyChanged
    {
        private readonly DockerContainerService _dockerContainerService;
        private DockerContainer _container;
        private bool _isNewContainer;

        public DockerContainerViewModel()
        {
            _dockerContainerService = new DockerContainerService();
            _container = new DockerContainer();
            _isNewContainer = true;
        }

        public DockerContainerViewModel(DockerContainer container)
        {
            _dockerContainerService = new DockerContainerService();
            _container = container;
            _isNewContainer = false;
        }

        public int Id
        {
            get => _container.Id;
            set
            {
                if (_container.Id != value)
                {
                    _container.Id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _container.Name;
            set
            {
                if (_container.Name != value)
                {
                    _container.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Host
        {
            get => _container.Host;
            set
            {
                if (_container.Host != value)
                {
                    _container.Host = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Port
        {
            get => _container.Port;
            set
            {
                if (_container.Port != value)
                {
                    _container.Port = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ApiVersion
        {
            get => _container.ApiVersion;
            set
            {
                if (_container.ApiVersion != value)
                {
                    _container.ApiVersion = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseTLS
        {
            get => _container.UseTLS;
            set
            {
                if (_container.UseTLS != value)
                {
                    _container.UseTLS = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CertificatePath
        {
            get => _container.CertificatePath;
            set
            {
                if (_container.CertificatePath != value)
                {
                    _container.CertificatePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseDockerApi
        {
            get => _container.UseDockerApi;
            set
            {
                if (_container.UseDockerApi != value)
                {
                    _container.UseDockerApi = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsActive
        {
            get => _container.IsActive;
            set
            {
                if (_container.IsActive != value)
                {
                    _container.IsActive = value;
                    OnPropertyChanged();
                }
            }
        }



        public string RemoteWorkspacePath
        {
            get => _container.RemoteWorkspacePath;
            set
            {
                if (_container.RemoteWorkspacePath != value)
                {
                    _container.RemoteWorkspacePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                if (_isNewContainer)
                {
                    _container.DateAdded = DateTime.Now;
                    await _dockerContainerService.AddAsync(_container);
                }
                else
                {
                    await _dockerContainerService.UpdateAsync(_container);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar o container: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void SelectCertificate()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Arquivos de certificado|*.pem;*.crt;*.cert|Todos os arquivos|*.*",
                Title = "Selecione o arquivo de certificado"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                CertificatePath = openFileDialog.FileName;
            }
        }



        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
