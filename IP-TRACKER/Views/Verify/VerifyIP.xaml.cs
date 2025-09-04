using System;
using System.Threading.Tasks;
using System.Windows;
using IP_TRACKER.NetworkPing;

namespace IP_TRACKER.Views.Verify
{
    public partial class VerifyIP : Window
    {
        public VerifyIP()
        {
            InitializeComponent();
        }

        private async void Search_IP_Click(object sender, RoutedEventArgs e)
        {
            string ip = Ipgiven.Text.Trim();

            if (string.IsNullOrEmpty(ip))
            {
                MessageBox.Show("Veuillez entrer une adresse IP.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // UI: désactiver et afficher loader
            Search.IsEnabled = false;
            Progress.Visibility = Visibility.Visible;
            StatusText.Text = "Recherche en cours...";

            try
            {
                var ping = SinglePing.GetInstance(ip);

                await Task.Run(() => ping.Run());

                ResultBox.Text = ping.GetResult();
                StatusText.Text = ping.IsUp ? "Hôte joignable" : "Hôte inaccessible";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du scan : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Erreur";
            }
            finally
            {
                // Restaurer l'UI
                Search.IsEnabled = true;
                Progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
