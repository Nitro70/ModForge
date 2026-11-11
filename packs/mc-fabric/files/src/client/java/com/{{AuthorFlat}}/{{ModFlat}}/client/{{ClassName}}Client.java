package {{Package}}.client;

import {{Package}}.{{ClassName}};

import net.fabricmc.api.ClientModInitializer;

public class {{ClassName}}Client implements ClientModInitializer {
	@Override
	public void onInitializeClient() {
		// Runs once while the game client starts, never on a dedicated server.
		// Rendering, screens and key binds are set up from here.
		{{ClassName}}.LOGGER.info("{{ModName|str}} client loaded");
	}
}
