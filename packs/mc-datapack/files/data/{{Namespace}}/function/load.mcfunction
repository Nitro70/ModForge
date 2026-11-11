# Runs once when the world loads and again after every /reload.
# It is called because data/minecraft/tags/function/load.json lists {{Namespace}}:load.
# When a world is opened this runs before any player has joined, so the message
# below is only seen after /reload.

tellraw @a {"text":"{{ModName|json}} loaded","color":"green"}
