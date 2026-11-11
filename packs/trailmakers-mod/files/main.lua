-- {{ModName}} {{Version}} by {{Author}}
--
-- Trailmakers runs this file once when the host turns the mod on for a session.
-- It runs on the host only and reaches the game through the tm table. Every tm
-- function is listed in mods\trailmakers_docs.lua in the game folder: if a
-- function is not in that file, it does not exist.
--
-- Anything written with tm.os.Log goes to mods\{{ModId}}.log in the game folder.

local MOD_NAME = "{{ModName}}"

local function log(message)
  tm.os.Log(MOD_NAME .. ": " .. tostring(message))
end

-- Per-player state, keyed by playerId. Player slots are reused: clear a slot
-- when its player leaves, or the next player in it inherits the old values.
local presses = {}
{{#if Hotkey}}

-- Key callbacks can never be taken away again, so each slot is bound only
-- once. Binding again when a new player takes the slot would fire twice.
local keyBound = {}

-- Valid key names are lowercase: letters, digits, "home", "end", "insert",
-- "page up", "page down", "left shift", "space" and so on. There are no
-- function keys and no escape. The builder uses x for delete and c for copy.
local HOTKEY = "{{Key}}"
{{/if}}
{{#if Menu}}

local function refreshMenu(playerId)
  tm.playerUI.SetUIValue(playerId, "{{ModId}}_count", "Pressed: " .. tostring(presses[playerId] or 0))
end
{{/if}}

local function countPress(playerId)
  presses[playerId] = (presses[playerId] or 0) + 1
  log("player " .. tostring(playerId) .. " pressed, total " .. tostring(presses[playerId]))
{{#if Menu}}
  refreshMenu(playerId)
{{/if}}
  -- Header and message are cut off after 32 characters.
  tm.playerUI.AddSubtleMessageForPlayer(playerId, MOD_NAME, "Pressed " .. tostring(presses[playerId]) .. " times", 3)
end
{{#if Hotkey}}

-- The game finds a key callback by its NAME, so it has to be a global
-- function. It receives the id of the player who pressed the key.
function {{ModId}}_onHotkey(playerId)
  countPress(playerId)
end
{{/if}}
{{#if Menu}}

-- A button callback receives a table: playerId, id (of the button), value
-- (the text on it) and data (the last argument given to AddUIButton).
local function onMenuButton(callbackData)
  countPress(callbackData.playerId)
end

local function buildMenu(playerId)
  tm.playerUI.AddUILabel(playerId, "{{ModId}}_title", MOD_NAME .. " {{Version}}")
  tm.playerUI.AddUILabel(playerId, "{{ModId}}_count", "Pressed: 0")
  tm.playerUI.AddUIButton(playerId, "{{ModId}}_press", "Press me", onMenuButton, nil)
end
{{/if}}

-- Called for every player who joins, including the host when the mod starts.
local function onPlayerJoined(player)
  local playerId = player.playerId
  presses[playerId] = 0
  log("player joined: " .. tostring(tm.players.GetPlayerName(playerId)) .. " (id " .. tostring(playerId) .. ")")
{{#if Menu}}
  buildMenu(playerId)
{{/if}}
{{#if Hotkey}}
  if not keyBound[playerId] then
    keyBound[playerId] = true
    tm.input.RegisterFunctionToKeyDownCallback(playerId, "{{ModId}}_onHotkey", HOTKEY)
  end
{{/if}}
end

local function onPlayerLeft(player)
  local playerId = player.playerId
  presses[playerId] = nil
end

tm.players.OnPlayerJoined.add(onPlayerJoined)
tm.players.OnPlayerLeft.add(onPlayerLeft)
{{#if Update}}

-- The game calls the global function update over and over while the mod runs.
-- How often is set here: 1/60 asks for 60 times a second.
tm.os.SetModTargetDeltaTime(1 / 60)

local sinceLastSecond = 0

function update()
  sinceLastSecond = sinceLastSecond + tm.os.GetModDeltaTime()
  if sinceLastSecond < 1 then
    return
  end
  sinceLastSecond = 0
  -- Runs about once a second. Keep the work in here small: a slow update
  -- stalls the game for everyone.
end
{{/if}}

log("{{Version}} loaded")
