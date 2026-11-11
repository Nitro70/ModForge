package {{Package}};

import org.bukkit.plugin.java.JavaPlugin;

// The one class that extends JavaPlugin. plugin.yml names it under "main".
public final class {{ClassName}} extends JavaPlugin {
    @Override
    public void onEnable() {
        // Runs when the server enables the plugin, after the worlds have loaded.
        getLogger().info("{{ModName|str}} " + getPluginMeta().getVersion() + " enabled");
{{#if Listener}}

        // A listener does nothing until it is registered.
        getServer().getPluginManager().registerEvents(new JoinListener(), this);
{{/if}}
{{#if Command}}

        // Adds /{{CommandName}}. Commands registered this way need no entry in plugin.yml.
        registerCommand("{{CommandName}}", "Says hello.", new HelloCommand());
{{/if}}
    }

    @Override
    public void onDisable() {
        // Runs when the server stops or the plugin is disabled.
        getLogger().info("{{ModName|str}} disabled");
    }
}
