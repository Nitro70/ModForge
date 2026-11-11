using HarmonyLib;

namespace {{Namespace}}.Patches
{
    // A Harmony patch runs your code before (Prefix) or after (Postfix) a game method.
    //
    // To make this one real:
    //   1. Find the class and method to change. Open the game's Assembly-CSharp.dll in dnSpy or ILSpy.
    //   2. Uncomment the attribute below and put the real names in.
    //   3. Build. Plugin.Awake already calls PatchAll, which picks up every [HarmonyPatch] class in this dll.
    //
    // If a patch does nothing in game, check BepInEx\LogOutput.log: a wrong method name is reported there.

    // [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.Jump))]
    internal static class ExamplePatch
    {
        // Runs before the game method. Return false to skip the original method.
        // "__instance" is the object the method was called on. Name a parameter like the original to read it.
        private static bool Prefix(/* PlayerController __instance */)
        {
{{#if Config}}
            if (!Plugin.Enabled.Value) return true;
{{/if}}
            return true;
        }

        // Runs after the game method. Add "ref float __result" to read or change what it returned.
        private static void Postfix()
        {
        }
    }
}
