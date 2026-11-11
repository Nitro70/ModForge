package {{Package}};

import net.minecraft.client.Minecraft;
import net.neoforged.api.distmarker.Dist;
import net.neoforged.bus.api.SubscribeEvent;
import net.neoforged.fml.ModContainer;
import net.neoforged.fml.common.EventBusSubscriber;
import net.neoforged.fml.common.Mod;
import net.neoforged.fml.event.lifecycle.FMLClientSetupEvent;

// This class does not load on dedicated servers, so client-only code is safe here.
@Mod(value = {{ClassName}}.MODID, dist = Dist.CLIENT)
// Registers every static @SubscribeEvent method in this class, on the client only.
@EventBusSubscriber(modid = {{ClassName}}.MODID, value = Dist.CLIENT)
public class {{ClassName}}Client {
    public {{ClassName}}Client(ModContainer container) {
        // Client-only setup that needs the mod container goes here.
    }

    @SubscribeEvent
    static void onClientSetup(FMLClientSetupEvent event) {
        {{ClassName}}.LOGGER.info("{{ModName|str}} client setup, player name {}", Minecraft.getInstance().getUser().getName());
    }
}
