-- Runs when the mod is activated. The mod manager loads every extension
-- registered as "manual" here right after it has run all mod scripts.
-- The name is the file name of lua/ge/extensions/{{ExtName}}.lua without .lua.
setExtensionUnloadMode('{{ExtName}}', 'manual')
