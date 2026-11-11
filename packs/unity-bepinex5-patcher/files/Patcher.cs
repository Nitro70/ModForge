using System.Collections.Generic;
{{#if Config}}
using System.IO;
using BepInEx;
using BepInEx.Configuration;
{{/if}}
using BepInEx.Logging;
using Mono.Cecil;

namespace {{Namespace}}
{
    // A preloader patcher. BepInEx finds it by shape, not by an attribute or a base class:
    //   - a public static property named TargetDLLs whose type is exactly IEnumerable<string>
    //   - a public static void method named Patch with one AssemblyDefinition parameter
    // If either is missing or declared differently (string[] instead of IEnumerable<string>, not public,
    // not static) the class is skipped without any message.
    public static class Patcher
    {
        public const string PatcherName = "{{ModName|str}}";
        public const string PatcherVersion = "{{Version}}";

        private static readonly ManualLogSource Log = Logger.CreateLogSource(PatcherName);
{{#if Config}}

        // Saved in BepInEx\config\{{ConfigName}}.cfg, created on first run.
        private static ConfigEntry<bool> enabled;
{{/if}}

        // File names of the assemblies to patch, with the extension. They are looked up in <Game>_Data\Managed.
        // A name that matches no file means Patch is never called, also without any message.
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "{{TargetDll|str}}" };

        // Optional. Called once before any patching.
        public static void Initialize()
        {
{{#if Config}}
            var config = new ConfigFile(Path.Combine(Paths.ConfigPath, "{{ConfigName}}.cfg"), true);
            enabled = config.Bind("General", "Enabled", true, "Turns the patcher on or off.");

{{/if}}
            Log.LogInfo($"{PatcherName} {PatcherVersion} loaded");
        }

        // Called once for each assembly in TargetDLLs, before the game loads it.
        // The assembly is changed in memory only: the file on disk stays as it is.
        public static void Patch(AssemblyDefinition assembly)
        {
{{#if Config}}
            if (!enabled.Value)
            {
                Log.LogInfo("Disabled in the config, leaving " + assembly.Name.Name + " alone");
                return;
            }

{{/if}}
            ModuleDefinition module = assembly.MainModule;
            Log.LogInfo("Patching " + assembly.Name.Name + " (" + module.Types.Count + " types)");

            // TODO: your changes here. An example that adds a public int field to a game class:
            //
            // TypeDefinition type = module.GetType("PlayerController");   // "Namespace.PlayerController" if it has one
            // if (type == null)
            // {
            //     Log.LogWarning("PlayerController not found, nothing patched");
            //     return;
            // }
            // type.Fields.Add(new FieldDefinition("myModData", FieldAttributes.Public, module.TypeSystem.Int32));
        }

        // Optional. Called once after every assembly has been patched and loaded.
        public static void Finish()
        {
        }
    }
}
