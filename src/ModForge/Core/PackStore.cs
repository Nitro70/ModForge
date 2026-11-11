using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModForge.Core
{
    /// <summary>Reads template packs from the exe, a downloaded zip, or a folder. Layout: docs/PACK-FORMAT.md.</summary>
    public static class PackStore
    {
        public static string DataDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ModForge"); }
        }

        public static string DownloadedZip { get { return Path.Combine(DataDir, "packs.zip"); } }
        public static string UserPacksDir { get { return Path.Combine(DataDir, "packs"); } }

        public static Pack LoadEmbedded()
        {
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("packs.zip"))
            {
                if (s == null) throw new ForgeException("this build has no templates inside it");
                return LoadZip(s, "built in");
            }
        }

        public static Pack LoadZip(Stream stream, string origin)
        {
            var entries = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                foreach (var e in zip.Entries)
                {
                    string name = e.FullName.Replace('\\', '/');
                    if (name.EndsWith("/", StringComparison.Ordinal)) continue;
                    using (var es = e.Open())
                    using (var ms = new MemoryStream())
                    {
                        es.CopyTo(ms);
                        entries[name] = ms.ToArray();
                    }
                }
            }
            return Build(entries, origin);
        }

        public static Pack LoadDirectory(string dir, string origin)
        {
            var entries = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            string root = Path.GetFullPath(dir).TrimEnd('\\', '/') + "\\";
            foreach (var f in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                entries[f.Substring(root.Length).Replace('\\', '/')] = File.ReadAllBytes(f);
            return Build(entries, origin);
        }

        /// <summary>Built-in pack, replaced by a newer downloaded one, plus any folders the user dropped in.</summary>
        public static Pack LoadAll(out string info)
        {
            Pack pack = LoadEmbedded();
            info = "templates v" + pack.Version + " (built in)";
            try
            {
                if (File.Exists(DownloadedZip))
                {
                    Pack dl;
                    using (var fs = File.OpenRead(DownloadedZip)) dl = LoadZip(fs, "downloaded");
                    if (dl.Version > pack.Version && dl.Templates.Count > 0)
                    {
                        pack = dl;
                        info = "templates v" + pack.Version + " (downloaded)";
                    }
                }
            }
            catch (Exception ex) { info += ", downloaded pack ignored: " + ex.Message; }

            try
            {
                if (Directory.Exists(UserPacksDir) && Directory.EnumerateFileSystemEntries(UserPacksDir).Any())
                {
                    Pack user = LoadDirectory(UserPacksDir, "yours");
                    foreach (var t in user.Templates)
                    {
                        pack.Templates.RemoveAll(x => x.Id == t.Id);
                        pack.Templates.Add(t);
                    }
                    pack.Games.AddRange(user.Games);
                    pack.Errors.AddRange(user.Errors);
                    if (user.Templates.Count > 0) info += " + " + user.Templates.Count + " of your own";
                }
            }
            catch (Exception ex) { info += ", your packs folder ignored: " + ex.Message; }

            Sort(pack);
            return pack;
        }

        static void Sort(Pack pack)
        {
            pack.Templates = pack.Templates
                .OrderBy(t => { int i = pack.EngineOrder.IndexOf(t.Engine); return i < 0 ? 999 : i; })
                .ThenBy(t => t.Order).ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        static Pack Build(Dictionary<string, byte[]> entries, string origin)
        {
            // A GitHub source zip wraps everything in "repo-branch/packs/": find pack.json and treat its folder as the root.
            string marker = entries.Keys.Where(k => k == "pack.json" || k.EndsWith("/pack.json", StringComparison.Ordinal))
                .OrderBy(k => k.Length).FirstOrDefault();
            string prefix = marker == null ? "" : marker.Substring(0, marker.Length - "pack.json".Length);

            var pack = new Pack { Name = origin };
            var byTemplate = new Dictionary<string, TemplateDef>(StringComparer.Ordinal);
            var manifests = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var kv in entries)
            {
                if (!kv.Key.StartsWith(prefix, StringComparison.Ordinal)) continue;
                string rel = kv.Key.Substring(prefix.Length);
                if (rel == "pack.json")
                {
                    var d = Json.ParseObject(Text(kv.Value));
                    pack.Version = Json.Int(d, "version");
                    pack.Name = Json.Str(d, "name", origin);
                    pack.EngineOrder = Json.Arr(d, "engines").Select(o => Convert.ToString(o)).ToList();
                    continue;
                }
                if (rel == "games.json")
                {
                    ReadGames(pack, Text(kv.Value));
                    continue;
                }
                int slash = rel.IndexOf('/');
                if (slash < 0) continue;
                string id = rel.Substring(0, slash);
                string rest = rel.Substring(slash + 1);
                if (rest == "template.json") manifests[id] = Text(kv.Value);
                else if (rest == "games.json") ReadGames(pack, Text(kv.Value));
                else if (rest.StartsWith("files/", StringComparison.Ordinal))
                {
                    TemplateDef t;
                    if (!byTemplate.TryGetValue(id, out t)) byTemplate[id] = t = new TemplateDef { Id = id };
                    t.Files[rest.Substring("files/".Length)] = kv.Value;
                }
            }

            foreach (var m in manifests)
            {
                TemplateDef t;
                if (!byTemplate.TryGetValue(m.Key, out t)) t = new TemplateDef { Id = m.Key };
                try { ReadManifest(t, m.Value); }
                catch (Exception ex) { pack.Errors.Add(m.Key + "/template.json: " + ex.Message); continue; }
                t.Origin = origin;
                pack.Templates.Add(t);
            }
            Sort(pack);
            return pack;
        }

        public static string Text(byte[] data)
        {
            if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
                return Encoding.UTF8.GetString(data, 3, data.Length - 3);
            return Encoding.UTF8.GetString(data);
        }

        static void ReadGames(Pack pack, string json)
        {
            var d = Json.ParseObject(json);
            foreach (var o in Json.Arr(d, "games"))
            {
                var g = o as Dictionary<string, object>;
                if (g == null) continue;
                pack.Games.Add(new KnownGame
                {
                    Name = Json.Str(g, "name"),
                    AppId = Json.Str(g, "appid"),
                    Engine = Json.Str(g, "engine"),
                    Backend = Json.Str(g, "backend"),
                    Family = Json.Str(g, "family"),
                    Loader = Json.Str(g, "loader"),
                    Notes = Json.Str(g, "notes"),
                    Folders = Json.Arr(g, "folders").Select(x => Convert.ToString(x)).ToList(),
                });
            }
        }

        static void ReadManifest(TemplateDef t, string json)
        {
            var d = Json.ParseObject(json);
            string id = Json.Str(d, "id", t.Id);
            if (id != t.Id) throw new ForgeException("\"id\" must match the folder name '" + t.Id + "'");
            t.Name = Json.Str(d, "name", t.Id);
            t.Engine = Json.Str(d, "engine", "Other");
            t.Loader = Json.Str(d, "loader");
            t.Language = Json.Str(d, "language");
            t.Summary = Json.Str(d, "summary");
            t.Description = Json.Str(d, "description");
            t.DocsUrl = Json.Str(d, "docs");
            t.Order = Json.Int(d, "order", 100);
            t.Eol = Json.Str(d, "eol", "crlf").ToLowerInvariant();
            t.Folder = Json.Str(d, "folder", "{{ModName}}");
            t.Main = Json.Str(d, "main");

            var match = Json.Obj(d, "match");
            t.MatchEngine = Json.Str(match, "engine");
            t.MatchBackend = Json.Str(match, "backend");
            t.MatchFamily = Json.Str(match, "family");
            t.MatchLoader = Json.Str(match, "loader");

            foreach (var o in Json.Arr(d, "options"))
            {
                var od = o as Dictionary<string, object>;
                if (od == null) throw new ForgeException("every option must be an object");
                var opt = new TemplateOption
                {
                    Id = Json.Str(od, "id"),
                    Label = Json.Str(od, "label"),
                    Type = Json.Str(od, "type", "text"),
                    Default = Json.Str(od, "default"),
                    Help = Json.Str(od, "help"),
                    Group = Json.Str(od, "group"),
                    Required = Json.Bool(od, "required"),
                    Pattern = Json.Str(od, "pattern"),
                    PatternHelp = Json.Str(od, "patternHelp"),
                    From = Json.Str(od, "from"),
                    ShowWhen = Json.Str(od, "showWhen"),
                    Remember = Json.Bool(od, "remember"),
                };
                if (string.IsNullOrEmpty(opt.Id)) throw new ForgeException("an option has no \"id\"");
                if (string.IsNullOrEmpty(opt.Label)) opt.Label = opt.Id;
                switch (opt.Type)
                {
                    case "text": case "bool": case "choice": case "folder": case "guid": break;
                    default: throw new ForgeException("option '" + opt.Id + "' has unknown type '" + opt.Type + "'");
                }
                foreach (var c in Json.Arr(od, "choices"))
                {
                    var cd = c as Dictionary<string, object>;
                    if (cd != null) opt.Choices.Add(new Choice { Value = Json.Str(cd, "value"), Label = Json.Str(cd, "label", Json.Str(cd, "value")) });
                    else opt.Choices.Add(new Choice { Value = Convert.ToString(c), Label = Convert.ToString(c) });
                }
                if (opt.Type == "choice" && opt.Choices.Count == 0) throw new ForgeException("choice option '" + opt.Id + "' has no choices");
                if (opt.Type == "choice" && string.IsNullOrEmpty(opt.Default)) opt.Default = opt.Choices[0].Value;
                if (t.Options.Any(x => x.Id == opt.Id)) throw new ForgeException("option '" + opt.Id + "' is listed twice");
                t.Options.Add(opt);
            }

            var derived = Json.Obj(d, "derived");
            if (derived != null)
                foreach (var kv in derived) t.Derived.Add(new KeyValuePair<string, string>(kv.Key, Convert.ToString(kv.Value)));

            var when = Json.Obj(d, "when");
            if (when != null)
                foreach (var kv in when) t.When.Add(new KeyValuePair<string, string>(kv.Key, Convert.ToString(kv.Value)));

            t.Next = Json.Arr(d, "next").Select(o => Convert.ToString(o)).ToList();
        }
    }
}
