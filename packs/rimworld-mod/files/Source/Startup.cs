{{#if Harmony}}
using HarmonyLib;
{{/if}}
using Verse;

namespace {{Namespace}}
{
    /// <summary>
    /// The game runs the static constructor of every class marked [StaticConstructorOnStartup] once,
    /// on the main thread, after all Defs are loaded. That makes it the place for start-up code.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Startup
    {
        public const string ModName = "{{ModName|str}}";
        public const string ModVersion = "{{Version}}";
{{#if Harmony}}

        // The Harmony id only has to be unique. The packageId from About.xml is.
        public const string HarmonyId = "{{PackageId}}";
{{/if}}

        static Startup()
        {
{{#if Harmony}}
            // Applies every [HarmonyPatch] class in this dll (see the Patches folder).
            new Harmony(HarmonyId).PatchAll();

{{/if}}
            // Shows in the debug log (turn on Development mode in Options, then press the ` key).
            Log.Message($"[{ModName}] {ModVersion} loaded");
        }
    }
}
