// IP_TRACKER.NetworkPing.SinglePing (extrait important)
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading;

namespace IP_TRACKER.NetworkPing
{
    class SinglePing
    {
        // singleton déjà en place (GetInstance etc.)
        private static SinglePing _instance;
        private static readonly object _lock = new object();

        private string ip;

        // états remplis par Run()
        public bool IsUp { get; private set; }
        public string Hostname { get; private set; }
        public string Status { get; private set; }
        public string Mac { get; private set; }
        public string Location { get; private set; }

        private SinglePing(string ip) { this.ip = ip; }

        public static SinglePing GetInstance(string ip)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null) _instance = new SinglePing(ip);
                }
            }
            else
            {
                _instance.ip = ip;
            }
            return _instance;
        }

        // Ton Run qui remplit les propriétés (bloquant)
        public void Run()
        {
            IsUp = PingHost();
            Hostname = IsUp ? GetHostName() : "Inaccessible";
            Status = IsUp ? "Actif" : "Inactif";
            Mac = IsUp ? GetMacAddress() : "Inconnu";
            Location = GetLocation();
        }

        // Retourne un résumé texte prêt à afficher
        public string GetResult()
        {
            return
$@"--- Résultat du scan ---

IP             : {ip}
Statut         : {Status}
Nom de machine : {Hostname}
Adresse MAC    : {Mac}
Localisation   : {Location}
Date/Heure     : {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        // --- méthodes utilitaires (PingHost, GetHostName, GetMacAddress, GetLocation) ---
        // (garde tes implémentations actuelles ici)
        private bool PingHost()
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(ip, 2000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch { return false; }
        }

        private string GetHostName()
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                return hostEntry.HostName;
            }
            catch { return "Inconnu"; }
        }

        private string GetMacAddress()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "arp",
                    Arguments = "-a " + ip,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                using (var reader = process.StandardOutput)
                {
                    string output = reader.ReadToEnd();
                    string[] lines = output.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.Contains(ip))
                        {
                            string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 2) return parts[1];
                        }
                    }
                }
            }
            catch { }
            return "Inconnu";
        }

        private string GetLocation()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"https://ipinfo.io/{ip}/json";
                    string json = client.GetStringAsync(url).Result;

                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        string city = doc.RootElement.GetProperty("city").GetString();
                        string region = doc.RootElement.GetProperty("region").GetString();
                        string country = doc.RootElement.GetProperty("country").GetString();
                        return $"{city}, {region}, {country}";
                    }
                }
            }
            catch { return "Inconnue"; }
        }
    }
}
