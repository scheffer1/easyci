using System.Windows;
using EasyCI.Services;

namespace EasyCI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Inicializa o banco de dados
        DatabaseInitializer.Initialize();
    }
}