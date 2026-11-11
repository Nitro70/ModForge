package {{Package}}.mixin;

import {{Package}}.{{ClassName}};

import net.minecraft.server.MinecraftServer;

import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.injection.At;
import org.spongepowered.asm.mixin.injection.Inject;
import org.spongepowered.asm.mixin.injection.callback.CallbackInfo;

// Every mixin class must also be listed in {{ModId}}.mixins.json, or it is never applied.
@Mixin(MinecraftServer.class)
public class ExampleMixin {
	// Runs at the start of MinecraftServer.loadLevel(), so once per world load.
	@Inject(at = @At("HEAD"), method = "loadLevel")
	private void onLoadLevel(CallbackInfo info) {
		{{ClassName}}.LOGGER.info("{{ModName|str}} mixin: a world is loading");
	}
}
