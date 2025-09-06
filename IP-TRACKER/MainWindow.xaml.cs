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
        private const int MaxDigitsPerOctet = 3;
        private const int MaxOctets = 4;
        private bool _isUpdating;

        public MainWindow()
        {
            InitializeComponent();

            DataObject.AddPastingHandler(Octet1, OnPaste);
            DataObject.AddPastingHandler(Octet2, OnPaste);
            DataObject.AddPastingHandler(Octet3, OnPaste);
            DataObject.AddPastingHandler(Octet4, OnPaste);
        }

        private async void Search_IP_Click(object sender, RoutedEventArgs e)
        {
            string ip = $"{Octet1.Text.Trim()}.{Octet2.Text.Trim()}.{Octet3.Text.Trim()}.{Octet4.Text.Trim()}";

            string[] parts = new[] { Octet1.Text, Octet2.Text, Octet3.Text, Octet4.Text };
            if (parts.Any(p => string.IsNullOrWhiteSpace(p)))
            {
                MessageBox.Show("Veuillez renseigner les 4 octets de l'adresse IP.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!parts.All(IsValidOctet))
            {
                MessageBox.Show("Chaque octet doit être un nombre entre 0 et 255.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ToggleUiForSearch(true, "Recherche en cours...");

            try
            {
                var ping = SinglePing.GetInstance(ip);
                await ping.RunAsync();

                ResultBox.Text = ping.GetResult();
                StatusText.Text = ping.IsUp ? "Hôte joignable" : "Hôte inaccessible";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du scan : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Erreur";
            }
            finally
            {
                ToggleUiForSearch(false, null);
            }
        }


        // --- Input handlers pour chaque octet ---

        private void Octet_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox tb))
            {
                e.Handled = true;
                return;
            }

            string input = e.Text;

            if (input == ".")
            {
                // si l'utilisateur tape '.', on passe au champ suivant
                e.Handled = true;
                MoveFocusToNext(tb);
                return;
            }

            // autoriser seulement les chiffres
            if (!Regex.IsMatch(input, "^[0-9]$"))
            {
                e.Handled = true;
                return;
            }

            // empêcher dépassement de MaxDigitsPerOctet en simulant insertion
            int caret = tb.CaretIndex;
            string newText = tb.Text.Substring(0, caret) + input + (caret < tb.Text.Length ? tb.Text.Substring(caret) : "");
            e.Handled = newText.Length > MaxDigitsPerOctet;
        }

        private void Octet_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;
            if (!(sender is TextBox tb)) return;

            try
            {
                _isUpdating = true;

                // garder seulement les chiffres
                string cleaned = Regex.Replace(tb.Text ?? string.Empty, @"[^\d]", "");
                if (tb.Text != cleaned)
                {
                    int oldCaret = tb.CaretIndex;
                    tb.Text = cleaned;
                    tb.CaretIndex = Math.Min(cleaned.Length, oldCaret);
                }

                // si on a atteint 3 chiffres, passer automatiquement au suivant
                if (cleaned.Length == MaxDigitsPerOctet)
                {
                    MoveFocusToNext(tb);
                }

                // Si l'utilisateur colle une IP complète (ex: "192.168.0.1"), on gère dans OnPaste,
                // mais si pour une raison quelconque le champ contient des '.' on déclenche le split ici.
                if (tb.Text.Contains("."))
                {
                    TrySplitAndDistribute(tb.Text);
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void Octet_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox tb)) return;

            if (e.Key == Key.Back)
            {
                if (string.IsNullOrEmpty(tb.Text) && !MoveFocusToPrevious(tb))
                {
                    // rien à faire si on est sur le premier
                }
            }

            if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                e.Handled = true;
                MoveFocusToNext(tb);
            }

        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.SourceDataObject.GetDataPresent(DataFormats.Text)) return;
            var txt = e.SourceDataObject.GetData(DataFormats.Text) as string ?? string.Empty;
            txt = txt.Trim();

            if (txt.Contains("."))
            {
                var tokens = txt.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => Regex.Replace(t, @"[^\d]", "")) 
                                .ToArray();

                if (tokens.Length >= 1)
                {
                    if (tokens.Length >= 4)
                    {
                        Octet1.Text = tokens[0].Substring(0, Math.Min(3, tokens[0].Length));
                        Octet2.Text = tokens[1].Substring(0, Math.Min(3, tokens[1].Length));
                        Octet3.Text = tokens[2].Substring(0, Math.Min(3, tokens[2].Length));
                        Octet4.Text = tokens[3].Substring(0, Math.Min(3, tokens[3].Length));
                    }
                    else
                    {
                        TextBox target = sender as TextBox ?? Octet1;
                        var boxes = new[] { Octet1, Octet2, Octet3, Octet4 };
                        int start = Array.IndexOf(boxes, target);
                        for (int i = 0; i < tokens.Length && start + i < boxes.Length; i++)
                        {
                            boxes[start + i].Text = tokens[i].Substring(0, Math.Min(3, tokens[i].Length));
                        }
                    }

                    e.CancelCommand();
                }
            }
        }

        // --- Helpers ---
        private bool MoveFocusToNext(TextBox tb)
        {
            if (tb == Octet1) { Octet2.Focus(); Octet2.CaretIndex = Octet2.Text.Length; return true; }
            if (tb == Octet2) { Octet3.Focus(); Octet3.CaretIndex = Octet3.Text.Length; return true; }
            if (tb == Octet3) { Octet4.Focus(); Octet4.CaretIndex = Octet4.Text.Length; return true; }
            return false;
        }

        private bool MoveFocusToPrevious(TextBox tb)
        {
            if (tb == Octet4) { Octet3.Focus(); Octet3.CaretIndex = Octet3.Text.Length; return true; }
            if (tb == Octet3) { Octet2.Focus(); Octet2.CaretIndex = Octet2.Text.Length; return true; }
            if (tb == Octet2) { Octet1.Focus(); Octet1.CaretIndex = Octet1.Text.Length; return true; }
            return false;
        }

        private void TrySplitAndDistribute(string text)
        {
            var tokens = text.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(t => Regex.Replace(t, @"[^\d]", ""))
                             .ToArray();

            if (tokens.Length <= 1) return;

            // déterminer l'index du champ qui a appelé ceci
            TextBox[] boxes = { Octet1, Octet2, Octet3, Octet4 };
            TextBox caller = boxes.FirstOrDefault(b => b.Text.Contains("."));
            int startIndex = caller != null ? Array.IndexOf(boxes, caller) : 0;

            for (int i = 0; i < tokens.Length && startIndex + i < boxes.Length; i++)
            {
                boxes[startIndex + i].Text = tokens[i].Substring(0, Math.Min(3, tokens[i].Length));
            }
        }

        private static bool IsValidOctet(string s)
        {
            if (!int.TryParse(s, out int v)) return false;
            return v >= 0 && v <= 255;
        }

        private void ToggleUiForSearch(bool isSearching, string statusMessage)
        {
            Search.IsEnabled = !isSearching;
            Progress.Visibility = isSearching ? Visibility.Visible : Visibility.Collapsed;
            if (!string.IsNullOrEmpty(statusMessage)) StatusText.Text = statusMessage;
        }
    }
}
