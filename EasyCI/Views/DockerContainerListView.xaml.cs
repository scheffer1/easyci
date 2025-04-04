using EasyCI.Models;
using EasyCI.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasyCI.Views
{
    /// <summary>
    /// Interaction logic for DockerContainerListView.xaml
    /// </summary>
    public partial class DockerContainerListView : Window
    {
        private readonly DockerContainerService _dockerContainerService;
        private ObservableCollection<DockerContainer> _containers;

        public DockerContainerListView()
        {
            InitializeComponent();
            _dockerContainerService = new DockerContainerService();
            _containers = new ObservableCollection<DockerContainer>();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadContainersAsync();
        }

        private async System.Threading.Tasks.Task LoadContainersAsync()
        {
            try
            {
                TxtStatus.Text = "Carregando containers...";
                var containers = await _dockerContainerService.GetAllAsync();
                
                _containers.Clear();
                foreach (var container in containers)
                {
                    _containers.Add(container);
                }
                
                DgContainers.ItemsSource = _containers;
                TxtStatus.Text = $"Containers carregados: {_containers.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar containers: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Erro ao carregar containers";
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dockerContainerView = new DockerContainerView();
            if (dockerContainerView.ShowDialog() == true)
            {
                // Recarregar a lista após adicionar
                _ = LoadContainersAsync();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedContainer();
        }

        private void DgContainers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                EditSelectedContainer();
            }
        }

        private void EditSelectedContainer()
        {
            var selectedContainer = DgContainers.SelectedItem as DockerContainer;
            if (selectedContainer != null)
            {
                var dockerContainerView = new DockerContainerView(selectedContainer);
                if (dockerContainerView.ShowDialog() == true)
                {
                    // Recarregar a lista após editar
                    _ = LoadContainersAsync();
                }
            }
            else
            {
                MessageBox.Show("Selecione um container para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedContainer = DgContainers.SelectedItem as DockerContainer;
            if (selectedContainer != null)
            {
                var result = MessageBox.Show($"Tem certeza que deseja excluir o container '{selectedContainer.Name}'?", 
                    "Confirmar Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        TxtStatus.Text = "Excluindo container...";
                        var success = await _dockerContainerService.DeleteAsync(selectedContainer.Id);
                        
                        if (success)
                        {
                            // Recarregar a lista após excluir
                            await LoadContainersAsync();
                            MessageBox.Show("Container excluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Não foi possível excluir o container.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                            TxtStatus.Text = "Erro ao excluir container";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao excluir container: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        TxtStatus.Text = "Erro ao excluir container";
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecione um container para excluir.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadContainersAsync();
        }
    }
}
