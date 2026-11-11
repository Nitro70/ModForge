plugins {
    // Lets Gradle download JDK 25 by itself when it is not installed.
    id("org.gradle.toolchains.foojay-resolver-convention") version "1.0.0"
}

// Names the built jar.
rootProject.name = "{{PluginName}}"
