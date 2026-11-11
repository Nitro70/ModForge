using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace ModForge.Core
{
    /// <summary>Works out what a game folder is: engine, scripting backend, version, and which mod loaders are already in it.</summary>
    public static class EngineDetector
    {
        public static GameInfo Inspect(string path, string name, string appId, string source)
        {
            var g = new GameInfo { Name = name, Path = path.TrimEnd('\\', '/'), AppId = appId ?? "", Installed = true, Source = source };
            try
            {
                if (!Directory.Exists(g.Path)) { g.Installed = false; return g; }
                if (!Unity(g) && !Unreal(g)) Godot(g);
                if (g.ExeName.Length == 0)
                {
                    string exe = BestExe(g.Path);
                    if (exe != null) { g.ExeName = Path.GetFileNameWithoutExtension(exe); g.Arch = Arch(exe); }
                }
            }
            catch (Exception) { /* an unreadable folder is just an unknown game */ }
            return g;
        }

        static bool Unity(GameInfo g)
        {
            string data = Directory.EnumerateDirectories(g.Path, "*_Data")
                .FirstOrDefault(d => File.Exists(d.Substring(0, d.Length - "_Data".Length) + ".exe"));
            if (data == null) return false;
            string exe = data.Substring(0, data.Length - "_Data".Length) + ".exe";
            string player = Path.Combine(g.Path, "UnityPlayer.dll");
            if (!File.Exists(player) && !Directory.Exists(Path.Combine(data, "Managed")) && !File.Exists(Path.Combine(g.Path, "GameAssembly.dll")))
                return false;

            g.Engine = "unity";
            g.ExeName = Path.GetFileNameWithoutExtension(exe);
            g.DataDir = Path.GetFileName(data);
            g.Arch = Arch(exe);
            g.Backend = File.Exists(Path.Combine(g.Path, "GameAssembly.dll")) ? "il2cpp" : "mono";
            g.EngineVersion = VersionOf(File.Exists(player) ? player : exe, 3);

            string core = Path.Combine(g.Path, "BepInEx", "core");
            if (Directory.Exists(core))
                g.Loaders.Add(File.Exists(Path.Combine(core, "BepInEx.Core.dll")) ? "BepInEx 6" : "BepInEx 5");
            if (Directory.Exists(Path.Combine(g.Path, "MelonLoader"))) g.Loaders.Add("MelonLoader");
            return true;
        }

        static bool Unreal(GameInfo g)
        {
            if (!Directory.Exists(Path.Combine(g.Path, "Engine"))) return false;
            foreach (var dir in Directory.EnumerateDirectories(g.Path))
            {
                if (string.Equals(Path.GetFileName(dir), "Engine", StringComparison.OrdinalIgnoreCase)) continue;
                string bin = Path.Combine(dir, "Binaries", "Win64");
                if (!Directory.Exists(bin)) continue;
                string exe = Directory.EnumerateFiles(bin, "*Shipping.exe").FirstOrDefault() ?? Directory.EnumerateFiles(bin, "*.exe").FirstOrDefault();
                if (exe == null) continue;

                g.Engine = "unreal";
                g.UnrealProject = Path.GetFileName(dir);
                g.UnrealBinDir = g.UnrealProject + "\\Binaries\\Win64";
                g.ExeName = Path.GetFileNameWithoutExtension(exe);
                g.Arch = "x64";
                string v = VersionOf(exe, 2);
                if (v.StartsWith("4.", StringComparison.Ordinal) || v.StartsWith("5.", StringComparison.Ordinal)) g.EngineVersion = v;
                if (File.Exists(Path.Combine(bin, "UE4SS.dll")) || File.Exists(Path.Combine(bin, "ue4ss", "UE4SS.dll")))
                    g.Loaders.Add("UE4SS");
                return true;
            }
            return false;
        }

        static void Godot(GameInfo g)
        {
            string exe = BestExe(g.Path);
            bool pck = Directory.EnumerateFiles(g.Path, "*.pck").Any();
            string product = "";
            if (exe != null)
            {
                try
                {
                    var vi = FileVersionInfo.GetVersionInfo(exe);
                    product = (vi.ProductName ?? "") + " " + (vi.FileDescription ?? "");
                }
                catch (Exception) { }
            }
            if (!pck && product.IndexOf("Godot", StringComparison.OrdinalIgnoreCase) < 0) return;
            g.Engine = "godot";
            if (exe != null)
            {
                g.ExeName = Path.GetFileNameWithoutExtension(exe);
                g.Arch = Arch(exe);
                if (product.IndexOf("Godot", StringComparison.OrdinalIgnoreCase) >= 0) g.EngineVersion = VersionOf(exe, 2);
            }
        }

        static string BestExe(string dir)
        {
            string folder = Path.GetFileName(dir).Replace(" ", "");
            return Directory.EnumerateFiles(dir, "*.exe")
                .Where(f =>
                {
                    string n = Path.GetFileName(f);
                    return n.IndexOf("Crash", StringComparison.OrdinalIgnoreCase) < 0
                        && n.IndexOf("unins", StringComparison.OrdinalIgnoreCase) < 0
                        && n.IndexOf("Redist", StringComparison.OrdinalIgnoreCase) < 0
                        && n.IndexOf("Launcher", StringComparison.OrdinalIgnoreCase) < 0;
                })
                .OrderByDescending(f => string.Equals(Path.GetFileNameWithoutExtension(f).Replace(" ", ""), folder, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(f => new FileInfo(f).Length)
                .FirstOrDefault();
        }

        static string VersionOf(string file, int parts)
        {
            try
            {
                var vi = FileVersionInfo.GetVersionInfo(file);
                foreach (var s in new[] { vi.ProductVersion, vi.FileVersion })
                {
                    var m = Regex.Match(s ?? "", @"^\s*(\d+)\.(\d+)(?:\.(\d+))?");
                    if (!m.Success) continue;
                    string v = m.Groups[1].Value + "." + m.Groups[2].Value;
                    if (parts >= 3 && m.Groups[3].Success) v += "." + m.Groups[3].Value;
                    if (v != "0.0" && v != "0.0.0" && v != "1.0.0" && v != "1.0") return v;
                }
            }
            catch (Exception) { }
            return "";
        }

        /// <summary>Reads the machine type from the exe header: x64 or x86.</summary>
        static string Arch(string exe)
        {
            try
            {
                using (var fs = new FileStream(exe, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var br = new BinaryReader(fs))
                {
                    fs.Position = 0x3C;
                    int pe = br.ReadInt32();
                    fs.Position = pe + 4;
                    ushort machine = br.ReadUInt16();
                    return machine == 0x8664 ? "x64" : machine == 0x014C ? "x86" : "";
                }
            }
            catch (Exception) { return ""; }
        }
    }

    /// <summary>Finds installed games in Steam and Epic libraries, plus folders the user added by hand.</summary>
    public static class GameScanner
    {
        static readonly HashSet<string> SkipApps = new HashSet<string> { "228980", "1070560", "1391110", "1628350", "1493710", "250820", "1826330" };

        /// <summary>
        /// Screenshot demo mode: no library is read. Every game the pack knows is shown as installed under C:\Games,
        /// so a published screenshot never shows the real machine's games.
        /// </summary>
        public static bool Demo;

        static readonly Dictionary<string, string> DemoVersions = new Dictionary<string, string>
        {
            { "unity", "2022.3.21" }, { "unreal", "5.3" }, { "godot", "4.3" },
        };

        static List<GameInfo> DemoGames(Pack pack)
        {
            var list = new List<GameInfo>();
            foreach (var k in pack.Games)
            {
                if (list.Any(g => g.Name == k.Name)) continue;
                string v;
                var g = new GameInfo
                {
                    Name = k.Name, AppId = k.AppId, Engine = k.Engine, Backend = k.Backend, Family = k.Family,
                    Notes = k.Notes, SuggestedLoader = k.Loader, Installed = true, Source = "Demo",
                    Path = @"C:\Games\" + k.Name.Replace(":", ""),
                    EngineVersion = DemoVersions.TryGetValue(k.Engine, out v) ? v : "",
                };
                if (k.Loader.Length > 0 && list.Count % 2 == 0) g.Loaders.Add(k.Loader);
                list.Add(g);
            }
            return list.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static List<GameInfo> Scan(Pack pack, IEnumerable<string> extraFolders)
        {
            if (Demo) return DemoGames(pack);
            var found = new List<GameInfo>();
            var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Action<GameInfo> add = g =>
            {
                if (g == null || !g.Installed || !seenPaths.Add(g.Path)) return;
                Apply(pack, g);
                found.Add(g);
            };

            foreach (var g in Steam()) add(g);
            foreach (var g in Epic()) add(g);
            foreach (var f in extraFolders ?? Enumerable.Empty<string>())
                if (Directory.Exists(f)) add(EngineDetector.Inspect(f, Path.GetFileName(f.TrimEnd('\\', '/')), "", "Added"));

            // Games that are not in a store library but live at a known place (folders may use %ENV% names).
            foreach (var k in pack.Games)
            {
                foreach (var raw in k.Folders.Where(x => x.IndexOf('%') >= 0 || x.IndexOf(':') >= 0))
                {
                    string p = Environment.ExpandEnvironmentVariables(raw);
                    if (!Directory.Exists(p)) continue;
                    var g = EngineDetector.Inspect(p, k.Name, k.AppId, "Found");
                    g.Name = k.Name;
                    add(g);
                    break;
                }
            }

            var result = found.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var k in pack.Games)
            {
                if (result.Any(g => Matches(k, g))) continue;
                result.Add(new GameInfo
                {
                    Name = k.Name, AppId = k.AppId, Engine = k.Engine, Backend = k.Backend, Family = k.Family,
                    Notes = k.Notes, SuggestedLoader = k.Loader, Installed = false, Source = "Known",
                });
            }
            return result;
        }

        static bool Matches(KnownGame k, GameInfo g)
        {
            if (k.AppId.Length > 0 && g.AppId.Length > 0) return k.AppId == g.AppId;
            if (string.Equals(k.Name, g.Name, StringComparison.OrdinalIgnoreCase)) return true;
            string folder = Path.GetFileName(g.Path ?? "");
            return k.Folders.Any(f => string.Equals(f, folder, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Fills in what the pack knows about a game that detection alone cannot tell.</summary>
        public static void Apply(Pack pack, GameInfo g)
        {
            var k = pack.Games.FirstOrDefault(x => Matches(x, g));
            if (k == null) return;
            g.Family = k.Family;
            g.Notes = k.Notes;
            g.SuggestedLoader = k.Loader;
            if (g.Engine.Length == 0) g.Engine = k.Engine;
            if (g.Backend.Length == 0) g.Backend = k.Backend;
        }

        static IEnumerable<GameInfo> Steam()
        {
            var libs = new List<string>();
            try
            {
                string steam = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string
                    ?? Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null) as string;
                if (!string.IsNullOrEmpty(steam))
                {
                    steam = steam.Replace('/', '\\');
                    libs.Add(steam);
                    string vdf = Path.Combine(steam, "steamapps", "libraryfolders.vdf");
                    if (File.Exists(vdf))
                        foreach (Match m in Regex.Matches(File.ReadAllText(vdf), "\"path\"\\s+\"([^\"]+)\""))
                            libs.Add(m.Groups[1].Value.Replace("\\\\", "\\"));
                }
            }
            catch (Exception) { }

            foreach (var lib in libs.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                string apps = Path.Combine(lib, "steamapps");
                if (!Directory.Exists(apps)) continue;
                string[] manifests;
                try { manifests = Directory.GetFiles(apps, "appmanifest_*.acf"); }
                catch (Exception) { continue; }
                foreach (var acf in manifests)
                {
                    GameInfo g = null;
                    try
                    {
                        string text = File.ReadAllText(acf);
                        string id = Field(text, "appid"), name = Field(text, "name"), dir = Field(text, "installdir");
                        if (id.Length == 0 || dir.Length == 0 || SkipApps.Contains(id)) continue;
                        if (name.StartsWith("Proton", StringComparison.Ordinal) || name.StartsWith("Steam Linux Runtime", StringComparison.Ordinal)) continue;
                        string path = Path.Combine(apps, "common", dir);
                        if (!Directory.Exists(path)) continue;
                        g = EngineDetector.Inspect(path, name, id, "Steam");
                    }
                    catch (Exception) { }
                    if (g != null) yield return g;
                }
            }
        }

        static string Field(string acf, string key)
        {
            var m = Regex.Match(acf, "\"" + key + "\"\\s+\"([^\"]*)\"", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : "";
        }

        static IEnumerable<GameInfo> Epic()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Epic", "EpicGamesLauncher", "Data", "Manifests");
            if (!Directory.Exists(dir)) yield break;
            string[] items;
            try { items = Directory.GetFiles(dir, "*.item"); }
            catch (Exception) { yield break; }
            foreach (var item in items)
            {
                GameInfo g = null;
                try
                {
                    var d = Json.ParseObject(File.ReadAllText(item));
                    string path = Json.Str(d, "InstallLocation"), name = Json.Str(d, "DisplayName");
                    if (path.Length > 0 && name.Length > 0 && Directory.Exists(path))
                        g = EngineDetector.Inspect(path, name, "", "Epic");
                }
                catch (Exception) { }
                if (g != null) yield return g;
            }
        }
    }
}
