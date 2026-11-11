#version 2

{{#if Kind == global}}
-- {{ModName}} by {{Author}}
--
-- A global mod: this script runs in every level while the mod is enabled.
--
-- "#version 2" on the first line selects the multiplayer script model. The file
-- is loaded twice, once on the server (the host) and once on every client:
--   server.*  runs on the host only. Physics, Spawn, MakeHole, Delete, Explosion.
--   client.*  runs on every player's PC. Input, sound, particles, lights and UI.
-- Both tables exist in both places, so "if server then" tells you nothing.
-- Server to clients: the shared table, or ClientCall(0, "client.name", ...).
-- Client to server: ServerCall("server.name", ...).
--
-- Every game function used here is documented in data\script_defs.lua in the
-- game folder. If a function is not in that file, it does not exist.

#include "script/include/player.lua"

local MESSAGE_TIME = 8    -- seconds the "loaded" line stays on screen
local NOTICE_TIME = 3     -- seconds a button notice stays on screen

---------------------------------------------------------------------------
-- Server
---------------------------------------------------------------------------

-- State lives in the shared table so clients can read it. It is created on
-- first use, not only in init, so it still exists after a quickload.
local function state()
	if shared.{{ModId}} == nil then
		shared.{{ModId}} = { presses = 0 }
	end
	return shared.{{ModId}}
end

-- Called once when the level starts.
function server.init()
	state()
end

-- Called every frame. dt is the frame time in seconds.
function server.tick(dt)
end

-- Called once per physics step, at a fixed rate. Forces and velocity changes
-- belong here. GetTime() returns simulated time only in here.
function server.update(dt)
end

-- Called by a client through ServerCall. The player id is sent along because
-- the server cannot tell who called.
function server.{{ModId}}Pressed(playerId)
	local st = state()
	st.presses = st.presses + 1
	ClientCall(0, "client.{{ModId}}Notice", GetPlayerName(playerId), st.presses)
end

---------------------------------------------------------------------------
-- Client
---------------------------------------------------------------------------

local age = 0
local notice = ""
local noticeTimer = 0
{{#if Options}}
local quiet = false
{{/if}}

-- Called once on each player's PC when the level starts.
function client.init()
	age = 0
{{#if Options}}
	-- Set on the options screen (options.lua).
	quiet = GetBool("savegame.mod.quiet")
{{/if}}
end

-- Called every frame on each player's PC.
function client.tick(dt)
	age = age + dt
	noticeTimer = math.max(0, noticeTimer - dt)

	-- Alt+J asks the server to count the press and tell everyone.
	if InputDown("alt") and InputPressed("j") then
		ServerCall("server.{{ModId}}Pressed", GetLocalPlayer())
	end
end

-- Called on every client by the server.
function client.{{ModId}}Notice(playerName, total)
	notice = playerName .. " pressed Alt+J (" .. tostring(total) .. " so far)"
	noticeTimer = NOTICE_TIME
end

-- Called every frame after the world is drawn. Only Ui functions belong here.
function client.draw(dt)
{{#if Options}}
	if quiet then return end
{{/if}}
	if age > MESSAGE_TIME and noticeTimer <= 0 then return end
	UiPush()
		UiTranslate(UiCenter(), 60)
		UiAlign("center middle")
		UiFont("regular.ttf", 26)
		UiTextOutline(0, 0, 0, 1, 0.1)
		UiColor(1, 1, 1, 1)
		if noticeTimer > 0 then
			UiText(notice)
		else
			UiText("{{ModName}} loaded. Press Alt+J.")
		end
	UiPop()
end
{{#elif Kind == tool}}
-- {{ModName}} by {{Author}}
--
-- A tool mod: adds one tool to every player's inventory. Left mouse button
-- punches a hole where the player is aiming.
--
-- "#version 2" on the first line selects the multiplayer script model. The file
-- is loaded twice, once on the server (the host) and once on every client:
--   server.*  runs on the host only. RegisterTool, MakeHole, Spawn, Explosion.
--   client.*  runs on every player's PC. Sound, particles, lights and UI.
-- Both tables exist in both places, so "if server then" tells you nothing.
--
-- Every game function used here is documented in data\script_defs.lua in the
-- game folder. If a function is not in that file, it does not exist.

#include "script/include/player.lua"

local TOOL_ID = "{{ModId}}"   -- internal id, also the prefab file name
local RANGE = 50              -- metres
local HOLE_SOFT = 0.8         -- hole radius in wood, plaster, glass
local HOLE_MEDIUM = 0.5       -- in brick, concrete, weak metal
local HOLE_HARD = 0.2         -- in hard metal and hard masonry

---------------------------------------------------------------------------
-- Server
---------------------------------------------------------------------------

-- Called once when the level starts. A tool is registered exactly once, and it
-- must point at the prefab, not at a .vox: the prefab is what places the model
-- in the player's hands.
function server.init()
	RegisterTool(TOOL_ID, "{{ModName}}", "MOD/prefab/{{ModId}}.xml", 6)
end

local function fire(p)
	-- Aim from the tool's muzzle when the tool is visible, else from the eyes.
	local origin = GetPlayerEyeTransform(p).pos
	local muzzle = GetToolLocationWorldTransform("muzzle", p)
	if muzzle ~= nil then
		origin = muzzle.pos
	end
	local hit, startPos, endPos, dir = GetPlayerAimInfo(origin, RANGE, p)
	if hit then
		MakeHole(endPos, HOLE_SOFT, HOLE_MEDIUM, HOLE_HARD)
	end
end

function server.tick(dt)
	-- A registered tool is disabled until it is enabled for each player.
	for p in PlayersAdded() do
		SetToolEnabled(TOOL_ID, true, p)
		SetToolAmmo(TOOL_ID, 9999, p)
	end

	for p in Players() do
		if GetPlayerTool(p) == TOOL_ID and GetPlayerVehicle(p) == 0 then
			if InputPressed("usetool", p) then
				fire(p)
			end
		end
	end
end

---------------------------------------------------------------------------
-- Client
---------------------------------------------------------------------------

local fireSound = 0
local age = 0
{{#if Options}}
local quiet = false
{{/if}}

function client.init()
	fireSound = LoadSound("tools/gun0.ogg")
	age = 0
{{#if Options}}
	-- Set on the options screen (options.lua).
	quiet = GetBool("savegame.mod.quiet")
{{/if}}
end

function client.tick(dt)
	age = age + dt

	-- The same input the server reacts to, used here only for sound and light,
	-- so every player hears and sees every other player's shots.
	for p in Players() do
		if GetPlayerTool(p) == TOOL_ID and GetPlayerVehicle(p) == 0 then
			if InputPressed("usetool", p) then
				local pos = GetPlayerTransform(p).pos
				local muzzle = GetToolLocationWorldTransform("muzzle", p)
				if muzzle ~= nil then
					pos = muzzle.pos
				end
				PlaySound(fireSound, pos)
				PointLight(pos, 1, 0.7, 0.5, 3)
			end
		end
	end
end

function client.draw(dt)
{{#if Options}}
	if quiet then return end
{{/if}}
	if age > 8 then return end
	UiPush()
		UiTranslate(UiCenter(), 60)
		UiAlign("center middle")
		UiFont("regular.ttf", 26)
		UiTextOutline(0, 0, 0, 1, 0.1)
		UiColor(1, 1, 1, 1)
		UiText("{{ModName}} loaded. Scroll to the tool and press the left mouse button.")
	UiPop()
end
{{#else}}
-- {{ModName}} by {{Author}}
--
-- This file does nothing on purpose. The spawnable itself is listed in
-- spawn.txt and built by spawn/{{ModId}}.xml.
--
-- It is here because a multiplayer session can only enable a mod that has a
-- main.lua. Without it the mod is listed for the session but cannot be turned on.

function server.init()
end

function client.init()
end
{{/if}}
