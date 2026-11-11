-- {{ModName}}: scripted entity, client side.

include("shared.lua")

-- Called every frame the entity is on screen.
function ENT:Draw()
	self:DrawModel()
end
