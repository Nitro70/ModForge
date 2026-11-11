-- {{ModName}} {{Version}} by {{Author}}
--
-- Game engine (GE) extension: one copy runs for the whole game. It can see the
-- world, the mod manager and every vehicle object, but NOT a vehicle's own Lua
-- (controller, electrics and powertrain live in each vehicle's separate Lua state).
--
-- The file name is the extension name. From the in-game console (GE - Lua) the
-- public functions are reachable as {{ExtName}}.<name>().

local M = {}

local logTag = '{{ExtName}}'

-- Called once when the extension is loaded.
local function onExtensionLoaded()
  log('I', logTag, '{{ModName}} {{Version}} loaded')
end

-- Called after every vehicle spawn (also after a reload with Ctrl+R).
local function onVehicleSpawned(vehicleId)
  log('I', logTag, 'vehicle spawned: ' .. tostring(vehicleId))
end

-- Called every frame. dtReal is the real frame time in seconds, dtSim is the
-- simulated time (0 while paused).
local function onUpdate(dtReal, dtSim, dtRaw)
end

-- Shows a message at the top left of the screen. Try it from the console:
--   {{ExtName}}.hello()
local function hello()
  ui_message('{{ModName}} says hello', 5, logTag)
end

-- Runs a line of Lua inside the player's vehicle. This is how GE code reaches
-- vehicle things. The command is queued: nothing is returned.
--   {{ExtName}}.runInPlayerVehicle('print(electrics.values.wheelspeed)')
local function runInPlayerVehicle(luaCode)
  local veh = getPlayerVehicle(0)
  if not veh then
    log('W', logTag, 'no player vehicle')
    return false
  end
  veh:queueLuaCommand(luaCode)
  return true
end

M.onExtensionLoaded = onExtensionLoaded
M.onVehicleSpawned = onVehicleSpawned
M.onUpdate = onUpdate
M.hello = hello
M.runInPlayerVehicle = runInPlayerVehicle

return M
