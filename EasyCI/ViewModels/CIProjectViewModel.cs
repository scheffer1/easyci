using EasyCI.Models;
using EasyCI.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace EasyCI.ViewModels
{
    public class CIProjectViewModel : INotifyPropertyChanged
    {
        private readonly CIProjectService _ciProjectService;
        private readonly GitRepositoryService _gitRepositoryService;
        private readonly DockerContainerService _dockerContainerService;
        private readonly BuildService _buildService;
        private CIProject _project;
        private bool _isNewProject;
        private ObservableCollection<GitRepository> _gitRepositories;
        private ObservableCollection<DockerContainer> _dockerContainers;
        private GitRepository? _selectedGitRepository;
        private DockerContainer? _selectedDockerContainer;

        public CIProjectViewModel()
        {
            _ciProjectService = new CIProjectService();
            _gitRepositoryService = new GitRepositoryService();
            _dockerContainerService = new DockerContainerService();
            _buildService = new BuildService();
            _project = new CIProject();
            _isNewProject = true;
            _gitRepositories = new ObservableCollection<GitRepository>();
            _dockerContainers = new ObservableCollection<DockerContainer>();
            LoadRepositoriesAndContainers();
        }

        public CIProjectViewModel(CIProject project)
        {
            _ciProjectService = new CIProjectService();
            _gitRepositoryService = new GitRepositoryService();
            _dockerContainerService = new DockerContainerService();
            _buildService = new BuildService();
            _project = project;
            _isNewProject = false;
            _gitRepositories = new ObservableCollection<GitRepository>();
            _dockerContainers = new ObservableCollection<DockerContainer>();
            LoadRepositoriesAndContainers();

            if (project.GitRepository != null)
                SelectedGitRepository = project.GitRepository;

            if (project.DockerContainer != null)
                SelectedDockerContainer = project.DockerContainer;
        }

        private async void LoadRepositoriesAndContainers()
        {
            try
            {
                var repositories = await _gitRepositoryService.GetAllAsync();
                GitRepositories.Clear();
                foreach (var repo in repositories)
                {
                    GitRepositories.Add(repo);
                }

                var containers = await _dockerContainerService.GetAllAsync();
                DockerContainers.Clear();
                foreach (var container in containers)
                {
                    DockerContainers.Add(container);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public int Id
        {
            get => _project.Id;
            set
            {
                if (_project.Id != value)
                {
                    _project.Id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _project.Name;
            set
            {
                if (_project.Name != value)
                {
                    _project.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ComposeFilePath
        {
            get => _project.ComposeFilePath;
            set
            {
                if (_project.ComposeFilePath != value)
                {
                    _project.ComposeFilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AutoBuild
        {
            get => _project.AutoBuild;
            set
            {
                if (_project.AutoBuild != value)
                {
                    _project.AutoBuild = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsActive
        {
            get => _project.IsActive;
            set
            {
                if (_project.IsActive != value)
                {
                    _project.IsActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<GitRepository> GitRepositories
        {
            get => _gitRepositories;
            set
            {
                _gitRepositories = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DockerContainer> DockerContainers
        {
            get => _dockerContainers;
            set
            {
                _dockerContainers = value;
                OnPropertyChanged();
            }
        }

        public GitRepository? SelectedGitRepository
        {
            get => _selectedGitRepository;
            set
            {
                _selectedGitRepository = value;
                if (value != null)
                {
                    _project.GitRepositoryId = value.Id;
                    _project.GitRepository = value;
                }
                OnPropertyChanged();
            }
        }

        public DockerContainer? SelectedDockerContainer
        {
            get => _selectedDockerContainer;
            set
            {
                _selectedDockerContainer = value;
                if (value != null)
                {
                    _project.DockerContainerId = value.Id;
                    _project.DockerContainer = value;
                }
                OnPropertyChanged();
            }
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                if (_selectedGitRepository == null || _selectedDockerContainer == null)
                {
                    MessageBox.Show("Selecione um repositório Git e um container Docker.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(Name))
                {
                    MessageBox.Show("O nome do projeto é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (_isNewProject)
                {
                    _project.DockerContainer = null;
                    _project.GitRepository = null;

                    _project.DateCreated = DateTime.Now;
                    _project.Status = "Aguardando início do build...";
                    await _ciProjectService.AddAsync(_project);

                    // Iniciar o processo de build automaticamente após salvar
                    var buildResult = await _buildService.ExecuteBuildAsync(_project.Id);
                    if (!buildResult.Success)
                    {
                        MessageBox.Show($"Projeto salvo, mas houve um problema ao iniciar o build: {buildResult.Message}",
                            "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    await _ciProjectService.UpdateAsync(_project);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar o projeto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
