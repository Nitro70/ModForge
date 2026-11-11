using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace {{Namespace}}
{
    /// <summary>
    /// The entry point. The game creates every class that inherits ModSystem and calls the Start methods
    /// when a world is loaded. A mod can have several of them.
    /// </summary>
    public class {{SystemClass}} : ModSystem
    {
        /// <summary>Called on the server and on the client.</summary>
        public override void Start(ICoreAPI api)
        {
            // Lang.Get reads assets/{{ModId}}/lang/en.json. The part before the colon is the mod id.
            Mod.Logger.Notification("{{ModName|str}} loaded: " + Lang.Get("{{ModId}}:hello"));
        }

        /// <summary>Called on the server only. Single player has a server too.</summary>
        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification("{{ModName|str}} started on the server side");
            // TODO: your code here
        }

        /// <summary>Called on the client only.</summary>
        public override void StartClientSide(ICoreClientAPI api)
        {
            Mod.Logger.Notification("{{ModName|str}} started on the client side");
            // TODO: your code here
        }
    }
}
