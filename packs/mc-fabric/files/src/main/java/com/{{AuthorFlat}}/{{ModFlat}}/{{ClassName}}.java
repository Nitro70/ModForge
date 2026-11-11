package {{Package}};

import net.fabricmc.api.ModInitializer;
{{#if Command}}
import net.fabricmc.fabric.api.command.v2.CommandRegistrationCallback;
{{/if}}
import net.fabricmc.loader.api.FabricLoader;

{{#if Command}}
import net.minecraft.commands.Commands;
import net.minecraft.network.chat.Component;
{{/if}}
import net.minecraft.resources.Identifier;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class {{ClassName}} implements ModInitializer {
	// Must be the same as "id" in fabric.mod.json.
	public static final String MOD_ID = "{{ModId}}";

	// Writes to the console and to logs/latest.log under the mod id.
	public static final Logger LOGGER = LoggerFactory.getLogger(MOD_ID);

	@Override
	public void onInitialize() {
		// Runs once while the game starts, on the client and on a dedicated server.
		LOGGER.info("{{ModName|str}} {} loaded", version());
{{#if Item}}

		ModItems.register();
{{/if}}
{{#if Command}}

		// Adds /{{ModId}}. Commands registered here exist in singleplayer and on servers.
		CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) ->
				dispatcher.register(Commands.literal(MOD_ID).executes(context -> {
					context.getSource().sendSuccess(
							() -> Component.translatableWithFallback("{{ModId}}.greeting", "Hello from {{ModName|str}}!"),
							false);
					return 1;
				})));
{{/if}}
	}

	// An id in this mod's namespace, like {{ModId}}:example_item.
	public static Identifier id(String path) {
		return Identifier.fromNamespaceAndPath(MOD_ID, path);
	}

	// The version from gradle.properties, read back from fabric.mod.json.
	private static String version() {
		return FabricLoader.getInstance()
				.getModContainer(MOD_ID)
				.map(mod -> mod.getMetadata().getVersion().getFriendlyString())
				.orElse("?");
	}
}
