package {{Package}};

import org.bukkit.event.EventHandler;
import org.bukkit.event.Listener;
import org.bukkit.event.player.PlayerJoinEvent;

// Every public method marked @EventHandler is called for the event type of its parameter.
public final class JoinListener implements Listener {
    @EventHandler
    public void onPlayerJoin(PlayerJoinEvent event) {
        event.getPlayer().sendPlainMessage("Hello, " + event.getPlayer().getName() + "!");
    }
}
