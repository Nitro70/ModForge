-- Control stage: runs while a save is being played. This is where a mod reacts
-- to events. The prototypes from data.lua can no longer be changed here.
--
-- Everything that must survive saving and loading goes into the storage table.
-- Local variables are lost on load, and in multiplayer every player must end up
-- with the same state, so never keep game state anywhere else.

local function welcome(player)
  if not settings.global["{{ModId}}-welcome"].value then
    return
  end
  -- A table instead of a string is a localised message: the key is looked up
  -- in locale/<language>/locale.cfg and __1__ is replaced by the next value.
  player.print({ "{{ModId}}.welcome", "{{Version}}" })
end

-- A new save is started with the mod, or the mod is added to an existing save.
script.on_init(function()
  storage.built = 0
  log("{{ModId}} {{Version}}: on_init")
  for _, player in pairs(game.players) do
    welcome(player)
  end
end)

-- The save was last played with a different set of mods or versions.
script.on_configuration_changed(function()
  storage.built = storage.built or 0
end)

-- A player is created: the first time each player enters the game.
script.on_event(defines.events.on_player_created, function(event)
  local player = game.get_player(event.player_index)
  if player then
    welcome(player)
  end
end)
{{#if Events}}

-- A player placed something. event.entity is what was built.
script.on_event(defines.events.on_built_entity, function(event)
  storage.built = (storage.built or 0) + 1
end)

-- Every 3600 ticks, which is one minute at normal speed.
script.on_nth_tick(3600, function(event)
  game.print({ "{{ModId}}.built", storage.built or 0 })
end)
{{/if}}
