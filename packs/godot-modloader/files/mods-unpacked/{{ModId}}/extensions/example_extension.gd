# A script extension: this script becomes a child of the game's script, and the Mod Loader makes the game
# use it in place of the original. Methods written here replace the game's methods of the same name.
# Change the path below to the game script you want to change. By convention the extension file sits at the
# same relative path as the original (res://ui/menu.gd is extended by extensions/ui/menu.gd); if you move
# this file, change the path in mod_main.gd too.
extends "{{VanillaScript}}"

# Example. Rename it to a method that exists in the game's script and remove the leading "# ".
#
{{#if Godot == 4}}
# func some_method(argument):
# 	# Code here runs before the game's own method.
# 	var result = super(argument)
# 	# Code here runs after it and can change what it returns.
# 	return result
#
# Not calling super() replaces the game's method completely, which also cuts off other mods that extend it.
# Scripts with a class_name cannot be extended in Godot 4: use the Mod Loader's script hooks for those.
{{#else}}
# func some_method(argument):
# 	# Code here runs before the game's own method.
# 	var result = .some_method(argument)
# 	# Code here runs after it and can change what it returns.
# 	return result
#
# Not calling the base method replaces the game's method completely, which also cuts off other mods that
# extend it. In Godot 3 the engine callbacks (_ready, _process, _init...) cannot be replaced: the game's
# version always runs too, so do not call ._ready() from an extension.
{{/if}}
