-- Settings stage: runs first, before any prototype exists. Only setting
-- prototypes can be defined here.
--
-- setting_type decides where the setting lives:
--   "startup"          read in data.lua as settings.startup[name].value, needs a game restart
--   "runtime-global"   read in control.lua as settings.global[name].value, one value per save
--   "runtime-per-user" read in control.lua through the player, one value per player
--
-- Setting names are shared by all mods, so they start with the mod's name.

data:extend({
  {
    type = "bool-setting",
    name = "{{ModId}}-welcome",
    setting_type = "runtime-global",
    default_value = true
  }
})
