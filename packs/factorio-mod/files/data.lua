-- Data stage: runs once while the game starts, before any save is loaded.
-- Everything that exists in the game (items, recipes, entities, technologies)
-- is a prototype added to data.raw here. There is no game, no players and no
-- map at this point.
--
-- Prototype names are shared by all mods, so they start with the mod's name.
-- Paths start with __modname__: __base__ is the base game, __{{ModId}}__ is
-- this mod's own folder.
{{#if Item}}

data:extend({
  {
    type = "item",
    name = "{{ModId}}-gadget",
    -- The base game's gear icon, tinted. For your own picture put a 64x64 PNG in
    -- this mod and use icon = "__{{ModId}}__/graphics/gadget.png" instead of icons.
    icons = {
      {
        icon = "__base__/graphics/icons/iron-gear-wheel.png",
        icon_size = 64,
        tint = { r = 0.4, g = 0.8, b = 1.0, a = 1.0 }
      }
    },
    subgroup = "intermediate-product",
    order = "z[{{ModId}}-gadget]",
    stack_size = 100
  },
  {
    type = "recipe",
    name = "{{ModId}}-gadget",
    -- true: craftable from the start. Set false and unlock it from a technology instead.
    enabled = true,
    energy_required = 1,
    ingredients = {
      { type = "item", name = "iron-plate", amount = 2 }
    },
    results = {
      { type = "item", name = "{{ModId}}-gadget", amount = 1 }
    }
  }
})
{{/if}}
