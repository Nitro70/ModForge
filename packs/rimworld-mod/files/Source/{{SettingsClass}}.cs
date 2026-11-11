using Verse;

namespace {{Namespace}}
{
    /// <summary>
    /// The mod's settings. The game saves them to its Config folder when the settings window closes.
    /// Every field needs a line in ExposeData, or it is not saved.
    /// </summary>
    public class {{SettingsClass}} : ModSettings
    {
        public bool enabled = true;
        public float multiplier = 2f;

        public override void ExposeData()
        {
            // The third argument is the default: it must equal the field's initial value above.
            Scribe_Values.Look(ref enabled, "enabled", true);
            Scribe_Values.Look(ref multiplier, "multiplier", 2f);
            base.ExposeData();
        }
    }
}
