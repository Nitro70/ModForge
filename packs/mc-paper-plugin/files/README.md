# {{ModName}}

A plugin for Paper servers running Minecraft {{PaperVersion}}. Plugin name: `{{PluginName}}`.

A plugin runs on the server only. Players join with an unmodified game.

## What you need

- **JDK 25.** If it is not installed, Gradle downloads one by itself (that is what the foojay line in
  `settings.gradle.kts` is for).
- A Paper server for Minecraft {{PaperVersion}} to test on (papermc.io/downloads).

## Layout

| File | What it is for |
|---|---|
| `build.gradle.kts` | The build: the Paper API dependency, Java 25, and the plugin's version. |
| `settings.gradle.kts` | The project name, which is also the jar's name. |
| `gradle/wrapper/gradle-wrapper.properties` | Which Gradle version builds the project ({{GradleVersion}}). |
| `src/main/resources/plugin.yml` | The file the server reads: name, version, main class, api-version. Without it the jar is not a plugin. |
| `src/main/java/com/{{AuthorFlat}}/{{ModFlat}}/{{ClassName}}.java` | The main class. `onEnable` runs when the server enables the plugin. |
{{#if Command}}
| `src/main/java/com/{{AuthorFlat}}/{{ModFlat}}/HelloCommand.java` | The `/{{CommandName}}` command. |
{{/if}}
{{#if Listener}}
| `src/main/java/com/{{AuthorFlat}}/{{ModFlat}}/JoinListener.java` | Greets players when they join. |
{{/if}}

There is no `gradlew`, `gradlew.bat` or `gradle-wrapper.jar` in this folder. IntelliJ IDEA does not need them: it
reads `gradle-wrapper.properties` and downloads that Gradle by itself. To build from a terminal, install Gradle
once and run this in the project folder to create the wrapper scripts:

```
gradle wrapper
```

## Build

```
gradlew build
```

The plugin is `build/libs/{{PluginName}}-{{Version}}.jar`.

## Install

Copy the jar into the `plugins` folder of the server and restart the server. `/reload` is not a safe way to load
a new build.

The console then prints `{{ModName}} {{Version}} enabled`, and `/plugins` lists `{{PluginName}}` in green.
{{#if Command}}
`/{{CommandName}}` answers with a greeting. From the console it works at once; in game you need to be an operator
or have the permission `{{CommandName}}.hello`.
{{/if}}
{{#if Listener}}
Joining the server shows a hello message in chat.
{{/if}}

## When it does not load

- **"Invalid plugin.yml" or the plugin is missing from /plugins:** `plugin.yml` is not at the root of the jar. It
  must stay in `src/main/resources`. Open the jar with a zip tool to check.
- **"Cannot find main class":** `main` in `plugin.yml` must be the full class name, `{{Package}}.{{ClassName}}`.
  Renaming the class or moving it to another package without changing `plugin.yml` breaks it.
- **"Unsupported API version":** the server is older than `api-version` ({{PaperVersion}}). Lower the value, or
  update the server.
- **"has been compiled by a more recent version of the Java Runtime":** the server runs on a Java older than 25.
- **The server is Spigot or CraftBukkit, not Paper:** this plugin uses Paper's own API (the command registration)
  and only loads on Paper and its forks.
- **A listener never fires:** it was not registered. `registerEvents` in `onEnable` does that, and the method needs
  `@EventHandler`.
- **The build cannot find `paper-api`:** the version in `build.gradle.kts` has the form `<Minecraft version>.build.+`
  and only exists for 26.1 and newer. For 1.21.11 and older the dependency is `<version>-R0.1-SNAPSHOT`.
- **The jar is in `mods` instead of `plugins`:** plugins are not mods. A Fabric or NeoForge server does not load them.
