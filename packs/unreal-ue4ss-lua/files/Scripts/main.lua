-- {{ModName}} {{Version}} by {{Author}}
-- A UE4SS Lua mod. UE4SS runs this file once when the mod starts.
--
-- Rules that keep a UE4SS Lua mod from crashing the game:
--   * Key binds run on UE4SS's own thread. Touch game objects only inside ExecuteInGameThread.
--   * Do not keep a game object (anything under /Game/) in a variable across map loads. It gets freed, and
--     IsValid() on a freed object can crash instead of returning false. Look the object up again when needed.
--   * Never call FindFirstOf, FindAllOf or FindObjects from code that runs every frame.
--   * Do not use RegisterLoadMapPreHook/PostHook, RegisterInitGameStatePreHook/PostHook or
--     RegisterBeginPlayPreHook/PostHook on UE4SS 3.0.1: its code for them keeps a pointer that goes stale.
--   * Keep console commands plus NotifyOnNewObject calls well under 45 per mod on UE4SS 3.0.1.

local MOD_NAME = "{{ProjectName}}"
local MOD_VERSION = "{{Version}}"

-- Shows up in UE4SS.log and in the UE4SS console window as: [Lua] [{{ProjectName}}] message
local function Log(message)
    print("[" .. MOD_NAME .. "] " .. tostring(message) .. "\n")
end
{{#if FindHelper}}

-- The first live object of a class, or nil. Takes the short class name: "PlayerController", "Character".
-- FindFirstOf hands back an invalid object, not nil, when nothing matches: this hides that.
-- It searches every object in the game, so call it when something happens, not every frame.
local function Find(shortClassName)
    local ok, object = pcall(FindFirstOf, shortClassName)
    if ok and object ~= nil and object:IsValid() then
        return object
    end
    return nil
end
{{/if}}
{{#if Keybind}}

-- Key bind: {{Key}}. For a modifier use RegisterKeyBind(Key.{{Key}}, { ModifierKey.CONTROL }, function() ... end).
RegisterKeyBind(Key.{{Key}}, function()
    -- This runs on UE4SS's key thread. Everything that touches the game goes through ExecuteInGameThread.
    ExecuteInGameThread(function()
        Log("{{Key}} pressed")
{{#if FindHelper}}
        local controller = Find("PlayerController")
        if controller then
            Log("player controller: " .. controller:GetFullName())
        else
            Log("no PlayerController yet")
        end
{{/if}}
        -- TODO: your code here
    end)
end)
{{/if}}
{{#if ConsoleCommand}}

-- Console command: type "{{CommandName}}" into the game's own console. UE4SS's ConsoleEnablerMod turns that
-- console on; its stock key is F10.
-- Parameters is a table of the words after the command. Return true to say the command was handled.
RegisterConsoleCommandHandler("{{CommandName}}", function(FullCommand, Parameters, Ar)
    local reply = MOD_NAME .. " " .. MOD_VERSION .. ": " .. #Parameters .. " parameter(s)"
    Log(reply)
    -- Ar writes into the game's console. It is not always an output device, so check before using it.
    if type(Ar) == "userdata" and Ar:type() == "FOutputDevice" then
        Ar:Log(reply)
    end
    -- TODO: your code here
    return true
end)
{{/if}}
{{#if Hook}}

-- Function hook. ClientRestart is an engine function the game calls when the local player gets a pawn
-- (at spawn, at respawn and after a map load), so this is a good place to set things up for the player.
-- The name is the function's full path. Functions under /Script/ are native: the callback runs before the
-- function. Blueprint functions (under /Game/) must be loaded before they can be hooked, and their callback
-- runs after the function has finished.
-- RegisterHook throws when the function does not exist, so it is called through pcall.
local hooked, hookError = pcall(RegisterHook, "/Script/Engine.PlayerController:ClientRestart", function(Context, NewPawn)
    local controller = Context:get()
    local pawn = NewPawn:get()
    if pawn:IsValid() then
        Log("ClientRestart: " .. controller:GetFullName() .. " now controls " .. pawn:GetFullName())
    end
    -- TODO: your code here
end)
if not hooked then
    Log("could not hook ClientRestart: " .. tostring(hookError))
end
{{/if}}
{{#if Notify}}

-- New object notification: runs each time the game constructs an object of this class or a class derived
-- from it. The object is brand new: its properties may not be filled in yet.
-- Call NotifyOnNewObject once per class and do all the work in that one callback.
NotifyOnNewObject("/Script/Engine.PlayerController", function(ConstructedObject)
    Log("new PlayerController: " .. ConstructedObject:GetFullName())
    -- TODO: your code here
    -- Return true to stop getting notifications.
end)
{{/if}}

Log("loaded " .. MOD_VERSION)
