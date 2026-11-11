using BepInEx;
{{#if Config || Component}}
using BepInEx.Configuration;
{{/if}}
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
{{#if Harmony}}
using HarmonyLib;
{{/if}}

namespace {{Namespace}}
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
{{#if ProcessFilter && GameExe}}
    [BepInProcess("{{GameExe|str}}.exe")]
{{/if}}
    public class Plugin : BasePlugin
    {
        // The GUID identifies the mod to BepInEx and names its config file. Never change it after release.
        public const string PluginGuid = "{{PluginGuid}}";
        public const string PluginName = "{{ModName|str}}";
        public const string PluginVersion = "{{Version}}";

        internal static Plugin Instance;
        internal static new ManualLogSource Log;
{{#if Harmony}}
        private Harmony harmony;
{{/if}}
{{#if Config}}

        // Saved in BepInEx\config\{{PluginGuid}}.cfg, created on first run.
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<float> Multiplier;
{{/if}}
{{#if Component}}
#if HAS_INTEROP
        internal static ConfigEntry<UnityEngine.KeyCode> ToggleKey;
#endif
{{/if}}

        // An IL2CPP plugin is a plain class, not a MonoBehaviour: BepInEx calls Load once when the game starts.
        public override void Load()
        {
            Instance = this;
            Log = base.Log;
{{#if Config}}

            Enabled = Config.Bind("General", "Enabled", true, "Turns the whole mod on or off.");
            Multiplier = Config.Bind("General", "Multiplier", 2f, "An example number setting.");
{{/if}}
{{#if Harmony}}

            // Applies every [HarmonyPatch] class in this dll (see the Patches folder).
            harmony = new Harmony(PluginGuid);
            harmony.PatchAll(typeof(Plugin).Assembly);
{{/if}}
{{#if Component}}

#if HAS_INTEROP
            ToggleKey = Config.Bind("Keys", "Toggle", UnityEngine.KeyCode.F6, "Press to toggle the mod's effect.");

            // Registers ModBehaviour with IL2CPP and adds it to BepInEx's own GameObject, which gives it Update.
            AddComponent<ModBehaviour>();
#else
            Log.LogWarning("Built without the interop assemblies: ModBehaviour (Update and the hotkey) was left out. See README.md.");
#endif
{{/if}}

            Log.LogInfo($"{PluginName} {PluginVersion} loaded");
        }
{{#if Harmony}}

        public override bool Unload()
        {
            harmony?.UnpatchSelf();
            return true;
        }
{{/if}}
    }
}
