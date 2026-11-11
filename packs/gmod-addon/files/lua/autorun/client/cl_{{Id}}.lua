-- {{ModName}}: client side.
--
-- Files in lua/autorun/client run on each player's PC only. Drawing, menus,
-- sounds and key handling belong here. Nothing in this file can be trusted by
-- the server: a player can change what runs on their own PC.

local PREFIX = "[{{ModName}}] "
local nameColor = Color(120, 200, 255)
local textColor = Color(255, 255, 255)

-- Receives the message the server sends from the {{Id}}_hello command.
net.Receive("{{Id}}_hello", function()
	local text = net.ReadString()
	chat.AddText(nameColor, PREFIX, textColor, text)
end)

-- Runs once when the player has fully loaded into the map.
hook.Add("InitPostEntity", "{{Id}}_ready", function()
	chat.AddText(nameColor, PREFIX, textColor,
		"version {{Version}} loaded. Type {{Id}}_hello in the console.")
end)

print(PREFIX .. "client file loaded")
