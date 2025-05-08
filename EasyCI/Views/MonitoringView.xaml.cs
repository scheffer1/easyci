using System.Windows;

namespace EasyCI.Views
{
    /// <summary>
    /// Interaction logic for MonitoringView.xaml
    /// </summary>
    public partial class MonitoringView : Window
    {
        public MonitoringView()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Voltar para a tela principal
            Close();
        }
    }
}
