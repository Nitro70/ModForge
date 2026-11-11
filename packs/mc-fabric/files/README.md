# {{ModName}}

A Fabric mod for Minecraft: Java Edition {{MinecraftVersion}}. Mod id: `{{ModId}}`.

## What you need

- **JDK 25.** Minecraft 26.x runs on Java 25 and its classes cannot be read by an older compiler. Gradle itself
  must also run on JDK 25 here (`JAVA_HOME`, or the Gradle JVM setting in IntelliJ).
- An internet connection for the first import: Gradle downloads Minecraft, Fabric Loader and Fabric API.

## Layout

| File | What it is for |
|---|---|
| `build.gradle` | The build: the Fabric Loom plugin, Minecraft, Fabric Loader and Fabric API. |
| `gradle.properties` | Every version number (Minecraft, loader, Loom, Fabric API) and the mod's own version and group. |
| `settings.gradle` | Tells Gradle where the Loom plugin comes from, and names the jar (`{{ModId}}`). |
| `gradle/wrapper/gradle-wrapper.properties` | Which Gradle version builds the project ({{GradleVersion}}). |
| `src/main/resources/fabric.mod.json` | The manifest Fabric Loader reads: mod id, entrypoints, mixin configs, dependencies. Without it the jar is not a mod. |
| `src/main/java/com/{{AuthorFlat}}/{{ModFlat}}/{{ClassName}}.java` | The main entrypoint. `onInitialize` runs once at game start. |
{{#if Item}}
| `src/main/java/com/{{AuthorFlat}}/{{ModFlat}}/ModItems.java` | Registers `{{ModId}}:example_item`. |
| `src/main/resources/assets/{{ModId}}/items/example_item.json` | Says which model the item uses. Required for every item. |
| `src/main/resources/assets/{{ModId}}/models/item/example_item.json` | The model. It borrows the diamond texture so it works without a picture. |
{{/if}}
| `src/main/resources/assets/{{ModId}}/lang/en_us.json` | English names and texts. |
{{#if Mixin}}
| `src/main/java/com/{{AuthorFlat}}/{{ModFlat}}/mixin/ExampleMixin.java` | A mixin that logs a line when a world loads. |
| `src/main/resources/{{ModId}}.mixins.json` | Lists the mixin classes. A mixin that is not listed here does nothing. |
{{/if}}
{{#if Client}}
| `src/client/java/com/{{AuthorFlat}}/{{ModFlat}}/client/{{ClassName}}Client.java` | The client entrypoint. Client-only code (anything from `net.minecraft.client`) goes under `src/client`. |
{{/if}}
{{#if Client && Mixin}}
| `src/client/java/.../client/mixin/ExampleClientMixin.java` and `src/client/resources/{{ModId}}.client.mixins.json` | A client mixin and its list. |
{{/if}}

There is no `gradlew`, `gradlew.bat` or `gradle-wrapper.jar` in this folder. IntelliJ IDEA does not need them: it
reads `gradle-wrapper.properties` and downloads that Gradle by itself. To build from a terminal, install Gradle
once and run this in the project folder to create the wrapper scripts:

```
gradle wrapper
```

## Build and run

```
gradlew runClient     starts a development game with the mod loaded
gradlew build         builds the jar
```

The mod is `build/libs/{{ModId}}-{{Version}}.jar`. The `-sources` jar next to it is not a mod.

## Install

1. Install Fabric Loader for Minecraft {{MinecraftVersion}} with the installer from fabricmc.net.
2. Put the Fabric API jar for {{MinecraftVersion}} and `{{ModId}}-{{Version}}.jar` in
   `{{#if GamePath}}{{GamePath}}{{#else}}%APPDATA%\.minecraft{{/if}}\mods`.
3. Start the Fabric profile in the launcher.

`logs/latest.log` then has the line `{{ModName}} {{Version}} loaded`.
{{#if Command}}
In a world, `/{{ModId}}` answers with a greeting.
{{/if}}
{{#if Item}}
The item is in the Ingredients tab of the creative inventory, or `/give @s {{ModId}}:example_item`.
{{/if}}

## When it does not load

- **The game starts but the mod is not in the list and the log has no line:** the jar is not in the `mods` folder
  of the game folder the Fabric profile uses, or you started a profile without Fabric.
- **"requires fabric-api" on the start screen:** Fabric API is a separate download and must be in `mods` too.
- **"requires version ~{{MinecraftVersion}} of minecraft":** the mod is built for one Minecraft version. To move it,
  change `minecraft_version` and `fabric_api_version` in `gradle.properties` and `"minecraft"` in `fabric.mod.json`.
- **The mod id is in three places and they must agree:** `"id"` in `fabric.mod.json`, `MOD_ID` in
  `{{ClassName}}.java`, and the folder name `assets/{{ModId}}`. The id may only use lower case letters, digits and `_`.
- **An entrypoint class was renamed or moved:** `fabric.mod.json` names the classes by full name
  (`{{Package}}.{{ClassName}}`). A wrong name crashes at start with ClassNotFoundException.
{{#if Mixin}}
- **A mixin does nothing:** its class name is missing from `{{ModId}}.mixins.json`, or the `"package"` there no
  longer matches the folder. A wrong method name crashes at start instead, because `defaultRequire` is 1.
{{/if}}
{{#if Item}}
- **The item is a purple and black cube:** `assets/{{ModId}}/items/example_item.json` is missing or points at a
  model that does not exist. File names must equal the item's id.
{{/if}}
- **`Could not find method modImplementation()` or `Task 'remapJar' not found`:** those belong to 1.21.x guides.
  Minecraft 26.x is not obfuscated: use `implementation`, no `mappings` line, and the `jar` task is the final jar.
- **`Unsupported class file major version 69`:** something is running on a JDK older than 25.
