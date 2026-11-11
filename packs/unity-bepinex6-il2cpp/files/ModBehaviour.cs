// This file needs Unity's classes, which come from the interop assemblies. The csproj defines HAS_INTEROP
// when it finds them; until then the whole file is left out so the project still builds.
#if HAS_INTEROP
using System;
using UnityEngine;

namespace {{Namespace}}
{
    // A MonoBehaviour written in a mod does not exist inside the IL2CPP game until it is registered.
    // Plugin.Load does that with AddComponent<ModBehaviour>(), which registers the class and attaches it.
    // To attach one to another GameObject yourself, register it first:
    //   Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<ModBehaviour>();
    public class ModBehaviour : MonoBehaviour
    {
        // Required. IL2CPP creates the object through this constructor; without it the component fails to attach.
        public ModBehaviour(IntPtr ptr) : base(ptr)
        {
        }

        private bool active;

        // Unity finds Awake, Start, Update, OnGUI and the rest by name, the same as in a normal Unity script.
        private void Update()
        {
{{#if Config}}
            if (!Plugin.Enabled.Value) return;

{{/if}}
            // Input is Unity's old input manager. If the game only uses the new Input System this throws; see README.md.
            if (Input.GetKeyDown(Plugin.ToggleKey.Value))
            {
                active = !active;
                Plugin.Log.LogInfo("Toggled " + (active ? "on" : "off"));
                // TODO: your code here
            }
        }
    }
}
#endif
