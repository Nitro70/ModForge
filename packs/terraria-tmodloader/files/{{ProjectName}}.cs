using Terraria.ModLoader;

namespace {{Namespace}}
{
	// tModLoader creates the one class that inherits Mod when the mod is loaded.
	// Items, tiles, NPCs and so on are separate classes; tModLoader finds them by itself (see Content).
	public class {{ProjectName}} : Mod
	{
		public override void Load()
		{
			// Shows in tModLoader-Logs\client.log in the tModLoader install folder.
			Logger.Info($"{DisplayName} {Version} loaded");
		}
	}
}
