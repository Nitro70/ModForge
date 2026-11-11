-- {{ModName}} {{Version}} by {{Author}}
--
-- Steamodded runs this file once while the game starts, because the metadata
-- file {{ModId}}.json names it as "main_file". Everything the mod adds to the
-- game is registered from here.
--
-- Do not put a "--- STEAMODDED HEADER" line in this file: that is the old way
-- to describe a mod, and with it Steamodded would try to load the mod twice.

-- Shows up in the Lovely console window and in Mods/lovely/log.
sendInfoMessage("{{ModName}} {{Version}} loaded", "{{ModId}}")
{{#if Joker}}

-- A Joker. The game refers to it as j_{{Prefix}}_example: the class prefix
-- "j", then the mod prefix from the metadata file, then the key below.
SMODS.Joker {
	key = "example",

	-- Name and description. #1# is replaced by the first value from loc_vars.
	-- {C:mult} colours the text like Mult, {} switches the colour off again.
	loc_txt = {
		name = "{{ModName}} Joker",
		text = {
			"{C:mult}+#1#{} Mult"
		}
	},

	-- The card's numbers. Keep every number here and read it from
	-- card.ability, so the description and the effect can never disagree.
	config = { extra = { mult = 4 } },

	-- Hands the values to the description above.
	loc_vars = function(self, info_queue, card)
		return { vars = { card.ability.extra.mult } }
	end,

	rarity = 1,              -- 1 common, 2 uncommon, 3 rare, 4 legendary
	cost = 2,                -- price in the shop
	unlocked = true,
	discovered = true,       -- visible in the collection from the start
	blueprint_compat = true,

	-- The picture. With no atlas given the card uses the game's own Joker
	-- sheet, and pos picks the sprite: x 0, y 0 is the plain Joker.
	-- For your own art see "Your own artwork" in README.md.
	pos = { x = 0, y = 0 },

	-- Called at every step of scoring. context says which step it is.
	calculate = function(self, card, context)
		-- joker_main: the moment Jokers add their own Mult and Chips.
		if context.joker_main then
			return {
				mult = card.ability.extra.mult
			}
		end
	end
}
{{/if}}
