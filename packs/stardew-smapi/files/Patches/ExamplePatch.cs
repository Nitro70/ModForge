using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace {{Namespace}}.Patches
{
    /// <summary>
    /// A Harmony patch: code that runs before or after one of the game's own methods. Use it only for what
    /// SMAPI's events and content API cannot do, because a patch breaks when the game changes that method.
    /// </summary>
    internal static class ExamplePatch
    {
        private static IMonitor Monitor;

        /// <summary>Called once from ModEntry.Entry.</summary>
        internal static void Apply(Harmony harmony, IMonitor monitor)
        {
            Monitor = monitor;

            // Game1.performTenMinuteClockUpdate runs every time the in-game clock moves ten minutes.
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.performTenMinuteClockUpdate)),
                postfix: new HarmonyMethod(typeof(ExamplePatch), nameof(After_PerformTenMinuteClockUpdate))
            );
        }

        /// <summary>Runs after the game's method. An error thrown from a patch can crash the game, so catch everything.</summary>
        private static void After_PerformTenMinuteClockUpdate()
        {
            try
            {
                Monitor.Log($"The clock moved to {Game1.timeOfDay}.", LogLevel.Trace);
                // TODO: your code here
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(After_PerformTenMinuteClockUpdate)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
