package {{Package}};

import net.fabricmc.fabric.api.creativetab.v1.CreativeModeTabEvents;

import net.minecraft.core.Registry;
import net.minecraft.core.registries.BuiltInRegistries;
import net.minecraft.core.registries.Registries;
import net.minecraft.resources.ResourceKey;
import net.minecraft.world.item.CreativeModeTabs;
import net.minecraft.world.item.Item;

public final class ModItems {
	// The item's id: {{ModId}}:example_item. The files under assets/{{ModId}}/items and
	// assets/{{ModId}}/models/item, and the name in the lang file, all use this same name.
	public static final ResourceKey<Item> EXAMPLE_ITEM_KEY =
			ResourceKey.create(Registries.ITEM, {{ClassName}}.id("example_item"));

	// An item must be told its own id through setId before it is created.
	public static final Item EXAMPLE_ITEM = Registry.register(
			BuiltInRegistries.ITEM,
			EXAMPLE_ITEM_KEY,
			new Item(new Item.Properties().setId(EXAMPLE_ITEM_KEY)));

	private ModItems() {
	}

	// Called from onInitialize. Calling it loads this class, which registers the items above.
	public static void register() {
		// Shows the item in the Ingredients tab of the creative inventory.
		CreativeModeTabEvents.modifyOutputEvent(CreativeModeTabs.INGREDIENTS)
				.register(output -> output.accept(EXAMPLE_ITEM));
	}
}
