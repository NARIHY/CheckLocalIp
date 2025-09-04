# 📡 IP-TRACKER

IP-TRACKER est une application **WPF en C#** permettant de vérifier l’accessibilité d’une adresse IP et de récupérer diverses informations réseau et de localisation.

## 🚀 Fonctionnalités

* 🔎 Vérifie si une **adresse IP** est active ou inactive (ping).
* 💻 Affiche le **nom de la machine** associée (si accessible).
* 🖧 Récupère l’**adresse MAC** (si disponible).
* 🌍 Localise l’adresse IP sur la carte (ville, région, pays).
* 🕒 Donne la **date et l’heure du scan**.
* 📋 Résultat affiché directement dans l’interface.

## 🖼️ Exemple d’utilisation

Lorsque l’utilisateur saisit une IP et clique sur **Rechercher**, l’application effectue un scan et retourne un rapport :

```
--- Résultat du scan ---
IP            : 192.152.154.111
Statut        : Inactif
Nom de machine: Inaccessible
Adresse MAC   : Inconnu
Localisation  : Pittsburgh, Pennsylvania, US
Date/Heure    : 2025-09-04 16:34:33
```

## 🛠️ Technologies utilisées

* **C# .NET 6 / .NET Framework (WPF)**
* **System.Net.NetworkInformation** (Ping, IP Hostname)
* **API de géolocalisation IP** (ex: ip-api, ipstack, etc.)
* **XAML** pour l’interface utilisateur

## 📦 Installation

1. Cloner le projet :

   ```bash
   git clone https://github.com/NARIHY/CheckLocalIp.git
   cd checkLocalIp
   ```

2. Ouvrir la solution dans **Visual Studio**.

3. Restaurer les dépendances (NuGet si nécessaire).

4. Compiler et exécuter.

## ▶️ Utilisation

1. Entrer une adresse IP dans le champ de texte.
2. Cliquer sur **Rechercher**.
3. Consulter le résultat du scan affiché en dessous.

## 📌 Améliorations possibles

* Exporter le résultat en **CSV / PDF**.
* Ajouter un historique des scans.
* Intégrer une carte interactive pour la localisation.
* Support multi-plateformes (via .NET MAUI).

---

👉 Ce projet est un outil simple mais utile pour administrateurs réseaux, pentesters débutants, ou toute personne souhaitant connaître l’état et la localisation d’une IP.

---
<img width="405" height="327" alt="image" src="https://github.com/user-attachments/assets/f93257cf-7652-4a4e-b522-a3f88a6572f4" />









