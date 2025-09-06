using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;

namespace IP_TRACKER.NetworkPing
{
    class SinglePing
    {
        private static SinglePing _instance;
        private static readonly object _lock = new object();

        private string ip;

        public bool IsUp { get; private set; }
        public string Hostname { get; private set; }
        public string Status { get; private set; }
        public string Mac { get; private set; }
        public string Location { get; private set; }

        private SinglePing(string ip) { this.ip = ip; }

        public static SinglePing GetInstance(string ip)
        {
            lock (_lock)
            {
                if (_instance == null)
                    _instance = new SinglePing(ip);
                else
                    _instance.ip = ip;
            }
            return _instance;
        }

        /// <summary>
        /// Lance le ping et remplit les propriétés (async)
        /// </summary>
        public async Task RunAsync()
        {
            IsUp = await PingHostAsync();
            Hostname = IsUp ? GetHostName() : "Inaccessible";
            Status = IsUp ? "Actif" : "Inactif";
            Mac = IsUp ? GetMacAddress() : "Inconnu";
            Location = await GetLocationAsync();
        }

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

        private async Task<bool> PingHostAsync()
        {
            try
            {
                using var ping = new Ping();
                PingReply reply = await ping.SendPingAsync(ip, 2000);
                return reply.Status == IPStatus.Success;
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

                using Process process = Process.Start(psi);
                using var reader = process.StandardOutput;
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
            catch { }
            return "Inconnu";
        }

        private async Task<string> GetLocationAsync()
        {
            try
            {
                using HttpClient client = new HttpClient();
                string url = $"https://ipinfo.io/{ip}/json";
                string json = await client.GetStringAsync(url);

                using JsonDocument doc = JsonDocument.Parse(json);
                string city = doc.RootElement.TryGetProperty("city", out var c) ? c.GetString() : "";
                string region = doc.RootElement.TryGetProperty("region", out var r) ? r.GetString() : "";
                string country = doc.RootElement.TryGetProperty("country", out var cn) ? cn.GetString() : "";
                return $"{city}, {region}, {country}".Trim(',', ' ');
            }
            catch { return "Inconnue"; }
        }
    }
}
