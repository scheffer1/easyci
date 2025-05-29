using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasyCI.Themes;
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

        // Atualizar o texto do botão de tema baseado no tema atual
        UpdateThemeButtonText();

        // Registrar para eventos de mudança de tema
        ThemeManager.ThemeChanged += (s, theme) => UpdateThemeButtonText();

        // Atualizar o ícone de maximizar/restaurar quando o estado da janela mudar
        StateChanged += (s, e) => UpdateMaximizeRestoreButton();
    }

    private void UpdateMaximizeRestoreButton()
    {
        // Atualizar o ícone do botão maximizar/restaurar com base no estado da janela
        if (WindowState == WindowState.Maximized)
        {
            BtnMaximize.Content = "\uE923"; // Ícone de restaurar
        }
        else
        {
            BtnMaximize.Content = "\uE922"; // Ícone de maximizar
        }
    }

    private void UpdateThemeButtonText()
    {
        // Atualizar o ícone e texto do botão de tema
        if (ThemeManager.CurrentTheme == ThemeType.Light)
        {
            ((TextBlock)((StackPanel)BtnToggleTheme.Content).Children[0]).Text = "\uE793"; // Ícone de lua
            ((TextBlock)((StackPanel)BtnToggleTheme.Content).Children[1]).Text = "Modo Escuro";
        }
        else
        {
            ((TextBlock)((StackPanel)BtnToggleTheme.Content).Children[0]).Text = "\uE706"; // Ícone de sol
            ((TextBlock)((StackPanel)BtnToggleTheme.Content).Children[1]).Text = "Modo Claro";
        }
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

    #region Eventos de Tema

    private void BtnToggleTheme_Click(object sender, RoutedEventArgs e)
    {
        ThemeManager.ToggleTheme();
    }

    #endregion

    #region Eventos da Barra de Título

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Permitir arrastar a janela quando o usuário clicar na barra de título
        if (e.ClickCount == 1)
        {
            DragMove();
        }
        else if (e.ClickCount == 2)
        {
            // Alternar entre maximizado e normal com duplo clique
            ToggleMaximize();
        }
    }

    private void ToggleMaximize()
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
        }
        else
        {
            WindowState = WindowState.Maximized;
        }
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void BtnMaximize_Click(object sender, RoutedEventArgs e)
    {
        ToggleMaximize();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    #endregion
}