using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ModForge.Core;

namespace ModForge
{
    public abstract class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            Raise(name);
            return true;
        }

        protected void Raise(string name)
        {
            var h = PropertyChanged;
            if (h != null) h(this, new PropertyChangedEventArgs(name));
        }
    }

    public sealed class RelayCommand : ICommand
    {
        readonly Action<object> run;
        public RelayCommand(Action<object> run) { this.run = run; }
        public RelayCommand(Action run) { this.run = _ => run(); }
        public event EventHandler CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object parameter) { return true; }
        public void Execute(object parameter) { run(parameter); }
    }

    public sealed class GameVM
    {
        public GameInfo Info;
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Initial { get; set; }
        public Brush Badge { get; set; }
        public bool IsInstalled { get; set; }
        public double Fade { get { return IsInstalled || Info == null ? 1.0 : 0.5; } }
        public string Tip { get; set; }

        static Brush Solid(string hex)
        {
            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            b.Freeze();
            return b;
        }

        static readonly Brush Unity = Solid("#3D6BE0"), Unreal = Solid("#8A5CF0"), Godot = Solid("#2FA39B"), Other = Solid("#4A5266"), Any = Solid("#C2783A");

        public static GameVM AnyGame()
        {
            return new GameVM { Info = null, Name = "Any game", Tag = "show every template", Initial = "*", Badge = Any, IsInstalled = true, Tip = "No game picked: paths are left for you to fill in." };
        }

        public static GameVM From(GameInfo g)
        {
            string tag;
            Brush badge = Other;
            switch (g.Engine)
            {
                case "unity":
                    tag = "Unity" + Short(g.EngineVersion) + (g.Backend == "il2cpp" ? "  IL2CPP" : g.Backend == "mono" ? "  Mono" : "");
                    badge = Unity;
                    break;
                case "unreal": tag = "Unreal" + Short(g.EngineVersion); badge = Unreal; break;
                case "godot": tag = "Godot" + Short(g.EngineVersion); badge = Godot; break;
                default: tag = g.Family.Length > 0 ? "own mod system" : "engine not recognised"; break;
            }
            if (g.Loaders.Count > 0) tag += "  +  " + string.Join(", ", g.Loaders);
            if (!g.Installed) tag += "  (not installed)";
            string tip = g.Installed ? g.Path : "Not found on this PC. Templates still work; the game folder is left for you to fill in.";
            if (g.Notes.Length > 0) tip += "\n" + g.Notes;
            return new GameVM
            {
                Info = g, Name = g.Name, Tag = tag, Badge = badge, IsInstalled = g.Installed, Tip = tip,
                Initial = g.Name.Length > 0 ? g.Name.Substring(0, 1).ToUpperInvariant() : "?",
            };
        }

        static string Short(string v)
        {
            if (string.IsNullOrEmpty(v)) return "";
            var p = v.Split('.');
            return " " + (p.Length >= 2 ? p[0] + "." + p[1] : v);
        }
    }

    public sealed class TemplateVM
    {
        public TemplateDef Def;
        public string Name { get { return Def.Name; } }
        public string Meta { get; set; }
        public string Summary { get { return Def.Summary; } }
        public string Badge { get; set; }
        public bool HasBadge { get { return !string.IsNullOrEmpty(Badge); } }
        public int Score;
    }

    public sealed class ChipVM : Observable
    {
        readonly Action<ChipVM> picked;
        public ChipVM(string label, Action<ChipVM> picked) { Label = label; this.picked = picked; }
        public string Label { get; private set; }
        bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { if (Set(ref isSelected, value) && value) picked(this); }
        }
    }

    public sealed class ChoiceVM : Observable
    {
        readonly OptionVM owner;
        public ChoiceVM(OptionVM owner, Choice c) { this.owner = owner; Value = c.Value; Label = c.Label; }
        public string Value { get; private set; }
        public string Label { get; private set; }
        public string GroupName { get { return "opt_" + owner.Def.Id; } }
        public bool IsSelected
        {
            get { return owner.Value == Value; }
            set { if (value) owner.Value = Value; }
        }
        public void Touch() { Raise("IsSelected"); }
    }

    public sealed class OptionVM : Observable
    {
        readonly MainVM main;
        public TemplateOption Def { get; private set; }
        public List<ChoiceVM> Choices { get; private set; }
        public ICommand Browse { get; private set; }

        public OptionVM(MainVM main, TemplateOption def, string value)
        {
            this.main = main;
            Def = def;
            this.value = value ?? "";
            Choices = def.Choices.Select(c => new ChoiceVM(this, c)).ToList();
            Browse = new RelayCommand(() =>
            {
                string p = main.PickFolder(Def.Label, Value);
                if (p != null) Value = p;
            });
        }

        public string Kind { get { return Def.Type; } }
        public string Label { get { return Def.Label; } }
        public string Help { get { return Def.Help; } }
        public bool HasHelp { get { return Def.Help.Length > 0; } }

        string value;
        public string Value
        {
            get { return value; }
            set
            {
                if (!Set(ref this.value, value ?? "")) return;
                Raise("IsOn");
                foreach (var c in Choices) c.Touch();
                main.OptionEdited(this);
            }
        }

        public bool IsOn
        {
            get { return Renderer.Truthy(value); }
            set { Value = value ? "true" : ""; }
        }

        /// <summary>Sets the value without counting it as something the user typed.</summary>
        public void SetQuietly(string v)
        {
            value = v ?? "";
            Raise("Value");
            Raise("IsOn");
            foreach (var c in Choices) c.Touch();
        }

        string error = "";
        public string Error { get { return error; } set { if (Set(ref error, value)) Raise("HasError"); } }
        public bool HasError { get { return error.Length > 0; } }

        bool isVisible = true;
        public bool IsVisible { get { return isVisible; } set { Set(ref isVisible, value); } }

        string groupHeader = "";
        public string GroupHeader { get { return groupHeader; } set { if (Set(ref groupHeader, value)) Raise("HasGroupHeader"); } }
        public bool HasGroupHeader { get { return groupHeader.Length > 0; } }
    }

    public sealed class FileVM
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public Thickness Indent { get; set; }
        public bool IsFolder { get; set; }
        public string Glyph { get { return IsFolder ? "" : ""; } }
        public GeneratedFile File;
    }

    public sealed class MainVM : Observable
    {
        readonly Settings settings;
        Pack pack;
        List<GameVM> allGames = new List<GameVM>();
        readonly Dictionary<string, string> sticky = new Dictionary<string, string>();
        readonly Dictionary<string, string> guids = new Dictionary<string, string>();
        List<GeneratedFile> generated = new List<GeneratedFile>();
        string folderName = "";
        bool building;

        /// <summary>Set by the window: shows a folder dialog and returns the path, or null.</summary>
        public Func<string, string, string> PickFolder = (title, start) => null;
        public Func<string, string, string> PickZipPath = (dir, name) => null;

        public ObservableCollection<GameVM> Games { get; private set; }
        public ObservableCollection<ChipVM> Engines { get; private set; }
        public ObservableCollection<TemplateVM> Templates { get; private set; }
        public ObservableCollection<OptionVM> Options { get; private set; }
        public ObservableCollection<FileVM> Files { get; private set; }
        public ObservableCollection<string> NextSteps { get; private set; }

        public ICommand BrowseOutput { get; private set; }
        public ICommand CreateFolder { get; private set; }
        public ICommand CreateZip { get; private set; }
        public ICommand AddGame { get; private set; }
        public ICommand Rescan { get; private set; }
        public ICommand OpenDocs { get; private set; }
        public ICommand OpenCreated { get; private set; }
        public ICommand CloseDone { get; private set; }
        public ICommand ToggleSource { get; private set; }
        public ICommand UpdatePacks { get; private set; }
        public ICommand OpenUserPacks { get; private set; }

        public MainVM(Pack pack, string packInfo, Settings settings, bool synchronous)
        {
            this.pack = pack;
            this.settings = settings;
            this.packInfo = packInfo;
            Games = new ObservableCollection<GameVM>();
            Engines = new ObservableCollection<ChipVM>();
            Templates = new ObservableCollection<TemplateVM>();
            Options = new ObservableCollection<OptionVM>();
            Files = new ObservableCollection<FileVM>();
            NextSteps = new ObservableCollection<string>();
            outputDir = settings.OutputDir;
            packUrl = settings.PackUrl;

            BrowseOutput = new RelayCommand(() => { string p = PickFolder("Where to create projects", OutputDir); if (p != null) OutputDir = p; });
            CreateFolder = new RelayCommand(() => Create(false));
            CreateZip = new RelayCommand(() => Create(true));
            AddGame = new RelayCommand(AddGameFolder);
            Rescan = new RelayCommand(() => ScanGames(false));
            OpenDocs = new RelayCommand(() => Open(SelectedTemplate != null ? SelectedTemplate.Def.DocsUrl : ""));
            OpenCreated = new RelayCommand(() => { Open(createdPath); ShowDone = false; });
            CloseDone = new RelayCommand(() => ShowDone = false);
            ToggleSource = new RelayCommand(() => ShowSource = !ShowSource);
            UpdatePacks = new RelayCommand(UpdateTemplates);
            OpenUserPacks = new RelayCommand(() => { try { Directory.CreateDirectory(PackStore.UserPacksDir); } catch (Exception) { } Open(PackStore.UserPacksDir); });

            BuildEngines();
            // Known games show at once; installed ones replace them when the scan finishes.
            allGames = pack.Games.Select(k => GameVM.From(new GameInfo
            {
                Name = k.Name, AppId = k.AppId, Engine = k.Engine, Backend = k.Backend, Family = k.Family,
                Notes = k.Notes, SuggestedLoader = k.Loader, Installed = false, Source = "Known",
            })).ToList();
            FilterGames();
            ScanGames(synchronous);
        }

        // ---------- games ----------

        string gameSearch = "";
        public string GameSearch { get { return gameSearch; } set { if (Set(ref gameSearch, value ?? "")) FilterGames(); } }

        string gamesNote = "looking for installed games...";
        public string GamesNote { get { return gamesNote; } set { Set(ref gamesNote, value); } }

        GameVM selectedGame;
        public GameVM SelectedGame
        {
            get { return selectedGame; }
            set
            {
                if (!Set(ref selectedGame, value) || building) return;
                if (value != null && Engines.Count > 0 && !Engines[0].IsSelected) { building = true; SelectChip(Engines[0]); building = false; }
                RebuildTemplates();
            }
        }

        GameInfo Game { get { return selectedGame != null ? selectedGame.Info : null; } }

        void ScanGames(bool synchronous)
        {
            GamesNote = "looking for installed games...";
            var extra = settings.ExtraGames;
            var p = pack;
            if (synchronous) { GamesFound(GameScanner.Scan(p, extra)); return; }
            Task.Run(() =>
            {
                List<GameInfo> found;
                try { found = GameScanner.Scan(p, extra); }
                catch (Exception) { found = new List<GameInfo>(); }
                Application.Current.Dispatcher.BeginInvoke(new Action(() => GamesFound(found)));
            });
        }

        void GamesFound(List<GameInfo> found)
        {
            string keep = selectedGame != null && selectedGame.Info != null ? selectedGame.Info.Name : null;
            // Games we have templates for come first; installed programs nobody mods sink to the bottom.
            allGames = found
                .OrderBy(g => g.Engine.Length > 0 || g.Family.Length > 0 ? (g.Installed ? 0 : 1) : 2)
                .ThenBy(g => g.Name, StringComparer.OrdinalIgnoreCase).Select(GameVM.From).ToList();
            int n = found.Count(g => g.Installed);
            GamesNote = n + " installed, " + (found.Count - n) + " more known";
            FilterGames();
            if (keep != null)
            {
                var again = Games.FirstOrDefault(g => g.Info != null && g.Info.Name == keep);
                if (again != null) SelectedGame = again;
            }
        }

        void FilterGames()
        {
            var keep = selectedGame;
            building = true;
            Games.Clear();
            Games.Add(anyGame);
            string q = gameSearch.Trim();
            foreach (var g in allGames)
                if (q.Length == 0 || g.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || g.Tag.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                    Games.Add(g);
            building = false;
            // The list was rebuilt, which cleared the selection. A picked game stays picked even when the search hides it.
            if (keep != null) { selectedGame = keep; Raise("SelectedGame"); }
            else SelectedGame = anyGame;
        }

        readonly GameVM anyGame = GameVM.AnyGame();

        void AddGameFolder()
        {
            string p = PickFolder("Pick the folder that holds the game's exe", "");
            if (p == null) return;
            var list = settings.ExtraGames;
            list.Add(p);
            settings.ExtraGames = list;
            settings.Save();
            var g = EngineDetector.Inspect(p, Path.GetFileName(p.TrimEnd('\\', '/')), "", "Added");
            GameScanner.Apply(pack, g);
            var vm = GameVM.From(g);
            allGames.RemoveAll(x => x.Info != null && string.Equals(x.Info.Path, g.Path, StringComparison.OrdinalIgnoreCase));
            allGames.Insert(0, vm);
            gameSearch = ""; Raise("GameSearch");
            FilterGames();
            SelectedGame = vm;
            Status = "Added " + g.Name + (g.Engine.Length > 0 ? " (" + vm.Tag + ")" : ": engine not recognised, every template is shown");
        }

        // ---------- templates ----------

        void BuildEngines()
        {
            Engines.Clear();
            Engines.Add(new ChipVM("All", ChipPicked));
            foreach (var e in pack.Templates.Select(t => t.Engine).Distinct()) Engines.Add(new ChipVM(e, ChipPicked));
            building = true;
            Engines[0].IsSelected = true;
            building = false;
        }

        void SelectChip(ChipVM chip)
        {
            foreach (var c in Engines) c.IsSelected = c == chip;
        }

        void ChipPicked(ChipVM chip)
        {
            foreach (var c in Engines) if (c != chip) c.IsSelected = false;
            if (!building) RebuildTemplates();
        }

        static int Score(TemplateDef t, GameInfo g)
        {
            if (g == null) return 0;
            if (t.MatchFamily.Length > 0) return g.Family == t.MatchFamily ? 100 : -1;
            if (t.MatchEngine.Length > 0)
            {
                if (g.Engine.Length == 0) return g.Family.Length == 0 ? 0 : -1;
                if (g.Engine != t.MatchEngine) return -1;
                if (t.MatchBackend.Length > 0 && g.Backend.Length > 0 && t.MatchBackend != g.Backend) return -1;
                int s = 50;
                if (t.MatchBackend.Length > 0 && t.MatchBackend == g.Backend) s += 10;
                if (t.MatchLoader.Length > 0 && g.Loaders.Contains(t.MatchLoader)) s += 30;
                if (g.SuggestedLoader.Length > 0 && g.SuggestedLoader == t.Loader) s += 5;
                return s;
            }
            return 1;
        }

        void RebuildTemplates()
        {
            string keepId = selectedTemplate != null ? selectedTemplate.Def.Id : null;
            var chip = Engines.FirstOrDefault(c => c.IsSelected);
            string engine = chip == null || chip == Engines[0] ? null : chip.Label;
            var g = Game;

            var list = new List<TemplateVM>();
            foreach (var t in pack.Templates)
            {
                if (engine != null && t.Engine != engine) continue;
                int score = Score(t, g);
                if (score < 0) continue;
                string meta = string.Join("  ·  ", new[] { t.Engine, t.Loader, t.Language }.Where(s => !string.IsNullOrEmpty(s) && s != "Game specific").Distinct());
                list.Add(new TemplateVM { Def = t, Meta = meta, Score = score });
            }
            int best = list.Count > 0 ? list.Max(x => x.Score) : 0;
            foreach (var vm in list)
            {
                if (g == null || vm.Score < 50) continue;
                if (vm.Def.MatchLoader.Length > 0 && g.Loaders.Contains(vm.Def.MatchLoader)) vm.Badge = "loader installed";
                else if (vm.Score == best) vm.Badge = vm.Def.MatchFamily.Length > 0 ? "made for this game" : "best match";
            }
            list = list.OrderByDescending(x => x.Score).ToList(); // stable: pack order inside the same score

            building = true;
            Templates.Clear();
            foreach (var vm in list) Templates.Add(vm);
            building = false;
            NoTemplates = list.Count == 0;

            var next = Templates.FirstOrDefault(x => x.Def.Id == keepId);
            // A newly picked game moves to its best template; otherwise stay on what the user chose.
            if (next == null || (g != null && list.Count > 0 && list[0].Score >= 50 && next.Score < list[0].Score)) next = Templates.FirstOrDefault();
            SelectedTemplate = next; // also rebuilds the options, since the game may have changed
        }

        bool noTemplates;
        public bool NoTemplates { get { return noTemplates; } set { Set(ref noTemplates, value); } }

        TemplateVM selectedTemplate;
        public TemplateVM SelectedTemplate
        {
            get { return selectedTemplate; }
            set
            {
                if (building) return;
                selectedTemplate = value;
                Raise("SelectedTemplate");
                Raise("HasTemplate");
                Raise("TemplateTitle");
                Raise("TemplateDescription");
                Raise("HasDocs");
                BuildOptions();
            }
        }

        public bool HasTemplate { get { return selectedTemplate != null; } }
        public string TemplateTitle { get { return selectedTemplate != null ? selectedTemplate.Def.Name : "Pick a template"; } }
        public bool HasDocs { get { return selectedTemplate != null && selectedTemplate.Def.DocsUrl.Length > 0; } }
        public string TemplateDescription
        {
            get
            {
                if (selectedTemplate == null) return "Choose a game on the left, then a template.";
                var d = selectedTemplate.Def;
                return d.Description.Length > 0 ? d.Description : d.Summary;
            }
        }

        // ---------- options ----------

        void BuildOptions()
        {
            building = true;
            Options.Clear();
            if (selectedTemplate != null)
            {
                var t = selectedTemplate.Def;
                var gameVars = Game != null ? Game.ToVars() : GameInfo.EmptyVars();
                foreach (var o in t.Options)
                {
                    string v;
                    if (o.Type == "guid")
                    {
                        string key = t.Id + "/" + o.Id;
                        if (!guids.TryGetValue(key, out v)) guids[key] = v = Guid.NewGuid().ToString();
                    }
                    else
                    {
                        v = Generator.DefaultFor(o, gameVars);
                        bool fromGame = o.From.Length > 0;
                        string s;
                        if (!fromGame && sticky.TryGetValue(o.Type + ":" + o.Id, out s) && (o.Type != "choice" || o.Choices.Any(c => c.Value == s))) v = s;
                        else if (!fromGame && o.Remember && settings.Get("remember." + o.Id).Length > 0) v = settings.Get("remember." + o.Id);
                    }
                    Options.Add(new OptionVM(this, o, v));
                }
            }
            building = false;
            Refresh();
        }

        public void OptionEdited(OptionVM o)
        {
            if (building) return;
            if (o.Def.From.Length == 0) sticky[o.Def.Type + ":" + o.Def.Id] = o.Value;
            Refresh();
        }

        // ---------- preview ----------

        string problem = "";
        public string Problem { get { return problem; } set { if (Set(ref problem, value)) { Raise("HasProblem"); Raise("CanCreate"); } } }
        public bool HasProblem { get { return problem.Length > 0; } }
        public bool CanCreate { get { return selectedTemplate != null && problem.Length == 0; } }

        string filesSummary = "";
        public string FilesSummary { get { return filesSummary; } set { Set(ref filesSummary, value); } }

        string targetHint = "";
        public string TargetHint { get { return targetHint; } set { Set(ref targetHint, value); } }

        void Refresh()
        {
            string keepPath = selectedFile != null && !selectedFile.IsFolder ? selectedFile.Path : null;
            generated = new List<GeneratedFile>();
            folderName = "";
            if (selectedTemplate == null)
            {
                Files.Clear();
                SelectedFile = null;
                Problem = "";
                FilesSummary = "";
                TargetHint = "";
                return;
            }
            var t = selectedTemplate.Def;
            try
            {
                var values = Options.ToDictionary(o => o.Def.Id, o => o.Value);
                var vars = Generator.BuildVars(t, values, Game);
                string lastGroup = null;
                foreach (var o in Options)
                {
                    o.IsVisible = Generator.IsVisible(o.Def, vars);
                    bool header = o.IsVisible && o.Def.Group.Length > 0 && o.Def.Group != lastGroup;
                    o.GroupHeader = header ? o.Def.Group.ToUpperInvariant() : "";
                    if (o.IsVisible) lastGroup = o.Def.Group;
                }
                var errors = Generator.Validate(t, vars);
                foreach (var o in Options)
                {
                    string e;
                    o.Error = errors.TryGetValue(o.Def.Id, out e) ? e : "";
                }
                folderName = Generator.FolderName(t, vars);
                generated = Generator.Generate(t, vars);
                mainPath = Generator.MainFile(t, vars, generated);
                Problem = errors.Count > 0 ? "Fix the highlighted " + (errors.Count == 1 ? "field" : "fields") + " to continue." : "";
            }
            catch (Exception ex)
            {
                Problem = ex.Message;
            }

            BuildTree();
            long bytes = generated.Sum(f => (long)f.Data.Length);
            FilesSummary = generated.Count == 0 ? "" : generated.Count + " files  ·  " + (bytes < 1024 ? bytes + " B" : (bytes / 1024.0).ToString("0.0") + " KB");
            TargetHint = folderName.Length == 0 ? "" : Path.Combine(OutputDir, folderName);

            string main = mainPath;
            FileVM pick = Files.FirstOrDefault(f => !f.IsFolder && f.Path == keepPath)
                ?? Files.FirstOrDefault(f => !f.IsFolder && f.Path == main)
                ?? Files.FirstOrDefault(f => !f.IsFolder);
            SelectedFile = pick;
        }

        string mainPath = "";

        sealed class Node
        {
            public SortedDictionary<string, Node> Dirs = new SortedDictionary<string, Node>(StringComparer.OrdinalIgnoreCase);
            public List<GeneratedFile> Files = new List<GeneratedFile>();
        }

        void BuildTree()
        {
            var root = new Node();
            foreach (var f in generated)
            {
                var parts = f.Path.Split('/');
                var n = root;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    Node child;
                    if (!n.Dirs.TryGetValue(parts[i], out child)) n.Dirs[parts[i]] = child = new Node();
                    n = child;
                }
                n.Files.Add(f);
            }
            Files.Clear();
            if (generated.Count > 0)
            {
                Files.Add(new FileVM { Name = folderName, Path = "", IsFolder = true, Indent = new Thickness(0) });
                Walk(root, 1, "");
            }
        }

        void Walk(Node n, int depth, string prefix)
        {
            foreach (var d in n.Dirs)
            {
                Files.Add(new FileVM { Name = d.Key, Path = prefix + d.Key, IsFolder = true, Indent = new Thickness(depth * 14, 0, 0, 0) });
                Walk(d.Value, depth + 1, prefix + d.Key + "/");
            }
            foreach (var f in n.Files.OrderBy(x => x.Path, StringComparer.OrdinalIgnoreCase))
                Files.Add(new FileVM { Name = f.Path.Substring(f.Path.LastIndexOf('/') + 1), Path = f.Path, File = f, Indent = new Thickness(depth * 14, 0, 0, 0) });
        }

        FileVM selectedFile;
        public FileVM SelectedFile
        {
            get { return selectedFile; }
            set
            {
                if (value != null && value.IsFolder) { Raise("SelectedFile"); return; }
                selectedFile = value;
                Raise("SelectedFile");
                Raise("PreviewText");
                Raise("PreviewTitle");
            }
        }

        public string PreviewTitle { get { return selectedFile != null ? selectedFile.Path : ""; } }
        public string PreviewText
        {
            get
            {
                if (selectedFile == null || selectedFile.File == null) return "";
                var f = selectedFile.File;
                if (!f.IsText) return "(binary file, " + f.Data.Length + " bytes)";
                return f.Text.Length == 0 ? "(empty file: it only has to exist)" : f.Text;
            }
        }

        // ---------- create ----------

        string outputDir;
        public string OutputDir
        {
            get { return outputDir; }
            set { if (Set(ref outputDir, value ?? "")) TargetHint = folderName.Length == 0 ? "" : Path.Combine(outputDir, folderName); }
        }

        string status = "Ready";
        public string Status { get { return status; } set { Set(ref status, value); Raise("StatusIsError"); } }
        bool statusIsError;
        public bool StatusIsError { get { return statusIsError; } }

        void Fail(string message) { statusIsError = true; Status = message; }
        void Ok(string message) { statusIsError = false; Status = message; }

        bool showDone;
        public bool ShowDone { get { return showDone; } set { Set(ref showDone, value); } }
        string createdPath = "";
        public string CreatedPath { get { return createdPath; } }
        string doneTitle = "";
        public string DoneTitle { get { return doneTitle; } }

        void Create(bool zip)
        {
            if (!CanCreate || generated.Count == 0) { Fail(HasProblem ? Problem : "Pick a template first."); return; }
            try
            {
                string target;
                if (zip)
                {
                    target = PickZipPath(OutputDir, folderName + ".zip");
                    if (target == null) return;
                    Generator.WriteZip(generated, target, folderName);
                    createdPath = Path.GetDirectoryName(target);
                    doneTitle = "Zip created";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(OutputDir)) { Fail("Choose where to create the project."); return; }
                    target = Path.Combine(OutputDir, folderName);
                    Generator.WriteFolder(generated, target);
                    createdPath = target;
                    doneTitle = "Project created";
                }

                settings.OutputDir = OutputDir;
                foreach (var o in Options.Where(o => o.Def.Remember && o.Value.Length > 0)) settings.Set("remember." + o.Def.Id, o.Value);
                settings.Save();

                NextSteps.Clear();
                foreach (var s in selectedTemplate.Def.Next) NextSteps.Add(s);
                createdTarget = target;
                Raise("CreatedTarget");
                Raise("CreatedPath");
                Raise("DoneTitle");
                Ok("Created " + target);
                ShowDone = true;
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        string createdTarget = "";
        public string CreatedTarget { get { return createdTarget; } }

        static void Open(string what)
        {
            if (string.IsNullOrEmpty(what)) return;
            try { Process.Start(new ProcessStartInfo(what) { UseShellExecute = true }); }
            catch (Exception) { }
        }

        // ---------- template source ----------

        string packInfo;
        public string PackInfo { get { return packInfo; } set { Set(ref packInfo, value); } }

        bool showSource;
        public bool ShowSource { get { return showSource; } set { Set(ref showSource, value); } }

        string packUrl;
        public string PackUrl { get { return packUrl; } set { Set(ref packUrl, value ?? ""); } }

        bool updating;
        void UpdateTemplates()
        {
            if (updating) return;
            string url = PackUrl.Trim();
            settings.PackUrl = url;
            settings.Save();
            if (url.Length == 0) { Fail("Paste the address of a template pack zip first."); return; }
            updating = true;
            Ok("Downloading templates...");
            int current = pack.Version;
            Task.Run(() =>
            {
                string result; bool ok = true;
                try { result = PackUpdater.Update(url, current); }
                catch (Exception ex) { result = "Update failed: " + (ex.InnerException ?? ex).Message; ok = false; }
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    updating = false;
                    if (!ok) { Fail(result); return; }
                    ReloadPacks();
                    Ok("Templates " + result);
                }));
            });
        }

        void ReloadPacks()
        {
            string info;
            pack = PackStore.LoadAll(out info);
            PackInfo = info;
            BuildEngines();
            foreach (var g in allGames.Where(g => g.Info != null)) GameScanner.Apply(pack, g.Info);
            RebuildTemplates();
        }

        // ---------- test hooks ----------

        public void ShowSourceForTest() { ShowSource = true; }

        public void SelectForTest(string templateId, string gameName)
        {
            if (gameName != null)
            {
                var g = Games.FirstOrDefault(x => x.Info != null && string.Equals(x.Name, gameName, StringComparison.OrdinalIgnoreCase));
                if (g != null) SelectedGame = g;
            }
            if (templateId != null)
            {
                var t = Templates.FirstOrDefault(x => x.Def.Id == templateId);
                if (t != null) SelectedTemplate = t;
            }
        }
    }
}
