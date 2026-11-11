-- {{ModName}}: server side.
--
-- Files in lua/autorun/server run on the server only and are never sent to
-- players. Game rules, damage, spawning and anything players must not be able
-- to change belong here. In single player your own PC is the server too.

-- The shared file normally runs first. These two lines make this file work
-- even if it does not.
{{Id}} = {{Id}} or {}

local function log(text)
	print("[{{ModName}}] " .. tostring(text))
end

-- Every net message name has to be registered on the server before use.
util.AddNetworkString("{{Id}}_hello")

-- hook.Add(event, unique name, function). The unique name must differ from
-- every other addon's, which is why it starts with the addon id.
hook.Add("PlayerInitialSpawn", "{{Id}}_welcome", function(ply)
	log(ply:Nick() .. " joined")
end)

-- A console command. On the server the first argument is the player who ran
-- it, or an invalid entity when it was typed into the server console.
concommand.Add("{{Id}}_hello", function(ply, cmd, args)
	if not IsValid(ply) then
		log("hello from the server console")
		return
	end

	-- Send a message to that one player. The client file receives it.
	net.Start("{{Id}}_hello")
	net.WriteString("Hello, " .. ply:Nick() .. "!")
	net.Send(ply)
end)

log("server file loaded")
