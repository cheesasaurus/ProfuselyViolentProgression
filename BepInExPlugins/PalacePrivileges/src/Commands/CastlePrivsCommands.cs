using System;
using System.Linq;
using System.Text;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using VampireCommandFramework;

namespace ProfuselyViolentProgression.LoadoutLockdown.Commands;

[CommandGroup("castlePrivs")]
public class CastlePrivsCommands
{
    protected string CategoryColor = VampireCommandFramework.Color.Teal;
    protected string PrivColor = VampireCommandFramework.Color.Green;
    protected string PrivSeparator = " <color=grey>|</color> ";
    protected int PrivsPerChunk = 10;

    // todo: remove
    [Command("debug", description: "debug stuff")]
    public void CommandDebug(ChatCommandContext ctx)
    {
        var a = new CastlePrivileges();
        a.Misc |= MiscPrivs.Lockbox;
        a.Arena |= ArenaPrivs.StartContest;
        a.Prisoner |= PrisonerPrivs.Kill | PrisonerPrivs.Subdue;

        var b = new CastlePrivileges();
        b.Misc |= MiscPrivs.Throne;
        b.Prisoner |= PrisonerPrivs.Subdue;

        var c = a | b;
        LogUtil.LogDebug($"{c.Misc} | {c.Arena} | {c.Prisoner}");

        var d = CastlePrivileges.All;
        d &= a;
        LogUtil.LogDebug($"{d.Misc} | {d.Arena} | {d.Prisoner}");

        var e = CastlePrivileges.All;
        LogUtil.LogDebug($"{e.Misc} | {e.Arena} | {e.Prisoner}");


        ctx.Reply("did debug thing");
    }

    [Command("list", description: "List possible castle privileges and categories.")]
    public void CommandList(ChatCommandContext ctx)
    {
        var groupedNames = PrivilegeParser.Instance.PrivilegeNamesGrouped(CastlePrivileges.All);

        var topLevelNames = string.Join(PrivSeparator, groupedNames[""]);
        ctx.Reply($"Misc castle privileges:\n  <color={PrivColor}>{topLevelNames}</color>");

        var categoryNames = string.Join(PrivSeparator, groupedNames.Keys.Where(k => k != "").OrderBy(k => k));
        var sb = new StringBuilder();
        sb.AppendLine($"More privileges can be found in these categories:");
        sb.AppendLine($"  <color={CategoryColor}>{categoryNames}</color>");
        ctx.Reply(sb.ToString());

        sb.Clear();
        sb.AppendLine($"View privileges within a category using:");
        sb.AppendLine($"  <color={VampireCommandFramework.Color.Command}>.castlePrivs list <<color={CategoryColor}>category></color></color>");
        ctx.Reply(sb.ToString());
    }

    [Command("list", description: "List all possible castle privileges in a category.")]
    public void CommandList(ChatCommandContext ctx, string category)
    {
        var groupedNames = PrivilegeParser.Instance.PrivilegeNamesGrouped(CastlePrivileges.All);
        if (!groupedNames.ContainsKey(category))
        {
            ctx.Reply($"<color=red>No privileges found in category <color={CategoryColor}>{category}</color></color>");
            return;
        }

        var count = groupedNames[category].Count;
        var privNameChunks = groupedNames[category].Chunk(PrivsPerChunk).ToList();
        var firstChunkString = string.Join(PrivSeparator, privNameChunks[0]);

        var sb = new StringBuilder();
        sb.AppendLine($"Found <color=#eb0>{count}</color> privileges in category <color={CategoryColor}>{category}</color>:");
        sb.AppendLine($"<color={PrivColor}>{firstChunkString}</color>");
        ctx.Reply(sb.ToString());

        for (var i = 1; i < privNameChunks.Count; i++)
        {
            var chunkString = string.Join(PrivSeparator, privNameChunks[i]);
            ctx.Reply($"<color={PrivColor}>{chunkString}</color>");
        }
    }

    [Command("reset", description: "Reset castle privilges granted/forbidden to others.")]
    public void CommandReset(ChatCommandContext ctx)
    {
        LogUtil.LogDebug(".castlePrivs reset");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("check", description: "Check castle privileges granted/forbidden to others.")]
    public void CommandCheck(ChatCommandContext ctx)
    {
        LogUtil.LogDebug(".castleprivs check");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("check", description: "Check castle privileges granted to your clan", usage: "clan")]
    public void CommandCheckClan(ChatCommandContext ctx, string targetType)
    {
        var exampleUsage = ".castlePrivs check clan";
        if (!targetType.ToLowerInvariant().Equals("clan"))
        {
            ctx.Reply($"invalid format. example usage: \"{exampleUsage}\"");
            return;
        }
        LogUtil.LogDebug(".castleprivs check clan");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("check", description: "Check castle privileges granted/forbidden to a player", usage: "player Bilbo")]
    public void CommandCheckPlayer(ChatCommandContext ctx, string targetType, string playerName)
    {
        var exampleUsage = ".castlePrivs check player Bilbo";
        if (!targetType.ToLowerInvariant().Equals("player"))
        {
            ctx.Reply($"invalid format. example usage: \"{exampleUsage}\"");
            return;
        }
        LogUtil.LogDebug(".castleprivs grant player");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("grant", description: "Grant castle privileges to your clan.", usage: "clan \"build.all tp.all doors.all\"")]
    public void CommandGrantClan(ChatCommandContext ctx, string targetType, string privileges)
    {
        var exampleUsage = ".castlePrivs grant clan \"build.all tp.all doors.all\"";
        if (!targetType.ToLowerInvariant().Equals("clan"))
        {
            ctx.Reply($"invalid format. example usage: \"{exampleUsage}\"");
            return;
        }
        LogUtil.LogDebug(".castleprivs grant clan");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("grant", description: "Grant castle privileges to a player. Privileges only apply while they are in your clan.", usage: "player Bilbo \"build.all tp.all doors.all\"")]
    public void CommandGrantPlayer(ChatCommandContext ctx, string targetType, string playerName, string privileges)
    {
        var exampleUsage = ".castlePrivs grant player Bilbo \"build.all tp.all doors.all\"";
        if (!targetType.ToLowerInvariant().Equals("player"))
        {
            ctx.Reply($"invalid format. example usage: \"{exampleUsage}\"");
            return;
        }
        LogUtil.LogDebug(".castleprivs grant player");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("ungrant", description: "Revoke castle privileges granted to your clan.", usage: "clan \"build.all tp.all doors.all\"")]
    public void CommandUnGrantClan(ChatCommandContext ctx, string targetType, string privileges)
    {
        var exampleUsage = ".castlePrivs ungrant clan \"build.all tp.all doors.all\"";
        if (!targetType.ToLowerInvariant().Equals("clan"))
        {
            ctx.Reply($"invalid format. example usage: \"{exampleUsage}\"");
            return;
        }
        LogUtil.LogDebug(".castleprivs ungrant clan");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("ungrant", description: "Revoke castle privileges granted to a player.", usage: "player Bilbo \"build.all tp.all doors.all\"")]
    public void CommandUnGrantPlayer(ChatCommandContext ctx, string targetType, string playerName, string privileges)
    {
        var exampleUsage = ".castlePrivs ungrant player Bilbo \"build.all tp.all doors.all\"";
        if (!targetType.ToLowerInvariant().Equals("player"))
        {
            ctx.Reply($"invalid format. example usage: \"{exampleUsage}\"");
            return;
        }
        LogUtil.LogDebug(".castleprivs ungrant player");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("forbid", description: "Forbid a player from getting specific castle privileges while in your clan.", usage: "player Gollum \"build.all tp.all doors.all\"")]
    public void CommandForbid(ChatCommandContext ctx, string playerName, string privileges)
    {
        var exampleUsage = ".castlePrivs forbid Gollum \"build.all tp.all doors.all\"";

        LogUtil.LogDebug(".castleprivs forbid");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("unforbid", description: "UnForbid a player from getting specific castle privileges while in your clan.", usage: "player Gollum \"build.all tp.all doors.all\"")]
    public void CommandUnForbid(ChatCommandContext ctx, string playerName, string privileges)
    {
        var exampleUsage = ".castlePrivs unforbid Gollum \"build.all tp.all doors.all\"";

        LogUtil.LogDebug(".castleprivs unforbid");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    // todo: more commands
}
