using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace {{Namespace}}
{
    /// <summary>
    /// The mod's settings. SMAPI saves this class as config.json in the mod's folder, so every public
    /// property here is a setting, and the values below are the defaults.
    /// </summary>
    public sealed class ModConfig
    {
        /// <summary>Turns the whole mod on or off.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>An example number setting.</summary>
        public int ExampleNumber { get; set; } = 10;

        /// <summary>The key that shows the hello message. Players can write "F9" or a combination like "LeftShift + F9".</summary>
        public KeybindList HelloKey { get; set; } = KeybindList.ForSingle(SButton.F9);
    }
}
