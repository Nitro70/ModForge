using HarmonyLib;

namespace {{Namespace}}.Patches
{
    // A Harmony patch runs your code before (Prefix) or after (Postfix) a game method.
    //
    // To make this one real:
{{#if Backend == il2cpp}}
    //   1. Find the class and method to change. Open Assembly-CSharp.dll from the game's
    //      MelonLoader\Il2CppAssemblies folder in dnSpy or ILSpy. It lists every class and method, but the
    //      bodies are empty stubs: the real code is native, inside GameAssembly.dll.
    //      Game classes get an "Il2Cpp" prefix on their namespace there (a class with no namespace is in "Il2Cpp").
{{#else}}
    //   1. Find the class and method to change. Open the game's Assembly-CSharp.dll in dnSpy or ILSpy.
{{/if}}
    //   2. Uncomment the attribute below and put the real names in.
    //   3. Build. There is no PatchAll call to add: MelonLoader applies every [HarmonyPatch] class in this dll
    //      by itself when it loads the mod.
    //
    // If a patch does nothing in game:
    //   - Check MelonLoader\Latest.log: a wrong method name is reported there.
{{#if Backend == il2cpp}}
    //   - Small methods (getters, setters, one-liners) are often inlined into their callers by the IL2CPP
    //     compiler. The patch applies without any error and then never runs. Patch a caller instead.
    //   - Transpilers do not work on IL2CPP games: there is no IL to rewrite.
{{/if}}

    // [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.Jump))]
    internal static class ExamplePatch
    {
        // Runs before the game method. Return false to skip the original method.
        // "__instance" is the object the method was called on. Name a parameter like the original to read it.
        private static bool Prefix(/* PlayerController __instance */)
        {
{{#if Config}}
            if (!Core.Enabled.Value) return true;
{{/if}}
            return true;
        }

        // Runs after the game method. Add "ref float __result" to read or change what it returned.
        private static void Postfix()
        {
        }
    }
}
