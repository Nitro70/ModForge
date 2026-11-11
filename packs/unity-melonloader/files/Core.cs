using MelonLoader;
{{#if Hotkey && Backend == il2cpp}}
#if HAS_UNITY
using UnityEngine;
#endif
{{#elif Hotkey}}
using UnityEngine;
{{/if}}

// MelonLoader reads these two attributes to find the mod. Without MelonInfo the dll is not treated as a mod at all.
[assembly: MelonInfo(typeof({{Namespace}}.Core), "{{ModName|str}}", "{{Version}}", "{{Author|str}}")]
{{#if GameDeveloper && GameProduct}}
// Loads only in this game. Both strings must match <Game>_Data\app.info exactly (line 1 and line 2).
[assembly: MelonGame("{{GameDeveloper|str}}", "{{GameProduct|str}}")]
{{#elif GameDeveloper}}
// Loads in every game by this developer. The string must match line 1 of <Game>_Data\app.info exactly.
[assembly: MelonGame("{{GameDeveloper|str}}")]
{{#else}}
// No arguments: the mod loads in any game. To limit it to one game, use
//   [assembly: MelonGame("Developer", "Game")]
// with line 1 and line 2 of <Game>_Data\app.info, copied exactly. A wrong name stops the mod from loading.
[assembly: MelonGame]
{{/if}}

namespace {{Namespace}}
{
    public class Core : MelonMod
    {
{{#if Config}}
        // Saved in UserData\MelonPreferences.cfg under [{{ProjectName}}], written when the game closes.
        private static MelonPreferences_Category prefs;
        internal static MelonPreferences_Entry<bool> Enabled;
        internal static MelonPreferences_Entry<float> Multiplier;
{{/if}}
{{#if Hotkey}}
{{#if Backend == il2cpp}}
#if HAS_UNITY
{{/if}}
{{#if Config}}
        internal static MelonPreferences_Entry<KeyCode> ToggleKey;
{{#else}}
        private const KeyCode ToggleKey = KeyCode.F6;
{{/if}}
        private bool active;
{{#if Backend == il2cpp}}
#endif
{{/if}}
{{/if}}
{{#if Config || Hotkey}}

{{/if}}
        // Called once, when MelonLoader has finished starting. Unity and game classes are safe to use from here on.
        public override void OnInitializeMelon()
        {
{{#if Config}}
            prefs = MelonPreferences.CreateCategory("{{ProjectName}}");
            Enabled = prefs.CreateEntry("Enabled", true, description: "Turns the whole mod on or off.");
            Multiplier = prefs.CreateEntry("Multiplier", 2f, description: "An example number setting.");
{{#if Hotkey}}
{{#if Backend == il2cpp}}
#if HAS_UNITY
{{/if}}
            ToggleKey = prefs.CreateEntry("ToggleKey", KeyCode.F6, description: "Press to toggle the mod's effect.");
{{#if Backend == il2cpp}}
#endif
{{/if}}
{{/if}}

{{/if}}
{{#if Hotkey && Backend == il2cpp}}
#if !HAS_UNITY
            LoggerInstance.Warning("Built without the game's generated assemblies: the hotkey was left out. See README.md.");
#endif
{{/if}}
            LoggerInstance.Msg($"{Info.Name} {Info.Version} loaded");
        }
{{#if Hotkey}}

{{#if Backend == il2cpp}}
#if HAS_UNITY
{{/if}}
        // Called every frame.
        public override void OnUpdate()
        {
{{#if Config}}
            if (!Enabled.Value) return;

            // Input is Unity's old input manager. If the game only uses the new Input System this throws; see README.md.
            if (Input.GetKeyDown(ToggleKey.Value))
{{#else}}
            // Input is Unity's old input manager. If the game only uses the new Input System this throws; see README.md.
            if (Input.GetKeyDown(ToggleKey))
{{/if}}
            {
                active = !active;
                LoggerInstance.Msg("Toggled " + (active ? "on" : "off"));
                // TODO: your code here
            }
        }
{{#if Backend == il2cpp}}
#endif
{{/if}}
{{/if}}
    }
}
