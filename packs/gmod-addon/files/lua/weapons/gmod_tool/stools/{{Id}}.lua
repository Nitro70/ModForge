-- {{ModName}}: a tool for the tool gun (STOOL).
--
-- The file name is the tool's mode name: {{Id}}. The tool gun loads every file
-- in lua/weapons/gmod_tool/stools on both sides and sends it to clients itself.
-- The file name must be lowercase.

TOOL.Category = "Construction"
TOOL.Name = "#tool.{{Id}}.name"

-- The help lines shown at the top left while the tool is out.
TOOL.Information = {
	{ name = "left" },
	{ name = "right" }
}

if CLIENT then
	language.Add("tool.{{Id}}.name", "{{ModName}}")
	language.Add("tool.{{Id}}.desc", "Colours props")
	language.Add("tool.{{Id}}.left", "Paint the prop red")
	language.Add("tool.{{Id}}.right", "Put the colour back")
end

local function isUsable(trace)
	return IsValid(trace.Entity) and not trace.Entity:IsPlayer()
end

-- Return true to play the tool gun's shot effect, false to do nothing.
function TOOL:LeftClick(trace)
	if not isUsable(trace) then return false end
	if CLIENT then return true end

	trace.Entity:SetColor(Color(255, 60, 60))
	return true
end

function TOOL:RightClick(trace)
	if not isUsable(trace) then return false end
	if CLIENT then return true end

	trace.Entity:SetColor(Color(255, 255, 255))
	return true
end

-- The tool's panel in the spawn menu. Note the dot: this one is not a method.
function TOOL.BuildCPanel(panel)
	panel:AddControl("Header", { Description = "#tool.{{Id}}.desc" })
end
