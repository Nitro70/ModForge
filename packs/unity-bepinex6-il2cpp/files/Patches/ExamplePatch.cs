using HarmonyLib;

namespace {{Namespace}}.Patches
{
    // A Harmony patch runs your code before (Prefix) or after (Postfix) a game method.
    //
    // To make this one real:
    //   1. Find the class and method to change. Open Assembly-CSharp.dll from the game's BepInEx\interop folder
    //      in dnSpy or ILSpy. It lists every class and method, but the bodies are empty stubs: the real code
    //      is native, inside GameAssembly.dll.
    //   2. Uncomment the attribute below and put the real names in.
    //   3. Build. Plugin.Load already calls PatchAll, which picks up every [HarmonyPatch] class in this dll.
    //
    // If a patch does nothing in game:
    //   - Check BepInEx\LogOutput.log: a wrong method name is reported there.
    //   - Small methods (getters, setters, one-liners) are often inlined into their callers by the IL2CPP
    //     compiler. The patch applies without any error and then never runs. Patch a caller instead.
    //   - Generic methods and methods that take or return structs are the least reliable to patch.

    // [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    internal static class ExamplePatch
    {
        // Runs before the game method. Return false to skip the original method.
        // "__instance" is the object the method was called on. Name a parameter like the original to read it.
        private static bool Prefix(/* PlayerControl __instance */)
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
