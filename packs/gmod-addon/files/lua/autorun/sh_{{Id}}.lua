-- {{ModName}} {{Version}} by {{Author}}
--
-- Shared: every file directly in lua/autorun runs on the server AND on each
-- client when Lua starts. Put what both sides need here: tables, settings,
-- helper functions.
--
-- The SERVER and CLIENT globals say which side is running the file right now.

{{Id}} = {{Id}} or {}
{{Id}}.Name = "{{ModName}}"
{{Id}}.Version = "{{Version}}"

-- Clients only receive Lua files the server has marked for download.
-- Marking a file twice does no harm.
if SERVER then
	AddCSLuaFile()
	AddCSLuaFile("autorun/client/cl_{{Id}}.lua")
end

function {{Id}}.Log(text)
	print("[" .. {{Id}}.Name .. "] " .. tostring(text))
end

{{Id}}.Log("shared file loaded on the " .. (SERVER and "server" or "client"))
