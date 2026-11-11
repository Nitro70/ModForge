-- {{ModName}} {{Version}} by {{Author}}
--
-- Vehicle extension: every file in lua/vehicle/extensions/auto/ is loaded into
-- EVERY vehicle's own Lua state, including traffic and props. Each vehicle has
-- its own copy, and the copy is rebuilt whenever the vehicle is respawned.
--
-- Here you have electrics, controller, powertrain, input and obj (the vehicle
-- itself). You do not have the game engine's globals: talk to that side with
-- obj:queueGameEngineLua('...').
--
-- In the in-game console pick the vehicle in the dropdown at the bottom left,
-- then the public functions are reachable as {{ExtName}}.<name>().

local M = {}

local logTag = '{{ExtName}}'
local announced = false

-- Called once when this vehicle's Lua state loads the extension.
local function onExtensionLoaded()
  log('I', logTag, '{{ModName}} {{Version}} loaded in vehicle ' .. tostring(obj:getId()))
end

-- Called when the vehicle is reset. Put state back to its start values here.
local function onReset()
  announced = false
end

-- Called every graphics frame with the frame time in seconds.
local function updateGFX(dt)
  -- Only a vehicle with the player in it shows the on-screen message.
  if not announced and playerInfo.anyPlayerSeated then
    announced = true
    guihooks.message('{{ModName}} is active in this vehicle', 5, logTag)
  end
end

-- Example of reading a value. Try it from the console (vehicle Lua):
--   print({{ExtName}}.speedKmh())
local function speedKmh()
  return (electrics.values.wheelspeed or 0) * 3.6
end

M.onExtensionLoaded = onExtensionLoaded
M.onReset = onReset
M.updateGFX = updateGFX
M.speedKmh = speedKmh

return M
