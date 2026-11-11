// The script module of the behavior pack. manifest.json names this file under "entry"
// and asks for @minecraft/server under "dependencies". Both must be there or the script never runs.
import { world } from "@minecraft/server";

// This top level runs very early, before the world exists. Only subscribe to events here;
// touching players, blocks or entities this early is an error.

// Runs once, after the world has finished loading. console.warn writes to the content log.
world.afterEvents.worldLoad.subscribe(() => {
  console.warn("{{ModName|str}} loaded");
});

// Runs every time a player appears. initialSpawn is true when they join, false on respawn.
world.afterEvents.playerSpawn.subscribe((event) => {
  if (event.initialSpawn) {
    event.player.sendMessage("Hello from {{ModName|str}}!");
  }
});
