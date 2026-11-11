using System;
using System.Collections.Generic;

namespace ModForge.Core
{
    public sealed class Choice
    {
        public string Value;
        public string Label;
    }

    /// <summary>One thing the user can set on a template. See docs/PACK-FORMAT.md.</summary>
    public sealed class TemplateOption
    {
        public string Id;
        public string Label;
        /// <summary>text | bool | choice | folder | guid</summary>
        public string Type = "text";
        public string Default = "";
        public string Help = "";
        public string Group = "";
        public List<Choice> Choices = new List<Choice>();
        public bool Required;
        public string Pattern = "";
        public string PatternHelp = "";
        /// <summary>Name of a game variable that supplies the default when a game is picked.</summary>
        public string From = "";
        /// <summary>Expression; the option is hidden (and keeps its default) while it is false.</summary>
        public string ShowWhen = "";
        /// <summary>Keep the last value the user typed across runs (author names and the like).</summary>
        public bool Remember;
    }

    public sealed class TemplateDef
    {
        public string Id;
        public string Name;
        public string Engine = "Other";
        public string Loader = "";
        public string Language = "";
        public string Summary = "";
        public string Description = "";
        public string DocsUrl = "";
        public int Order = 100;
        /// <summary>lf or crlf. Shell scripts are always lf.</summary>
        public string Eol = "crlf";

        public string MatchEngine = "";
        public string MatchBackend = "";
        public string MatchFamily = "";
        public string MatchLoader = "";

        public List<TemplateOption> Options = new List<TemplateOption>();
        public List<KeyValuePair<string, string>> Derived = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> When = new List<KeyValuePair<string, string>>();
        public List<string> Next = new List<string>();
        /// <summary>Name of the folder the project is created in, as a template string.</summary>
        public string Folder = "{{ModName}}";
        /// <summary>Path of the file the preview opens on, as a template string. Optional.</summary>
        public string Main = "";

        /// <summary>Source files, keyed by their path under files/ with forward slashes.</summary>
        public SortedDictionary<string, byte[]> Files = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);
        public string Origin = "";
    }

    public sealed class KnownGame
    {
        public string Name = "";
        public string AppId = "";
        public string Engine = "";
        public string Backend = "";
        public string Family = "";
        public string Loader = "";
        public string Notes = "";
        public List<string> Folders = new List<string>();
    }

    public sealed class Pack
    {
        public string Name = "";
        public int Version;
        public List<TemplateDef> Templates = new List<TemplateDef>();
        public List<KnownGame> Games = new List<KnownGame>();
        public List<string> EngineOrder = new List<string>();
        /// <summary>Templates that could not be read. One bad template never hides the rest.</summary>
        public List<string> Errors = new List<string>();
    }

    public sealed class GameInfo
    {
        public string Name = "";
        public string Path = "";
        public string AppId = "";
        /// <summary>unity | unreal | godot | "" when unknown or a custom engine</summary>
        public string Engine = "";
        /// <summary>mono | il2cpp | ""</summary>
        public string Backend = "";
        public string Family = "";
        public string ExeName = "";
        public string DataDir = "";
        public string EngineVersion = "";
        public string Arch = "";
        public string UnrealProject = "";
        public string UnrealBinDir = "";
        public List<string> Loaders = new List<string>();
        public bool Installed;
        public string Source = "";
        public string Notes = "";
        public string SuggestedLoader = "";

        /// <summary>The variables a template can read about the picked game. All are always defined.</summary>
        public Dictionary<string, string> ToVars()
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "HasGame", Installed ? "true" : "" },
                { "GameName", Name ?? "" },
                { "GamePath", Installed ? (Path ?? "") : "" },
                { "GameExe", ExeName ?? "" },
                { "GameDataDir", DataDir ?? "" },
                { "UnityVersion", Engine == "unity" ? (EngineVersion ?? "") : "" },
                { "UnityBackend", Backend ?? "" },
                { "UnityTfm", Engine == "unity" ? UnityTfm(EngineVersion) : "" },
                { "EngineVersion", EngineVersion ?? "" },
                { "GameArch", Arch ?? "" },
                { "SteamAppId", AppId ?? "" },
                { "UnrealProject", UnrealProject ?? "" },
                { "UnrealBinDir", UnrealBinDir ?? "" },
            };
        }

        /// <summary>The .NET target a Mono plugin should build for, from the Unity version. Empty when unknown.</summary>
        static string UnityTfm(string version)
        {
            var parts = (version ?? "").Split('.');
            int major, minor = 0;
            if (parts.Length < 1 || !int.TryParse(parts[0], out major)) return "";
            if (parts.Length > 1) int.TryParse(parts[1], out minor);
            if (major > 2021 || (major == 2021 && minor >= 2) || (major >= 6 && major < 2000)) return "netstandard2.1";
            if (major >= 2018) return "net46";
            return "net35";
        }

        public static Dictionary<string, string> EmptyVars()
        {
            return new GameInfo().ToVars();
        }
    }

    public sealed class GeneratedFile
    {
        public string Path;
        public byte[] Data;
        public bool IsText;
        public string Text;
    }

    public sealed class ForgeException : Exception
    {
        public ForgeException(string message) : base(message) { }
    }
}
