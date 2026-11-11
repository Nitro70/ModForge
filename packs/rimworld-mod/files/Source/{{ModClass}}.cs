using UnityEngine;
using Verse;

namespace {{Namespace}}
{
    /// <summary>
    /// The game creates one instance of every class that inherits Mod, early, before Defs are loaded.
    /// This one owns the settings and draws their window (Options, Mod settings, {{ModName}}).
    /// </summary>
    public class {{ModClass}} : Mod
    {
        public static {{SettingsClass}} Settings;

        public {{ModClass}}(ModContentPack content) : base(content)
        {
            // Loads the saved settings, or creates them with their defaults.
            Settings = GetSettings<{{SettingsClass}}>();
        }

        /// <summary>The name in the mod settings list. Returning an empty string hides the mod from that list.</summary>
        public override string SettingsCategory()
        {
            return "{{ModName|str}}";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled("{{ProjectName}}_Enabled".Translate(), ref Settings.enabled, "{{ProjectName}}_EnabledTip".Translate());
            listing.Label("{{ProjectName}}_Multiplier".Translate(Settings.multiplier.ToString("0.0")));
            Settings.multiplier = listing.Slider(Settings.multiplier, 0f, 10f);
            listing.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}
