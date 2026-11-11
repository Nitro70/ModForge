using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace ModForge.Core
{
    /// <summary>A flat key/value file in %LOCALAPPDATA%\ModForge. Losing it only loses conveniences.</summary>
    public sealed class Settings
    {
        readonly Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>When true nothing is written to disk (screenshot and test modes).</summary>
        public bool ReadOnly;

        static string FilePath { get { return Path.Combine(PackStore.DataDir, "settings.json"); } }

        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                if (File.Exists(FilePath))
                    foreach (var kv in Json.ParseObject(File.ReadAllText(FilePath)))
                        s.map[kv.Key] = Convert.ToString(kv.Value);
            }
            catch (Exception) { }
            return s;
        }

        public string Get(string key, string def = "")
        {
            string v;
            return map.TryGetValue(key, out v) ? v : def;
        }

        public void Set(string key, string value)
        {
            map[key] = value ?? "";
        }

        public void Save()
        {
            if (ReadOnly) return;
            try
            {
                Directory.CreateDirectory(PackStore.DataDir);
                File.WriteAllText(FilePath, Json.WriteMap(map));
            }
            catch (Exception) { }
        }

        public string OutputDir
        {
            get
            {
                string d = Get("OutputDir");
                return d.Length > 0 ? d : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ModForge");
            }
            set { Set("OutputDir", value); }
        }

        /// <summary>Where "Update templates" downloads from: a packs zip, or a GitHub source zip that contains pack.json.</summary>
        public string PackUrl
        {
            get { string u = Get("PackUrl").Trim(); return u.Length > 0 ? u : DefaultPackUrl; }
            set { Set("PackUrl", value); }
        }

        /// <summary>This repo's main branch as a zip: Update finds packs/pack.json inside it. Can be changed in the app.</summary>
        public const string DefaultPackUrl = "https://github.com/Nitro70/ModForge/archive/refs/heads/main.zip";

        public List<string> ExtraGames
        {
            get { return Get("ExtraGames").Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
            set { Set("ExtraGames", string.Join("|", value.Distinct(StringComparer.OrdinalIgnoreCase))); }
        }
    }

    public static class PackUpdater
    {
        /// <summary>Downloads a pack zip, checks it really is one, and keeps it. Returns what happened in plain words.</summary>
        public static string Update(string url, int currentVersion)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ForgeException("no template source is set");
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            byte[] data;
            using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) })
            {
                http.DefaultRequestHeaders.UserAgent.ParseAdd("ModForge");
                data = http.GetByteArrayAsync(url).GetAwaiter().GetResult();
            }
            Pack pack;
            using (var ms = new MemoryStream(data)) pack = PackStore.LoadZip(ms, "downloaded");
            if (pack.Templates.Count == 0) throw new ForgeException("that download has no templates in it");
            if (pack.Version <= currentVersion) return "already up to date (v" + currentVersion + ")";
            Directory.CreateDirectory(PackStore.DataDir);
            File.WriteAllBytes(PackStore.DownloadedZip, data);
            return "updated to v" + pack.Version + " (" + pack.Templates.Count + " templates)";
        }
    }
}
