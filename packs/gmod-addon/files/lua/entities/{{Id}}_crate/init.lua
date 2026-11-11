-- {{ModName}}: scripted entity, server side.

-- Send the client files to players, then load the shared part.
AddCSLuaFile("cl_init.lua")
AddCSLuaFile("shared.lua")
include("shared.lua")

function ENT:Initialize()
	-- A model that ships with the game.
	self:SetModel("models/props_junk/wood_crate001a.mdl")
	self:PhysicsInit(SOLID_VPHYSICS)
	self:SetMoveType(MOVETYPE_VPHYSICS)
	self:SetSolid(SOLID_VPHYSICS)
	self:SetUseType(SIMPLE_USE)   -- Use fires once per key press, not every tick

	local phys = self:GetPhysicsObject()
	if IsValid(phys) then
		phys:Wake()
	end
end

-- Called when a player presses the use key (E) on the entity.
function ENT:Use(activator)
	if IsValid(activator) and activator:IsPlayer() then
		activator:ChatPrint("You used the {{ModName}} crate")
	end
end
