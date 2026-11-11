-- The mod's options screen, opened with the Options button in the mod list of
-- the main menu. It runs on its own, outside any level, so it is a plain script
-- with a draw function and no server or client part.
--
-- Settings are kept in the registry under savegame.mod, which is private to
-- this mod and saved with the player's profile. main.lua reads the same key.

function draw()
	local quiet = GetBool("savegame.mod.quiet")
	UiPush()
		UiTranslate(UiCenter(), 200)
		UiAlign("center middle")
		UiFont("bold.ttf", 44)
		UiText("{{ModName}}")
		UiTranslate(0, 90)
		UiFont("regular.ttf", 26)
		UiButtonImageBox("ui/common/box-outline-6.png", 6, 6)
		local label = "On-screen message: on"
		if quiet then
			label = "On-screen message: off"
		end
		if UiTextButton(label, 360, 44) then
			SetBool("savegame.mod.quiet", not quiet)
		end
		UiTranslate(0, 90)
		if UiTextButton("Close", 200, 44) then
			Menu()
		end
	UiPop()
end
