using VampireCommandFramework;

namespace ProfuselyViolentProgression.LoadoutLockdown;

public class UnEquipForbiddenItemsCommand
{
    [Command("UnEquipForbiddenGear", shortHand: "uefi", description: "unequip forbidden gear from all players", adminOnly: true)]
    public void Execute(ChatCommandContext ctx) {
        ctx.Reply("unequipping forbidden gear from all players...");
        LoadoutLockdownService.Instance?.UnEquipForbiddenItemsFromAllPlayerCharacters();
        ctx.Reply("finished unequipping");
    }
}
