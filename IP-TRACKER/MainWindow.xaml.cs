using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using IP_TRACKER.NetworkPing;

namespace IP_TRACKER
{
    public partial class MainWindow : Window
    {
        private const int MaxDigits = 12; 
        private bool _isUpdating;
        private const int MaxDigitsPerOctet = 3;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Search_IP_Click(object sender, RoutedEventArgs e)
        {
            string raw = IpTextBox.Text?.Trim() ?? string.Empty;

            if (!IsValidIPFormat(raw))
            {
                MessageBox.Show("Format d'adresse IP invalide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ToggleUiForSearch(true, "Recherche en cours...");

            try
            {
                var ping = SinglePing.GetInstance(raw);
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
                ToggleUiForSearch(false, null);
            }
        }

        private bool IsValidIPFormat(string ip)
        {
            // Pattern pour supporter différents formats IP
            string pattern = @"^(\d{1,3}\.){2,4}\d{1,3}$";
            if (!Regex.IsMatch(ip, pattern)) return false;

            // Vérifier que chaque octet est entre 0-255
            string[] parts = ip.Split('.');
            return parts.All(part => int.TryParse(part, out int value) && value >= 0 && value <= 255);
        }

        private void IpTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_isUpdating) return;
            if (!(sender is TextBox tb))
            {
                e.Handled = true;
                return;
            }

            string input = e.Text;

            if (input == ".")
            {
                // délègue à ta méthode de vérification pour le point
                e.Handled = !CanInsertDot(tb);
                return;
            }

            if (IsDigit(input))
            {
                // Vérifier la limite totale de chiffres en tenant compte d'une sélection qui sera remplacée
                string text = tb.Text ?? string.Empty;
                int digitsAfterReplace = CountDigits(text);

                if (tb.SelectionLength > 0)
                {
                    // on retire les chiffres qui seront remplacés par l'entrée courante
                    string selected = text.Substring(tb.SelectionStart, tb.SelectionLength);
                    digitsAfterReplace -= CountDigits(selected);
                }

                if (digitsAfterReplace >= MaxDigits)
                {
                    e.Handled = true;
                    return;
                }

                // vérifier la longueur de l'octet courant (prend en compte la sélection)
                if (!CanInsertDigitAtCaret(tb))
                {
                    e.Handled = true;
                    return;
                }

                e.Handled = false;
                return;
            }

            // tout le reste interdit
            e.Handled = true;
        }

        

        private void IpTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;
            if (!(sender is TextBox tb)) return;

            try
            {
                _isUpdating = true;
                string original = tb.Text ?? string.Empty;
                string cleaned = CleanIPInput(original);
                
                if (tb.Text != cleaned)
                    tb.Text = cleaned;
                
                tb.CaretIndex = cleaned.Length;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private string CleanIPInput(string input)
        {
            // Supprimer les points excessifs et les caractères non-valides
            string result = Regex.Replace(input, @"[^\d.]", "");
            result = Regex.Replace(result, @"\.+", ".");
            result = result.Trim('.');

            // Limiter le nombre total de digits
            string digitsOnly = new string(result.Where(c => char.IsDigit(c)).ToArray());
            if (digitsOnly.Length > MaxDigits)
                digitsOnly = digitsOnly.Substring(0, MaxDigits);

            return FormatIPAddress(digitsOnly);
        }

        private string FormatIPAddress(string digits)
        {
            if (string.IsNullOrEmpty(digits)) return "";

            // Formater selon la longueur des digits
            return digits.Length switch
            {
                <= 3 => digits,
                <= 6 => $"{digits.Substring(0, 3)}.{digits.Substring(3)}",
                <= 9 => $"{digits.Substring(0, 3)}.{digits.Substring(3, 3)}.{digits.Substring(6)}",
                _ => $"{digits.Substring(0, 3)}.{digits.Substring(3, 3)}.{digits.Substring(6, 3)}.{digits.Substring(9)}"
            };
        }

        private bool CanInsertDot(TextBox tb)
        {
            string text = tb.Text ?? string.Empty;
            int dotCount = text.Count(c => c == '.');
            
            if (dotCount >= 3) return true;
            if (text.EndsWith(".")) return false;
            if (text.Length == 0) return false;

            // Vérifier le segment actuel
            string[] segments = text.Split('.');
            string lastSegment = segments.Last();
            
            return lastSegment.Length > 0;
        }

        private static bool IsDigit(string text) => 
            !string.IsNullOrEmpty(text) && Regex.IsMatch(text, "^[0-9]$");

        private static int CountDigits(string s) => 
            s.Count(char.IsDigit);

        private void ToggleUiForSearch(bool isSearching, string statusMessage)
        {
            Search.IsEnabled = !isSearching;
            Progress.Visibility = isSearching ? Visibility.Visible : Visibility.Collapsed;
            if (!string.IsNullOrEmpty(statusMessage)) StatusText.Text = statusMessage;
        }
    }
}