-- {{ModName}}: a scripted entity (SENT), the part both sides load.
--
-- The folder name is the entity's class name: {{Id}}_crate. The game loads
-- init.lua on the server and cl_init.lua on the client; each of them includes
-- this file.

ENT.Type = "anim"
ENT.Base = "base_gmodentity"

ENT.PrintName = "{{ModName}} Crate"
ENT.Author = "{{Author}}"
ENT.Category = "{{ModName}}"

ENT.Spawnable = true         -- listed in the spawn menu under Entities
ENT.AdminOnly = false
