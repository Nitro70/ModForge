-- {{ModName}}: a scripted weapon (SWEP).
--
-- The file name is the weapon's class name: weapon_{{Id}}. A weapon can also
-- be a folder (lua/weapons/<class>/shared.lua, init.lua, cl_init.lua); a single
-- file like this one runs on both the server and the client.

AddCSLuaFile()

SWEP.PrintName = "{{ModName}}"
SWEP.Author = "{{Author}}"
SWEP.Instructions = "Left click: push what you look at. Right click: say hello."
SWEP.Category = "{{ModName}}"

SWEP.Spawnable = true        -- listed in the spawn menu under Weapons
SWEP.AdminOnly = false

SWEP.Slot = 1                -- weapon bar column, counted from 0
SWEP.SlotPos = 2
SWEP.DrawAmmo = false
SWEP.DrawCrosshair = true

-- Models that ship with the game. UseHands draws the player model's hands on
-- a view model whose name starts with c_.
SWEP.ViewModel = "models/weapons/c_pistol.mdl"
SWEP.WorldModel = "models/weapons/w_pistol.mdl"
SWEP.UseHands = true

SWEP.Primary.ClipSize = -1
SWEP.Primary.DefaultClip = -1
SWEP.Primary.Automatic = false
SWEP.Primary.Ammo = "none"

SWEP.Secondary.ClipSize = -1
SWEP.Secondary.DefaultClip = -1
SWEP.Secondary.Automatic = false
SWEP.Secondary.Ammo = "none"

function SWEP:Initialize()
	self:SetHoldType("pistol")
end

-- Called on the server and on the firing player's client.
function SWEP:PrimaryAttack()
	self:SetNextPrimaryFire(CurTime() + 0.4)
	self:EmitSound("Weapon_Pistol.Empty")

	-- Changing the world is the server's job.
	if CLIENT then return end

	local owner = self:GetOwner()
	if not IsValid(owner) then return end

	local trace = owner:GetEyeTrace()
	local target = trace.Entity
	if not IsValid(target) then return end

	local phys = target:GetPhysicsObject()
	if IsValid(phys) then
		phys:ApplyForceCenter(owner:GetAimVector() * phys:GetMass() * 500)
	end
end

function SWEP:SecondaryAttack()
	self:SetNextSecondaryFire(CurTime() + 1)

	if CLIENT then return end

	local owner = self:GetOwner()
	if IsValid(owner) then
		owner:ChatPrint("Hello from {{ModName}}")
	end
end
