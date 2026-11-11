# Runs 20 times a second, forever.
# It is called because data/minecraft/tags/function/tick.json lists {{Namespace}}:tick.
# One command per line, without the leading slash. Keep this file cheap: everything here
# runs every tick. An example to try (remove the # in front):

# execute as @a at @s run particle minecraft:end_rod ~ ~0.1 ~ 0 0 0 0 1
