# {{ModName}}

A NeoForge mod for Minecraft: Java Edition {{MinecraftVersion}}. Mod id: `{{ModId}}`.

## What you need

- **JDK 25.** If it is not installed, Gradle downloads one by itself (that is what the foojay line in
  `settings.gradle` is for). Gradle itself runs on any JDK from 17 up.
- An internet connection for the first import: Gradle downloads Minecraft and NeoForge, which takes a few minutes.

## Layout

| File | What it is for |
|---|---|
| `build.gradle` | The build: the ModDevGradle plugin, the NeoForge version, the run configurations. |
| `gradle.properties` | Every value you normally change: Minecraft and NeoForge versions, mod id, name, version, author. |
| `settings.gradle` | Where plugins come from, and the project name. |
| `gradle/wrapper/gradle-wrapper.properties` | Which Gradle version builds the project ({{GradleVersion}}). |
| `src/main/templates/META-INF/neoforge.mods.toml` | The manifest NeoForge reads. The build fills in its `${...}` values from `gradle.properties` and puts it at `META-INF/neoforge.mods.toml` in the jar. Without it the jar is not a mod. |
| `src/main/java/com/{{AuthorFlat}}/{{ModFlat}}/{{ClassName}}.java` | The main class, marked with `@Mod("{{ModId}}")`. Its constructor runs when the mod loads. |
{{#if Client}}
| `src/main/java/com/{{AuthorFlat}}/{{ModFlat}}/{{ClassName}}Client.java` | A second `@Mod` class that only loads on the client. |
{{/if}}
{{#if Item}}
| `src/main/resources/assets/{{ModId}}/items/example_item.json` | Says which model the example item uses. Required for every item. |
| `src/main/resources/assets/{{ModId}}/models/item/example_item.json` | The model. It borrows the diamond texture so it works without a picture. |
{{/if}}
| `src/main/resources/assets/{{ModId}}/lang/en_us.json` | English names and texts. |

There is no `gradlew`, `gradlew.bat` or `gradle-wrapper.jar` in this folder. IntelliJ IDEA does not need them: it
reads `gradle-wrapper.properties` and downloads that Gradle by itself. To build from a terminal, install Gradle
once and run this in the project folder to create the wrapper scripts:

```
gradle wrapper
```

## Build and run

```
gradlew runClient     starts a development game with the mod loaded
gradlew runServer     starts a development server
gradlew build         builds the jar
```

The mod is `build/libs/{{ModId}}-{{Version}}.jar`.

## Install

1. Install NeoForge for Minecraft {{MinecraftVersion}} with the installer from neoforged.net.
2. Put `{{ModId}}-{{Version}}.jar` in `{{#if GamePath}}{{GamePath}}{{#else}}%APPDATA%\.minecraft{{/if}}\mods`.
3. Start the NeoForge profile in the launcher.

`logs/latest.log` then has the line `{{ModName}} {{Version}} loaded`, and the mod is in the Mods list.
{{#if Command}}
In a world, `/{{ModId}}` answers with a greeting.
{{/if}}
{{#if Item}}
The item is in the Ingredients tab of the creative inventory, or `/give @s {{ModId}}:example_item`.
{{/if}}

## When it does not load

- **The mod is not in the Mods list:** the jar is not in the `mods` folder of the game folder the NeoForge
  profile uses, or the jar has no `META-INF/neoforge.mods.toml` (the file was moved out of `src/main/templates`).
- **The mod id is in three places and they must agree:** `mod_id` in `gradle.properties`, `MODID` in
  `{{ClassName}}.java`, and the folder name `assets/{{ModId}}`. If `@Mod` names an id that the toml does not
  declare, the game stops at start with an error about a missing mod. The id may only use lower case letters,
  digits and `_`.
- **"requires neoforge ... or above" or a Minecraft version error:** the jar was built for another version.
  `neo_version` in `gradle.properties` must start with the Minecraft version ({{MinecraftVersion}}).
- **A file named `mods.toml` is ignored:** since Minecraft 1.20.5 the file is `neoforge.mods.toml`.
{{#if Item}}
- **Registered things do not exist in game:** the `DeferredRegister` was never attached. The line
  `ITEMS.register(modEventBus)` in the constructor does that.
- **The item is a purple and black cube:** `assets/{{ModId}}/items/example_item.json` is missing or points at a
  model that does not exist. File names must equal the item's id.
{{/if}}
- **An event method is never called:** game events need `NeoForge.EVENT_BUS.register(this)` and `@SubscribeEvent`;
  loading events (setup, registration, creative tabs) go through the mod event bus that the constructor receives.
- **The IDE cannot find Minecraft classes:** the Gradle import did not finish. Reload the Gradle project and
  wait for it.
