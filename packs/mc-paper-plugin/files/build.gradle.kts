plugins {
    java
}

group = "{{Package}}"
version = "{{Version}}"

repositories {
    mavenCentral()
    maven(url = "https://repo.papermc.io/repository/maven-public/") {
        name = "papermc"
    }
}

dependencies {
    // The server provides the API at runtime, so it is compileOnly and never goes into the jar.
    // The + takes the newest build for this Minecraft version.
    compileOnly("io.papermc.paper:paper-api:{{PaperVersion}}.build.+")
}

java {
    // Paper 26.x runs on Java 25.
    toolchain.languageVersion.set(JavaLanguageVersion.of(25))
}

tasks.withType<JavaCompile>().configureEach {
    options.encoding = "UTF-8"
}

tasks.processResources {
    // Puts the version from this file into plugin.yml.
    val props = mapOf("version" to project.version.toString())
    inputs.properties(props)
    filesMatching("plugin.yml") {
        expand(props)
    }
}
