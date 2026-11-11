package {{Package}};

import io.papermc.paper.command.brigadier.BasicCommand;
import io.papermc.paper.command.brigadier.CommandSourceStack;

// /{{CommandName}}: the simplest kind of command, which gets its arguments as plain strings.
public final class HelloCommand implements BasicCommand {
    @Override
    public void execute(CommandSourceStack source, String[] args) {
        // The sender is a player, the console or a command block.
        source.getSender().sendPlainMessage("Hello from {{ModName|str}}!");
    }

    @Override
    public String permission() {
        // Players need this permission to see and run the command. Operators have it.
        return "{{CommandName}}.hello";
    }
}
