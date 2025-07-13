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
    protected string CommandColor = VampireCommandFramework.Color.Command;
    protected string CategoryColor = VampireCommandFramework.Color.Teal;
    protected string PrivColorValid = VampireCommandFramework.Color.Green;
    protected string PrivColorInvalid = VampireCommandFramework.Color.Gold;
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
        ctx.Reply($"Misc castle privileges:\n  <color={PrivColorValid}>{topLevelNames}</color>");

        var categoryNames = string.Join(PrivSeparator, groupedNames.Keys.Where(k => k != "").OrderBy(k => k));
        var sb = new StringBuilder();
        sb.AppendLine($"More privileges can be found in these categories:");
        sb.AppendLine($"  <color={CategoryColor}>{categoryNames}</color>");
        ctx.Reply(sb.ToString());

        sb.Clear();
        sb.AppendLine($"View privileges within a category using:");
        sb.AppendLine($"  <color={CommandColor}>.castlePrivs list <color={CategoryColor}>category</color></color>");
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
        sb.AppendLine($"<color={PrivColorValid}>{firstChunkString}</color>");
        ctx.Reply(sb.ToString());

        for (var i = 1; i < privNameChunks.Count; i++)
        {
            var chunkString = string.Join(PrivSeparator, privNameChunks[i]);
            ctx.Reply($"<color={PrivColorValid}>{chunkString}</color>");
        }
    }

    [Command("reset", description: "Reset castle privilges granted/forbidden to others.")]
    public void CommandReset(ChatCommandContext ctx)
    {
        LogUtil.LogDebug(".castlePrivs reset");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("check", description: "List players to whom you've granted/forbade privileges.")]
    public void CommandCheck(ChatCommandContext ctx)
    {
        LogUtil.LogDebug(".castleprivs check");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("check", description: "Check castle privileges granted to your clan", usage: "clan")]
    public void CommandCheckClan(ChatCommandContext ctx, string targetType)
    {        
        if (!targetType.ToLowerInvariant().Equals("clan"))
        {
            var exampleUsage = ".castlePrivs check clan";
            SendFormatInvalidMessage(ctx, exampleUsage);
            return;
        }
        LogUtil.LogDebug(".castleprivs check clan");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("check", description: "Check castle privileges granted/forbidden to a player", usage: "player Bilbo")]
    public void CommandCheckPlayer(ChatCommandContext ctx, string targetType, string playerName)
    {        
        if (!targetType.ToLowerInvariant().Equals("player"))
        {
            var exampleUsage = ".castlePrivs check player Bilbo";
            SendFormatInvalidMessage(ctx, exampleUsage);
            return;
        }
        LogUtil.LogDebug(".castleprivs check player");
        // todo: implement
        ctx.Reply("Not implemented");
    }

    [Command("grant", description: "Grant castle privileges to your clan.", usage: "clan \"build.all tp.all doors.all\"")]
    public void CommandGrantClan(ChatCommandContext ctx, string targetType, string privileges)
    {        
        if (!targetType.ToLowerInvariant().Equals("clan"))
        {
            var exampleUsage = ".castlePrivs grant clan \"build.all tp.all doors.all\"";
            SendFormatInvalidMessage(ctx, exampleUsage);
            return;
        }

        if (!ParseAndSendValidationMessages(ctx, privileges, out var parseResult))
        {
            return;
        }

        // todo: implement

        var privNamesStr = string.Join(PrivSeparator, parseResult.ValidPrivNames);
        ctx.Reply($"Granted privileges to clan:\n<color={PrivColorValid}>{privNamesStr}</color>");
    }

    [Command("grant", description: "Grant extra castle privileges to a player. Privileges only apply while they are in your clan.", usage: "player Bilbo \"build.all tp.all doors.all\"")]
    public void CommandGrantPlayer(ChatCommandContext ctx, string targetType, string playerName, string privileges)
    {        
        if (!targetType.ToLowerInvariant().Equals("player"))
        {
            var exampleUsage = ".castlePrivs grant player Bilbo \"build.all tp.all doors.all\"";
            SendFormatInvalidMessage(ctx, exampleUsage);
            return;
        }

        if (!ParseAndSendValidationMessages(ctx, privileges, out var parseResult))
        {
            return;
        }

        // todo: implement

        // todo: should unforbid as well as grant

        var privNamesStr = string.Join(PrivSeparator, parseResult.ValidPrivNames);
        ctx.Reply($"Granted privileges to player {playerName}:\n<color={PrivColorValid}>{privNamesStr}</color>");
    }

    [Command("ungrant", description: "Revoke castle privileges granted to your clan.", usage: "clan \"build.all tp.all doors.all\"")]
    public void CommandUnGrantClan(ChatCommandContext ctx, string targetType, string privileges)
    {        
        if (!targetType.ToLowerInvariant().Equals("clan"))
        {
            var exampleUsage = ".castlePrivs ungrant clan \"build.all tp.all doors.all\"";
            SendFormatInvalidMessage(ctx, exampleUsage);
            return;
        }

        if (!ParseAndSendValidationMessages(ctx, privileges, out var parseResult))
        {
            return;
        }

        // todo: implement

        var privNamesStr = string.Join(PrivSeparator, parseResult.ValidPrivNames);
        ctx.Reply($"Revoked privileges for your clan:\n<color={PrivColorValid}>{privNamesStr}</color>");
    }

    [Command("ungrant", description: "Revoke extra castle privileges granted to a player.", usage: "player Bilbo \"build.all tp.all doors.all\"")]
    public void CommandUnGrantPlayer(ChatCommandContext ctx, string targetType, string playerName, string privileges)
    {        
        if (!targetType.ToLowerInvariant().Equals("player"))
        {
            var exampleUsage = ".castlePrivs ungrant player Bilbo \"build.all tp.all doors.all\"";
            SendFormatInvalidMessage(ctx, exampleUsage);
            return;
        }

        if (!ParseAndSendValidationMessages(ctx, privileges, out var parseResult))
        {
            return;
        }

        // todo: implement

        var privNamesStr = string.Join(PrivSeparator, parseResult.ValidPrivNames);
        ctx.Reply($"Revoked extra privileges for player {playerName}:\n<color={PrivColorValid}>{privNamesStr}</color>");
    }

    [Command("forbid", description: "Forbid a player from getting specific castle privileges while in your clan.", usage: "player Gollum \"build.all tp.all doors.all\"")]
    public void CommandForbid(ChatCommandContext ctx, string playerName, string privileges)
    {
        var exampleUsage = ".castlePrivs forbid Gollum \"build.all tp.all doors.all\"";

        if (!ParseAndSendValidationMessages(ctx, privileges, out var parseResult))
        {
            return;
        }

        // todo: implement

        // todo: should ungrant as well as forbid

        var privNamesStr = string.Join(PrivSeparator, parseResult.ValidPrivNames);
        ctx.Reply($"Disqualified player {playerName} from potential clan privileges:\n<color={PrivColorValid}>{privNamesStr}</color>");
    }

    [Command("unforbid", description: "UnForbid a player from getting specific castle privileges while in your clan.", usage: "player Gollum \"build.all tp.all doors.all\"")]
    public void CommandUnForbid(ChatCommandContext ctx, string playerName, string privileges)
    {
        var exampleUsage = ".castlePrivs unforbid Gollum \"build.all tp.all doors.all\"";

        if (!ParseAndSendValidationMessages(ctx, privileges, out var parseResult))
        {
            return;
        }

        // todo: implement

        var privNamesStr = string.Join(PrivSeparator, parseResult.ValidPrivNames);
        ctx.Reply($"Requalified player {playerName} for potential clan privileges:\n<color={PrivColorValid}>{privNamesStr}</color>");
    }

    private bool ParseAndSendValidationMessages(ChatCommandContext ctx, string privileges, out PrivilegeParser.ParseResult parseResult)
    {
        parseResult = PrivilegeParser.Instance.ParsePrivilegesFromCommandString(privileges);
        if (parseResult.InvalidPrivNames.Any())
        {
            ctx.Reply($"Invalid privileges\n<color={PrivColorInvalid}>{string.Join(PrivSeparator, parseResult.InvalidPrivNames)}</color>");
        }

        var isValid = parseResult.ValidPrivNames.Any();
        if (!isValid)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<color=red>No valid privileges specified.</color>");
            sb.Append("To list possible privileges, use ");
            sb.AppendLine($"<color={CommandColor}>.castlePrivs list</color>");
            ctx.Reply(sb.ToString());
        }
        return isValid;
    }

    private void SendFormatInvalidMessage(ChatCommandContext ctx, string exampleCommand)
    {
        ctx.Reply($"<color=red>Invalid format.</color> Example usage:\n<color={CommandColor}>{exampleCommand}</color>");
    }
    
}
