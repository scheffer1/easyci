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
    public class GitRepositoryViewModel : INotifyPropertyChanged
    {
        private readonly GitRepositoryService _gitRepositoryService;
        private GitRepository _repository;
        private bool _isNewRepository;

        public GitRepositoryViewModel()
        {
            _gitRepositoryService = new GitRepositoryService();
            _repository = new GitRepository();
            _isNewRepository = true;
        }

        public GitRepositoryViewModel(GitRepository repository)
        {
            _gitRepositoryService = new GitRepositoryService();
            _repository = repository;
            _isNewRepository = false;
        }

        public int Id
        {
            get => _repository.Id;
            set
            {
                if (_repository.Id != value)
                {
                    _repository.Id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _repository.Name;
            set
            {
                if (_repository.Name != value)
                {
                    _repository.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Url
        {
            get => _repository.Url;
            set
            {
                if (_repository.Url != value)
                {
                    _repository.Url = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Branch
        {
            get => _repository.Branch;
            set
            {
                if (_repository.Branch != value)
                {
                    _repository.Branch = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SshKeyPath
        {
            get => _repository.SshKeyPath;
            set
            {
                if (_repository.SshKeyPath != value)
                {
                    _repository.SshKeyPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Username
        {
            get => _repository.Username;
            set
            {
                if (_repository.Username != value)
                {
                    _repository.Username = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsActive
        {
            get => _repository.IsActive;
            set
            {
                if (_repository.IsActive != value)
                {
                    _repository.IsActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                if (_isNewRepository)
                {
                    _repository.DateAdded = DateTime.Now;
                    await _gitRepositoryService.AddAsync(_repository);
                }
                else
                {
                    await _gitRepositoryService.UpdateAsync(_repository);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar o repositório: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void SelectSshKey()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Arquivos de chave SSH|*.*",
                Title = "Selecione o arquivo de chave SSH"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SshKeyPath = openFileDialog.FileName;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
