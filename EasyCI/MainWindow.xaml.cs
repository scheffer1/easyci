using System;
using System.Windows;
using System.Windows.Controls;
using EasyCI.Views;

namespace EasyCI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class EasyCiMain : Window
{
    public EasyCiMain()
    {
        InitializeComponent();
    }

    #region Eventos de Navegação

    private void BtnCadastrarGit_Click(object sender, RoutedEventArgs e)
    {
        // Abrir tela de cadastro de repositório Git
        var gitRepositoryView = new Views.GitRepositoryView();
        gitRepositoryView.ShowDialog();
    }

    private void BtnListarGit_Click(object sender, RoutedEventArgs e)
    {
        // Abrir tela de listagem de repositórios Git
        var gitRepositoryListView = new Views.GitRepositoryListView();
        gitRepositoryListView.ShowDialog();
    }

    private void BtnCadastrarDocker_Click(object sender, RoutedEventArgs e)
    {
        // Abrir tela de cadastro de container Docker
        var dockerContainerView = new Views.DockerContainerView();
        dockerContainerView.ShowDialog();
    }

    private void BtnListarDocker_Click(object sender, RoutedEventArgs e)
    {
        // Abrir tela de listagem de containers Docker
        var dockerContainerListView = new Views.DockerContainerListView();
        dockerContainerListView.ShowDialog();
    }

    private void BtnCriarProjeto_Click(object sender, RoutedEventArgs e)
    {
        // Abrir tela de criação de projeto CI
        var ciProjectView = new Views.CIProjectView();
        ciProjectView.ShowDialog();
    }

    private void BtnListarProjetos_Click(object sender, RoutedEventArgs e)
    {
        // Abrir tela de listagem de projetos CI
        var ciProjectListView = new Views.CIProjectListView();
        ciProjectListView.ShowDialog();
    }

    private void BtnMonitoramento_Click(object sender, RoutedEventArgs e)
    {
        // Abrir tela de monitoramento
        var monitoringView = new Views.MonitoringView();
        monitoringView.ShowDialog();
    }

    #endregion
}