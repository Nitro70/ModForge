using BepInEx;
{{#if Config || Hotkey || Gui}}
using BepInEx.Configuration;
{{/if}}
using BepInEx.Logging;
using BepInEx.Unity.Mono;
{{#if Harmony}}
using HarmonyLib;
{{/if}}
using UnityEngine;

namespace {{Namespace}}
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
{{#if ProcessFilter && GameExe}}
    [BepInProcess("{{GameExe|str}}.exe")]
{{/if}}
    public class Plugin : BaseUnityPlugin
    {
        // The GUID identifies the mod to BepInEx and names its config file. Never change it after release.
        public const string PluginGuid = "{{PluginGuid}}";
        public const string PluginName = "{{ModName|str}}";
        public const string PluginVersion = "{{Version}}";

        internal static Plugin Instance;
        internal static ManualLogSource Log;
{{#if Harmony}}
        private Harmony harmony;
{{/if}}
{{#if Config}}

        // Saved in BepInEx\config\{{PluginGuid}}.cfg, created on first run.
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<float> Multiplier;
{{/if}}
{{#if Hotkey}}
        internal static ConfigEntry<KeyCode> ToggleKey;
{{/if}}
{{#if Gui}}
        internal static ConfigEntry<KeyCode> MenuKey;
        private bool showWindow;
        private Rect windowRect = new Rect(40, 40, 320, 160);
{{/if}}
{{#if Hotkey}}
        private bool active;
{{/if}}

        private void Awake()
        {
            Instance = this;
            Log = Logger;
{{#if Config}}

            Enabled = Config.Bind("General", "Enabled", true, "Turns the whole mod on or off.");
            Multiplier = Config.Bind("General", "Multiplier", 2f, "An example number setting.");
{{/if}}
{{#if Hotkey}}
            ToggleKey = Config.Bind("Keys", "Toggle", KeyCode.F6, "Press to toggle the mod's effect.");
{{/if}}
{{#if Gui}}
            MenuKey = Config.Bind("Keys", "Menu", KeyCode.F8, "Press to show or hide the window.");
{{/if}}
{{#if Harmony}}

            // Applies every [HarmonyPatch] class in this dll (see the Patches folder).
            harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
{{/if}}

            Log.LogInfo($"{PluginName} {PluginVersion} loaded");
        }
{{#if Hotkey || Gui}}

        private void Update()
        {
{{#if Hotkey}}
            // UnityInput works with both the old and the new Unity input system.
            if (UnityInput.Current.GetKeyDown(ToggleKey.Value))
            {
                active = !active;
                Log.LogInfo("Toggled " + (active ? "on" : "off"));
                // TODO: your code here
            }
{{/if}}
{{#if Gui}}
            if (UnityInput.Current.GetKeyDown(MenuKey.Value))
                showWindow = !showWindow;
{{/if}}
        }
{{/if}}
{{#if Gui}}

        private void OnGUI()
        {
            if (!showWindow) return;
            windowRect = GUILayout.Window(GetInstanceID(), windowRect, DrawWindow, PluginName);
        }

        private void DrawWindow(int id)
        {
            GUILayout.Label("Version " + PluginVersion);
{{#if Config}}
            Enabled.Value = GUILayout.Toggle(Enabled.Value, "Enabled");
            GUILayout.Label("Multiplier: " + Multiplier.Value.ToString("0.0"));
            Multiplier.Value = GUILayout.HorizontalSlider(Multiplier.Value, 0f, 10f);
{{/if}}
            if (GUILayout.Button("Do something"))
            {
                Log.LogInfo("Button pressed");
                // TODO: your code here
            }
            GUI.DragWindow();
        }
{{/if}}
{{#if Harmony}}

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
        }
{{/if}}
    }
}
