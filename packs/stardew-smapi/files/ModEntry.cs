{{#if Harmony}}
using HarmonyLib;
{{/if}}
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace {{Namespace}}
{
    /// <summary>The mod entry point. SMAPI creates this class once and calls Entry.</summary>
    internal sealed class ModEntry : Mod
    {
{{#if Config}}
        /// <summary>The settings from config.json in the mod's folder.</summary>
        private ModConfig Config;

{{/if}}
        public override void Entry(IModHelper helper)
        {
{{#if Config}}
            // Reads config.json, or creates it with the defaults from ModConfig on the first run.
            this.Config = helper.ReadConfig<ModConfig>();

{{/if}}
{{#if Config && Gmcm}}
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
{{/if}}
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
{{#if Config}}
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
{{#else}}
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
{{/if}}
{{#if Harmony}}

            // Applies the patches in the Patches folder.
            var harmony = new Harmony(this.ModManifest.UniqueID);
            Patches.ExamplePatch.Apply(harmony, this.Monitor);
{{/if}}

            this.Monitor.Log($"{this.ModManifest.Name} {this.ModManifest.Version} loaded", LogLevel.Info);
        }
{{#if Config && Gmcm}}

        /// <summary>Runs once, after every mod is loaded. The place to talk to other mods.</summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Null when the player does not have Generic Mod Config Menu. The mod works without it.
            var menu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (menu is null)
                return;

            menu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );
            menu.AddBoolOption(
                mod: this.ModManifest,
                getValue: () => this.Config.Enabled,
                setValue: value => this.Config.Enabled = value,
{{#if I18n}}
                name: () => this.Helper.Translation.Get("config.enabled.name"),
                tooltip: () => this.Helper.Translation.Get("config.enabled.tooltip")
{{#else}}
                name: () => "Enabled",
                tooltip: () => "Turns the whole mod on or off."
{{/if}}
            );
            menu.AddNumberOption(
                mod: this.ModManifest,
                getValue: () => this.Config.ExampleNumber,
                setValue: value => this.Config.ExampleNumber = value,
{{#if I18n}}
                name: () => this.Helper.Translation.Get("config.example-number.name"),
                tooltip: () => this.Helper.Translation.Get("config.example-number.tooltip"),
{{#else}}
                name: () => "Example number",
                tooltip: () => "An example number setting.",
{{/if}}
                min: 0,
                max: 100
            );
            menu.AddKeybindList(
                mod: this.ModManifest,
                getValue: () => this.Config.HelloKey,
                setValue: value => this.Config.HelloKey = value,
{{#if I18n}}
                name: () => this.Helper.Translation.Get("config.hello-key.name"),
                tooltip: () => this.Helper.Translation.Get("config.hello-key.tooltip")
{{#else}}
                name: () => "Hello key",
                tooltip: () => "Press to show a message."
{{/if}}
            );
        }
{{/if}}

        /// <summary>Runs after a save is loaded and the world is ready.</summary>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.Monitor.Log($"Save loaded for {Game1.player.Name}.", LogLevel.Debug);
        }
{{#if Config}}

        /// <summary>Runs when the player presses or releases any key or button.</summary>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            // Ignore input on the title screen and while a menu is open.
            if (!Context.IsPlayerFree || !this.Config.Enabled)
                return;

            if (this.Config.HelloKey.JustPressed())
                this.SayHello();
        }
{{#else}}

        /// <summary>Runs when the player presses a key or button.</summary>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // Ignore input on the title screen and while a menu is open.
            if (!Context.IsPlayerFree)
                return;

            if (e.Button == SButton.F9)
                this.SayHello();
        }
{{/if}}

        private void SayHello()
        {
{{#if I18n}}
            string text = this.Helper.Translation.Get("hud.hello");
{{#else}}
            string text = "Hello from {{ModName|str}}!";
{{/if}}
            Game1.addHUDMessage(new HUDMessage(text));
            this.Monitor.Log(text, LogLevel.Debug);
            // TODO: your code here
        }
    }
}
