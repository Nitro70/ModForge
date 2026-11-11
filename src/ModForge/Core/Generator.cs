using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ModForge.Core
{
    /// <summary>Turns a template plus the user's answers into a list of files, then writes a folder or a zip.</summary>
    public static class Generator
    {
        static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);
        static readonly char[] BadNameChars = Path.GetInvalidFileNameChars();

        public static string DefaultFor(TemplateOption o, IDictionary<string, string> gameVars)
        {
            string v;
            if (!string.IsNullOrEmpty(o.From) && gameVars != null && gameVars.TryGetValue(o.From, out v) && !string.IsNullOrEmpty(v))
            {
                if (o.Type != "choice") return v;
                // A choice takes the game value when it is one of its values, or the value it starts with
                // ("4.27" picks a choice whose value is "4"); otherwise the template's own default.
                var hit = o.Choices.FirstOrDefault(c => string.Equals(c.Value, v, StringComparison.OrdinalIgnoreCase))
                    ?? o.Choices.Where(c => c.Value.Length > 0 && v.StartsWith(c.Value + ".", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(c => c.Value.Length).FirstOrDefault();
                if (hit != null) return hit.Value;
            }
            if (o.Type == "guid") return Guid.NewGuid().ToString();
            if (o.Type == "bool") return Renderer.Truthy(o.Default) ? "true" : "";
            return o.Default ?? "";
        }

        /// <summary>Every name a template can use: game facts, a few built-ins, the option values, then the derived names.</summary>
        public static Dictionary<string, string> BuildVars(TemplateDef t, IDictionary<string, string> values, GameInfo game)
        {
            var gameVars = game != null ? game.ToVars() : GameInfo.EmptyVars();
            var vars = new Dictionary<string, string>(gameVars, StringComparer.Ordinal);
            vars["Year"] = DateTime.Now.Year.ToString();
            vars["Date"] = DateTime.Now.ToString("yyyy-MM-dd");
            vars["TemplateId"] = t.Id;
            vars["TemplateName"] = t.Name;

            foreach (var o in t.Options)
            {
                string v;
                if (values == null || !values.TryGetValue(o.Id, out v) || v == null) v = DefaultFor(o, gameVars);
                if (o.Type == "bool") v = Renderer.Truthy(v) ? "true" : "";
                else v = v.Trim();
                vars[o.Id] = v;
            }

            // Derived names may use each other in any order; keep resolving until nothing is left.
            var pending = t.Derived.ToList();
            while (pending.Count > 0)
            {
                int before = pending.Count;
                string lastError = null;
                foreach (var kv in pending.ToList())
                {
                    try
                    {
                        vars[kv.Key] = Renderer.Render(kv.Value, vars, "derived " + kv.Key);
                        pending.Remove(kv);
                    }
                    catch (ForgeException ex) { lastError = ex.Message; }
                }
                if (pending.Count == before) throw new ForgeException(lastError ?? "derived names depend on each other in a loop");
            }
            return vars;
        }

        /// <summary>Problems with what the user typed, keyed by option id. Empty when it is all fine.</summary>
        public static Dictionary<string, string> Validate(TemplateDef t, IDictionary<string, string> vars)
        {
            var errors = new Dictionary<string, string>();
            foreach (var o in t.Options)
            {
                if (!IsVisible(o, vars)) continue;
                string v;
                vars.TryGetValue(o.Id, out v);
                v = v ?? "";
                if (o.Required && v.Length == 0) { errors[o.Id] = "needed"; continue; }
                if (v.Length > 0 && !string.IsNullOrEmpty(o.Pattern) && !Regex.IsMatch(v, "^(?:" + o.Pattern + ")$"))
                    errors[o.Id] = string.IsNullOrEmpty(o.PatternHelp) ? "not a valid value" : o.PatternHelp;
            }
            return errors;
        }

        public static bool IsVisible(TemplateOption o, IDictionary<string, string> vars)
        {
            if (o.Type == "guid") return false;
            if (string.IsNullOrEmpty(o.ShowWhen)) return true;
            return Renderer.Eval(o.ShowWhen, vars, "showWhen of " + o.Id);
        }

        public static string FolderName(TemplateDef t, IDictionary<string, string> vars)
        {
            string name = Renderer.Render(t.Folder, vars, "folder").Trim();
            name = new string(name.Select(c => BadNameChars.Contains(c) ? '_' : c).ToArray()).Trim('.', ' ');
            return name.Length == 0 ? "Mod" : name;
        }

        public static List<GeneratedFile> Generate(TemplateDef t, IDictionary<string, string> vars)
        {
            var result = new List<GeneratedFile>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var src in t.Files)
            {
                if (!Included(t, src.Key, vars)) continue;

                var segs = src.Key.Split('/').Select(s => Renderer.Render(s, vars, src.Key).Trim()).ToArray();
                int last = segs.Length - 1;
                if (segs[last].EndsWith(".tpl", StringComparison.OrdinalIgnoreCase)) segs[last] = segs[last].Substring(0, segs[last].Length - 4);
                foreach (var s in segs)
                {
                    if (s.Length == 0 || s == "." || s == ".." || s.IndexOfAny(BadNameChars) >= 0 || s.EndsWith(".", StringComparison.Ordinal))
                        throw new ForgeException(src.Key + ": makes the file name '" + string.Join("/", segs) + "', which Windows does not allow");
                }
                string path = string.Join("/", segs);
                if (!seen.Add(path)) throw new ForgeException(src.Key + ": two files end up named '" + path + "'");

                var gf = new GeneratedFile { Path = path };
                if (IsText(src.Value))
                {
                    string text = Renderer.Render(PackStore.Text(src.Value), vars, src.Key);
                    text = text.Replace("\r\n", "\n").Replace("\r", "\n");
                    if (!UsesLf(t, path)) text = text.Replace("\n", "\r\n");
                    gf.IsText = true;
                    gf.Text = text;
                    gf.Data = Utf8NoBom.GetBytes(text);
                }
                else gf.Data = src.Value;
                result.Add(gf);
            }
            if (result.Count == 0) throw new ForgeException("the template produced no files");
            return result.OrderBy(f => f.Path, StringComparer.OrdinalIgnoreCase).ToList();
        }

        static readonly string[] EntryNames = { "plugin.cs", "modentry.cs", "main.lua", "mod_main.gd", "dllmain.cpp", "main.cpp", "control.lua", "main.js" };
        static readonly string[] CodeExts = { ".cs", ".java", ".kt", ".lua", ".gd", ".cpp", ".js", ".ts", ".mcfunction" };

        /// <summary>The file the preview opens on: the template's "main" if it names one, else the most likely entry point.</summary>
        public static string MainFile(TemplateDef t, IDictionary<string, string> vars, List<GeneratedFile> files)
        {
            if (!string.IsNullOrEmpty(t.Main))
            {
                string want = Renderer.Render(t.Main, vars, "main").Replace('\\', '/');
                if (files.Any(f => f.Path == want)) return want;
            }
            Func<GeneratedFile, string> name = f => f.Path.Substring(f.Path.LastIndexOf('/') + 1).ToLowerInvariant();
            var hit = files.FirstOrDefault(f => EntryNames.Contains(name(f)))
                ?? files.FirstOrDefault(f => name(f).StartsWith("main.", StringComparison.Ordinal) && CodeExts.Any(e => f.Path.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                ?? files.Where(f => CodeExts.Any(e => f.Path.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                        .OrderBy(f => f.Path.Count(c => c == '/')).FirstOrDefault()
                ?? files.FirstOrDefault(f => !name(f).StartsWith(".", StringComparison.Ordinal) && name(f) != "readme.md")
                ?? files.FirstOrDefault();
            return hit != null ? hit.Path : "";
        }

        static bool UsesLf(TemplateDef t, string path)
        {
            string name = path.Substring(path.LastIndexOf('/') + 1);
            if (name.EndsWith(".sh", StringComparison.OrdinalIgnoreCase) || name == "gradlew") return true;
            if (name.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase)) return false;
            return t.Eol == "lf";
        }

        static bool IsText(byte[] data)
        {
            int n = Math.Min(data.Length, 8000);
            for (int i = 0; i < n; i++) if (data[i] == 0) return false;
            return true;
        }

        static bool Included(TemplateDef t, string src, IDictionary<string, string> vars)
        {
            foreach (var rule in t.When)
            {
                if (!Applies(rule.Key, src)) continue;
                if (!Renderer.Eval(rule.Value, vars, "when " + rule.Key)) return false;
            }
            return true;
        }

        public static bool Applies(string key, string src)
        {
            if (key.IndexOf('*') >= 0)
                return Regex.IsMatch(src, "^" + Regex.Escape(key).Replace("\\*", ".*") + "$");
            string k = key.TrimEnd('/');
            return src == k || src.StartsWith(k + "/", StringComparison.Ordinal);
        }

        public static void WriteFolder(IEnumerable<GeneratedFile> files, string dir)
        {
            if (Directory.Exists(dir) && Directory.EnumerateFileSystemEntries(dir).Any())
                throw new ForgeException("the folder already has files in it: " + dir);
            foreach (var f in files)
            {
                string full = Path.Combine(dir, f.Path.Replace('/', '\\'));
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                File.WriteAllBytes(full, f.Data);
            }
        }

        public static void WriteZip(IEnumerable<GeneratedFile> files, string zipPath, string rootFolder)
        {
            string tmp = zipPath + ".tmp";
            if (File.Exists(tmp)) File.Delete(tmp);
            using (var fs = File.Create(tmp))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                foreach (var f in files)
                {
                    var e = zip.CreateEntry(rootFolder + "/" + f.Path, CompressionLevel.Optimal);
                    using (var es = e.Open()) es.Write(f.Data, 0, f.Data.Length);
                }
            }
            if (File.Exists(zipPath)) File.Delete(zipPath);
            File.Move(tmp, zipPath);
        }
    }
}
