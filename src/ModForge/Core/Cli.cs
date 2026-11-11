using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ModForge.Core
{
    /// <summary>
    /// ModForge.exe --cli list | scan | validate [--packs dir] [id...] | generate id --out dir [--zip file] [--set K=V] [--game folder] [--packs dir]
    /// The same engine the window uses, with no window. Used to test packs.
    /// </summary>
    public static class Cli
    {
        public static int Run(string[] args)
        {
            try
            {
                if (args.Length == 0) return Usage();
                var rest = args.Skip(1).ToList();
                string packsDir = Take(rest, "--packs");
                string info = "";
                Pack pack = packsDir != null ? PackStore.LoadDirectory(packsDir, packsDir) : PackStore.LoadAll(out info);

                switch (args[0])
                {
                    case "list":
                        foreach (var t in pack.Templates)
                            Console.WriteLine("{0,-28} {1,-12} {2}", t.Id, t.Engine, t.Name);
                        Console.WriteLine(pack.Templates.Count + " templates, pack v" + pack.Version + " " + info);
                        return 0;
                    case "update":
                        Console.WriteLine(PackUpdater.Update(rest.Count > 0 ? rest[0] : Settings.Load().PackUrl, pack.Version));
                        return 0;
                    case "scan":
                        foreach (var g in GameScanner.Scan(pack, Settings.Load().ExtraGames).Where(g => g.Installed))
                            Console.WriteLine("{0,-40} {1,-7} {2,-7} {3,-10} {4,-10} {5}", Cut(g.Name, 40), g.Engine, g.Backend, g.EngineVersion, string.Join("+", g.Loaders), g.Path);
                        return 0;
                    case "validate": return Validate(pack, rest);
                    case "generate": return Generate(pack, rest);
                    default: return Usage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: " + ex.Message);
                return 2;
            }
        }

        static int Usage()
        {
            Console.WriteLine("ModForge --cli list | scan | validate [--packs dir] [id...] | generate <id> --out <dir> [--zip <file>] [--set K=V]... [--game <folder>] [--packs dir]");
            return 1;
        }

        static string Cut(string s, int n) { return s.Length <= n ? s : s.Substring(0, n - 1) + "~"; }

        static string Take(List<string> args, string flag)
        {
            int i = args.IndexOf(flag);
            if (i < 0 || i + 1 >= args.Count) return null;
            string v = args[i + 1];
            args.RemoveRange(i, 2);
            return v;
        }

        static int Generate(Pack pack, List<string> args)
        {
            string outDir = Take(args, "--out"), zip = Take(args, "--zip"), gamePath = Take(args, "--game");
            var values = new Dictionary<string, string>();
            string set;
            while ((set = Take(args, "--set")) != null)
            {
                int eq = set.IndexOf('=');
                if (eq < 1) throw new ForgeException("--set needs Name=Value");
                values[set.Substring(0, eq)] = set.Substring(eq + 1);
            }
            if (args.Count != 1 || (outDir == null && zip == null)) return Usage();
            var t = pack.Templates.FirstOrDefault(x => x.Id == args[0]);
            if (t == null) throw new ForgeException("no template called " + args[0]);

            GameInfo game = null;
            if (gamePath != null)
            {
                game = EngineDetector.Inspect(gamePath, Path.GetFileName(gamePath.TrimEnd('\\', '/')), "", "Added");
                GameScanner.Apply(pack, game);
            }
            var vars = Generator.BuildVars(t, values, game);
            var errors = Generator.Validate(t, vars);
            if (errors.Count > 0) throw new ForgeException(string.Join("; ", errors.Select(e => e.Key + ": " + e.Value)));
            var files = Generator.Generate(t, vars);
            string folder = Generator.FolderName(t, vars);
            if (outDir != null) Generator.WriteFolder(files, Path.Combine(outDir, folder));
            if (zip != null) Generator.WriteZip(files, zip, folder);
            Console.WriteLine("wrote " + files.Count + " files: " + (outDir != null ? Path.Combine(outDir, folder) : zip));
            return 0;
        }

        // ---- validate: render every template under many option combinations and check what comes out ----

        static int Validate(Pack pack, List<string> ids)
        {
            int bad = 0;
            foreach (var e in pack.Errors) { Console.WriteLine("FAIL  " + e); bad++; }
            var list = ids.Count == 0 ? pack.Templates : pack.Templates.Where(t => ids.Contains(t.Id)).ToList();
            if (list.Count == 0) { Console.WriteLine("no templates to check"); return 1; }
            foreach (var t in list)
            {
                var problems = new List<string>();
                int runs = 0, fileCount = 0;
                string opens = "";
                if (t.Summary.Length == 0) problems.Add("template.json has no \"summary\"");
                if (t.Next.Count == 0) problems.Add("template.json has no \"next\" steps");
                if (t.Files.Count == 0) problems.Add("no files/ folder");
                foreach (var f in t.Files)
                    if (LongDash(PackStore.Text(f.Value))) problems.Add(f.Key + ": contains a long dash");
                foreach (var rule in t.When)
                    if (!t.Files.Keys.Any(k => Generator.Applies(rule.Key, k))) problems.Add("\"when\" key '" + rule.Key + "' matches no file (a pattern with * must match the whole path)");

                foreach (var combo in Combos(t))
                {
                    foreach (var game in new[] { null, FakeGame(t) })
                    {
                        runs++;
                        string label = (game == null ? "no game" : "with game") + " " + string.Join(",", combo.Select(kv => kv.Key + "=" + kv.Value));
                        try
                        {
                            var vars = Generator.BuildVars(t, combo, game);
                            KnownNames = new HashSet<string>(vars.Keys);
                            foreach (var o in t.Options) Generator.IsVisible(o, vars);
                            var errors = Generator.Validate(t, vars);
                            foreach (var e in errors) problems.Add("default of '" + e.Key + "' fails its own check (" + e.Value + ") [" + label + "]");
                            Generator.FolderName(t, vars);
                            var files = Generator.Generate(t, vars);
                            fileCount = Math.Max(fileCount, files.Count);
                            if (runs == 1) opens = Generator.MainFile(t, vars, files);
                            if (t.Main.Length > 0)
                            {
                                string want = Renderer.Render(t.Main, vars, "main").Replace('\\', '/');
                                if (!files.Any(f => f.Path == want)) problems.Add("\"main\" names " + want + ", which is not among the files [" + label + "]");
                            }
                            foreach (var f in files.Where(f => f.IsText)) CheckFile(f, problems, label);
                        }
                        catch (Exception ex) { problems.Add(ex.Message + " [" + label + "]"); }
                    }
                }

                problems = problems.Distinct().ToList();
                Console.WriteLine((problems.Count == 0 ? "ok    " : "FAIL  ") + t.Id + "  (" + runs + " renders, up to " + fileCount + " files, opens on " + opens + ")");
                foreach (var p in problems.Take(25)) Console.WriteLine("        " + p);
                if (problems.Count > 0) bad++;
            }
            Console.WriteLine(bad == 0 ? "all " + list.Count + " templates pass" : bad + " of " + list.Count + " templates have problems");
            return bad == 0 ? 0 : 1;
        }

        /// <summary>The names the template being checked can use; one left in the output means it was escaped or skipped by mistake.</summary>
        static HashSet<string> KnownNames = new HashSet<string>();

        static bool LongDash(string s) { return s.IndexOf('\u2014') >= 0 || s.IndexOf('\u2013') >= 0; }

        static GameInfo FakeGame(TemplateDef t)
        {
            return new GameInfo
            {
                Name = "Test Game", Path = @"C:\Games\Test Game", AppId = "123456", Installed = true,
                Engine = t.MatchEngine, Backend = t.MatchBackend.Length > 0 ? t.MatchBackend : "mono", Family = t.MatchFamily,
                ExeName = "TestGame", DataDir = "TestGame_Data", EngineVersion = t.MatchEngine == "unreal" ? "5.3" : "2022.3.10",
                Arch = "x64", UnrealProject = "TestGame", UnrealBinDir = @"TestGame\Binaries\Win64",
            };
        }

        /// <summary>Defaults, everything on, everything off, then each bool flipped alone and each choice value alone.</summary>
        static IEnumerable<Dictionary<string, string>> Combos(TemplateDef t)
        {
            yield return new Dictionary<string, string>();
            var bools = t.Options.Where(o => o.Type == "bool").ToList();
            if (bools.Count > 0)
            {
                yield return bools.ToDictionary(o => o.Id, o => "true");
                yield return bools.ToDictionary(o => o.Id, o => "");
                foreach (var b in bools)
                    yield return new Dictionary<string, string> { { b.Id, Renderer.Truthy(b.Default) ? "" : "true" } };
            }
            foreach (var c in t.Options.Where(o => o.Type == "choice"))
                foreach (var ch in c.Choices)
                {
                    yield return new Dictionary<string, string> { { c.Id, ch.Value } };
                    if (bools.Count > 0)
                    {
                        var all = bools.ToDictionary(o => o.Id, o => "true");
                        all[c.Id] = ch.Value;
                        yield return all;
                    }
                }
        }

        static void CheckFile(GeneratedFile f, List<string> problems, string label)
        {
            string ext = Path.GetExtension(f.Path).ToLowerInvariant();
            string name = Path.GetFileName(f.Path).ToLowerInvariant();
            if (LongDash(f.Text)) problems.Add(f.Path + ": contains a long dash [" + label + "]");
            if (f.Text.IndexOf("{{#", StringComparison.Ordinal) >= 0 || f.Text.IndexOf("{{/", StringComparison.Ordinal) >= 0)
                problems.Add(f.Path + ": a block tag was left in the output [" + label + "]");
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(f.Text, @"\{\{\s*([A-Za-z_][A-Za-z0-9_]*)"))
                if (KnownNames.Contains(m.Groups[1].Value))
                    problems.Add(f.Path + ": {{" + m.Groups[1].Value + "}} was not filled in [" + label + "]");
            try
            {
                if (ext == ".json" || ext == ".mcmeta" || ext == ".uplugin")
                {
                    if (f.Text.Trim().Length > 0) Json.Parse(StripJsonComments(f.Text, name));
                }
                else if (ext == ".xml" || ext == ".csproj" || ext == ".props" || ext == ".targets" || ext == ".config" || ext == ".vcxproj" || ext == ".nuspec")
                {
                    var doc = new XmlDocument { XmlResolver = null };
                    doc.LoadXml(f.Text);
                }
            }
            catch (Exception ex) { problems.Add(f.Path + ": does not parse: " + ex.Message + " [" + label + "]"); }
        }

        /// <summary>A few JSON files (VS Code settings, tsconfig) officially allow // comments; allow them there only.</summary>
        static string StripJsonComments(string text, string name)
        {
            if (!(name == "launch.json" || name == "tasks.json" || name == "settings.json" || name == "extensions.json" || name == "tsconfig.json" || name == "jsconfig.json"))
                return text;
            return string.Join("\n", text.Replace("\r", "").Split('\n').Where(l => !l.TrimStart().StartsWith("//", StringComparison.Ordinal)));
        }
    }
}
