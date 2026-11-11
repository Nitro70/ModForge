extends Node
# {{ModId}} {{Version}}
# The mod's entry point. The Godot Mod Loader creates this node and calls _init() while the game starts,
# before the game's own scenes exist. Script extensions must be installed here, in _init().

# These names must be unique to this mod, in every file that uses them.
const {{ConstPrefix}}_DIR := "{{ModId}}"
const {{ConstPrefix}}_LOG_NAME := "{{ModId}}:Main"

var mod_dir_path := ""
var extensions_dir_path := ""
var translations_dir_path := ""


func _init() -> void:
{{#if Godot == 4}}
	mod_dir_path = ModLoaderMod.get_unpacked_dir().path_join({{ConstPrefix}}_DIR)
{{#else}}
	mod_dir_path = ModLoaderMod.get_unpacked_dir().plus_file({{ConstPrefix}}_DIR)
{{/if}}
	# Add extensions
	install_script_extensions()
	# Add translations
	add_translations()


func install_script_extensions() -> void:
{{#if Godot == 4}}
	extensions_dir_path = mod_dir_path.path_join("extensions")
{{#if Extension}}
	# Each extension is a script in the extensions folder that extends one of the game's scripts.
	ModLoaderMod.install_script_extension(extensions_dir_path.path_join("example_extension.gd"))
{{#else}}
	# ModLoaderMod.install_script_extension(extensions_dir_path.path_join(...))
{{/if}}
{{#else}}
	extensions_dir_path = mod_dir_path.plus_file("extensions")
{{#if Extension}}
	# Each extension is a script in the extensions folder that extends one of the game's scripts.
	ModLoaderMod.install_script_extension(extensions_dir_path.plus_file("example_extension.gd"))
{{#else}}
	# ModLoaderMod.install_script_extension(extensions_dir_path.plus_file(...))
{{/if}}
{{/if}}


func add_translations() -> void:
{{#if Godot == 4}}
	translations_dir_path = mod_dir_path.path_join("translations")
	# ModLoaderMod.add_translation(translations_dir_path.path_join(...))
{{#else}}
	translations_dir_path = mod_dir_path.plus_file("translations")
	# ModLoaderMod.add_translation(translations_dir_path.plus_file(...))
{{/if}}


# The game's scene tree exists now. This line in the log is the proof that the mod loaded.
func _ready() -> void:
	ModLoaderLog.info("Ready! Version {{Version}}", {{ConstPrefix}}_LOG_NAME)
	# TODO: your code here
