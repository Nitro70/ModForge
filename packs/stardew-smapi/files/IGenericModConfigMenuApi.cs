using System;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace {{Namespace}}
{
    /// <summary>
    /// The part of Generic Mod Config Menu's API this mod uses. SMAPI maps it onto the real API by method
    /// name and parameters, so the mod needs no reference to the other mod's dll. The interface must stay
    /// public, and each method must match the original exactly. To use more of the API, copy the method from
    /// https://github.com/spacechase0/StardewValleyMods/blob/develop/framework/GenericModConfigMenu/IGenericModConfigMenuApi.cs
    /// </summary>
    public interface IGenericModConfigMenuApi
    {
        /// <summary>Registers this mod's config page. Call it before adding options.</summary>
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        /// <summary>Adds a heading.</summary>
        void AddSectionTitle(IManifest mod, Func<string> text, Func<string> tooltip = null);

        /// <summary>Adds a checkbox.</summary>
        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);

        /// <summary>Adds a whole number, shown as a slider when min and max are set.</summary>
        void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string> formatValue = null, string fieldId = null);

        /// <summary>Adds a key binding.</summary>
        void AddKeybindList(IManifest mod, Func<KeybindList> getValue, Action<KeybindList> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);
    }
}
