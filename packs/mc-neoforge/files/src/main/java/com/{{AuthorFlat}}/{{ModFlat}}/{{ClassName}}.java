package {{Package}};

import org.slf4j.Logger;

import com.mojang.logging.LogUtils;

{{#if Command}}
import net.minecraft.commands.Commands;
import net.minecraft.network.chat.Component;
{{/if}}
{{#if Item}}
import net.minecraft.world.item.CreativeModeTabs;
import net.minecraft.world.item.Item;
{{/if}}
import net.neoforged.bus.api.IEventBus;
import net.neoforged.bus.api.SubscribeEvent;
import net.neoforged.fml.ModContainer;
import net.neoforged.fml.common.Mod;
import net.neoforged.fml.event.lifecycle.FMLCommonSetupEvent;
import net.neoforged.neoforge.common.NeoForge;
{{#if Item}}
import net.neoforged.neoforge.event.BuildCreativeModeTabContentsEvent;
{{/if}}
{{#if Command}}
import net.neoforged.neoforge.event.RegisterCommandsEvent;
{{/if}}
import net.neoforged.neoforge.event.server.ServerStartingEvent;
{{#if Item}}
import net.neoforged.neoforge.registries.DeferredItem;
import net.neoforged.neoforge.registries.DeferredRegister;
{{/if}}

// The value here must be the mod id from gradle.properties (it ends up in META-INF/neoforge.mods.toml).
@Mod({{ClassName}}.MODID)
public class {{ClassName}} {
    public static final String MODID = "{{ModId}}";

    // Writes to the console and to logs/latest.log.
    public static final Logger LOGGER = LogUtils.getLogger();
{{#if Item}}

    // Everything registered through ITEMS gets the id {{ModId}}:<name>.
    public static final DeferredRegister.Items ITEMS = DeferredRegister.createItems(MODID);

    // The item {{ModId}}:example_item. The files under assets/{{ModId}}/items and
    // assets/{{ModId}}/models/item, and the name in the lang file, all use this same name.
    public static final DeferredItem<Item> EXAMPLE_ITEM = ITEMS.registerSimpleItem("example_item");
{{/if}}

    // The constructor is the first code that runs when the mod is loaded.
    // NeoForge passes in the mod event bus and the mod's container.
    public {{ClassName}}(IEventBus modEventBus, ModContainer modContainer) {
        LOGGER.info("{{ModName|str}} {} loaded", modContainer.getModInfo().getVersion());

        modEventBus.addListener(this::commonSetup);
{{#if Item}}

        // Without this line nothing in ITEMS is ever registered.
        ITEMS.register(modEventBus);
        modEventBus.addListener(this::addCreative);
{{/if}}

        // Lets the @SubscribeEvent methods of this object receive game events.
        NeoForge.EVENT_BUS.register(this);
    }

    private void commonSetup(FMLCommonSetupEvent event) {
        // Runs after all mods have registered their content.
        LOGGER.info("{{ModName|str}} common setup");
    }
{{#if Item}}

    // Shows the item in the Ingredients tab of the creative inventory.
    private void addCreative(BuildCreativeModeTabContentsEvent event) {
        if (event.getTabKey() == CreativeModeTabs.INGREDIENTS) {
            event.accept(EXAMPLE_ITEM);
        }
    }
{{/if}}

    @SubscribeEvent
    public void onServerStarting(ServerStartingEvent event) {
        // Runs when a world is opened or a dedicated server starts.
        LOGGER.info("{{ModName|str}}: server starting");
    }
{{#if Command}}

    // Adds /{{ModId}}.
    @SubscribeEvent
    public void onRegisterCommands(RegisterCommandsEvent event) {
        event.getDispatcher().register(Commands.literal(MODID).executes(context -> {
            context.getSource().sendSuccess(
                    () -> Component.translatableWithFallback("{{ModId}}.greeting", "Hello from {{ModName|str}}!"),
                    false);
            return 1;
        }));
    }
{{/if}}
}
