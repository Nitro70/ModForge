using HarmonyLib;
using Verse;

namespace {{Namespace}}.Patches
{
    /// <summary>
    /// A Harmony patch: code that runs before (Prefix) or after (Postfix) one of the game's own methods.
    /// This one runs after Game.FinalizeInit, which the game calls when a colony has finished loading.
    /// </summary>
    [HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
    public static class ExamplePatch
    {
        public static void Postfix()
        {
{{#if Settings}}
            if (!{{ModClass}}.Settings.enabled)
                return;

{{/if}}
            Log.Message($"[{Startup.ModName}] a game was loaded");
            // TODO: your code here
        }
    }
}
