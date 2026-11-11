// {{ModName}} {{Version}} by {{Author}}

#include <Geode/Geode.hpp>

// To modify a game class, include its modify header. Without the header for
// the class the $modify below does not compile.
#include <Geode/modify/MenuLayer.hpp>

// Brings cocos2d and the Geode namespaces into scope.
using namespace geode::prelude;

// Runs once when the mod is loaded, before any menu exists.
$on_mod(Loaded) {
	log::info("{{ModName}} {{Version}} loaded");
}

// $modify(OurName, GameClass) hooks the game class. Every function written
// here replaces the game's function of the same name and signature. The
// signature has to match exactly: with a different one nothing is hooked and
// nothing is reported.
class $modify({{ProjectName}}MenuLayer, MenuLayer) {
	// MenuLayer::init builds the main menu.
	bool init() {
		// Always call the original first, and give up if it failed.
		if (!MenuLayer::init()) {
			return false;
		}

		log::debug("MenuLayer::init hooked, the layer has {} children", this->getChildrenCount());

		// A button that uses one of the game's own sprites.
		auto button = CCMenuItemSpriteExtra::create(
			CCSprite::createWithSpriteFrameName("GJ_likeBtn_001.png"),
			this,
			menu_selector({{ProjectName}}MenuLayer::onButton)
		);

		// "bottom-menu" is a node ID. IDs are added to the game's menus by the
		// geode.node-ids mod, which is why mod.json lists it as a dependency.
		auto menu = this->getChildByID("bottom-menu");
		if (!menu) {
			log::warn("bottom-menu not found, the button was not added");
			return true;
		}
		menu->addChild(button);

		// _spr puts the mod ID in front, so the ID cannot clash with another mod.
		button->setID("hello-button"_spr);

		// Lets the menu lay out its buttons again now that it has one more.
		menu->updateLayout();

		return true;
	}

	// A button callback always returns void and takes a CCObject*.
	void onButton(CCObject*) {
		FLAlertLayer::create("{{ModName}}", "Hello from {{ModName}}!", "OK")->show();
	}
};
