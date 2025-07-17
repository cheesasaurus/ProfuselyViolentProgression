using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using VampireCommandFramework;

namespace ProfuselyViolentProgression.PalacePrivileges.Commands;

[CommandGroup("castlePrivs")]
public class CastlePrivsCommands
{
    protected string ColorGold = VampireCommandFramework.Color.Gold;
    protected string CommandColor = VampireCommandFramework.Color.Command;
    protected string CategoryColor = VampireCommandFramework.Color.Teal;
    protected string PrivColorValid = VampireCommandFramework.Color.Green;
    protected string PrivColorInvalid = VampireCommandFramework.Color.Gold;
    protected string PrivColorForbidden = VampireCommandFramework.Color.Red;
    protected string PrivSeparator = " <color=grey>|</color> ";
    protected int PrivsPerChunk = 10;

    // todo: remove
    [Command("debug", description: "debug stuff")]
    public unsafe void CommandDebug(ChatCommandContext ctx)
    {
        ctx.Reply("A");
    }

    [Command("list", description: "List possible castle privileges and categories.")]
    public void CommandList(ChatCommandContext ctx)
    {
        var groupedNames = Core.PrivilegeParser.PrivilegeNamesGrouped(CastlePrivileges.All);

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
        var groupedNames = Core.PrivilegeParser.PrivilegeNamesGrouped(CastlePrivileges.All);
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
        Core.CastlePrivilegesService.ResetPlayerSettings(ctx.User.PlatformId);
        ctx.Reply("Reset all privileges which were granted/forbidden to others.");
    }

    [Command("check", description: "List players to whom you've granted/forbade privileges.")]
    public void CommandCheck(ChatCommandContext ctx)
    {
        var playerNames = Core.CastlePrivilegesService.NamesOfPlayersWithCustomPrivs(ctx.User.PlatformId);
        if (!playerNames.Any())
        {
            ctx.Reply($"You haven't granted/forbade privileges to any specific players.");
            return;
        }

        ctx.Reply($"You've set custom privileges for <color={ColorGold}>{playerNames.Count()}</color> players");

        var chunks = playerNames.Chunk(PrivsPerChunk).ToList();
        for (var i = 0; i < chunks.Count; i++)
        {
            var chunkString = string.Join(PrivSeparator, chunks[i]);
            ctx.Reply($"<color={ColorGold}>{chunkString}</color>");
        }
    }

    [Command("check", description: "Check castle privileges granted to your clan", usage: "clan")]
    public void CommandCheckClan(ChatCommandContext ctx, string targetType)
    {
        if (!targetType.ToLowerInvariant().Equals("clan"))
        {
            // most likely the user meant to check a player
            var playerName = targetType;
            CommandCheckPlayer(ctx, "player", playerName);
            return;
        }

        var clanPrivs = Core.CastlePrivilegesService.PrivilegesForClan(ctx.User.PlatformId);
        var privNames = Core.PrivilegeParser.PrivilegeNames(clanPrivs);

        ctx.Reply($"Privileges for clan members:");
        SendMessages_Privileges(ctx, privNames);
    }

    [Command("check", description: "Check castle privileges granted/forbidden to a player", usage: "player Bilbo")]
    public void CommandCheckPlayer(ChatCommandContext ctx, string targetType, string playerName)
    {
        if (!targetType.ToLowerInvariant().Equals("player"))
        {
            var exampleUsage = ".castlePrivs check player Bilbo";
            SendMessage_FormatInvalid(ctx, exampleUsage);
            return;
        }

        if (!CheckNotReferringToSelf_SendValidationMessages(ctx, playerName))
        {
            return;
        }

        if (!TryFindUserAndSendValidationMessages(ctx, playerName, out var userModel))
        {
            return;
        }

        var playerNameFormatted = $"<color={ColorGold}>{userModel.User.CharacterName}</color>";

        if (!Core.CastlePrivilegesService.TryGetCustomPrivilegesForActingPlayer(ctx.User.PlatformId, userModel.User.PlatformId, out var actingPlayerPrivileges))
        {
            ctx.Reply($"No custom privileges granted/forbidden to player {playerNameFormatted}");
            return;
        }

        var grantedPrivNames = Core.PrivilegeParser.PrivilegeNames(actingPlayerPrivileges.Granted);
        var forbiddenPrivNames = Core.PrivilegeParser.PrivilegeNames(actingPlayerPrivileges.Forbidden);

        if (!grantedPrivNames.Any() && !forbiddenPrivNames.Any())
        {
            ctx.Reply($"No custom privileges granted/forbidden to player {playerNameFormatted}");
            return;
        }

        if (grantedPrivNames.Any())
        {
            ctx.Reply($"Privileges <color=green>granted</color> for player {playerNameFormatted}:");
            SendMessages_Privileges(ctx, grantedPrivNames);
        }
        
        if (forbiddenPrivNames.Any())
        {
            ctx.Reply($"Privileges <color={PrivColorForbidden}>forbidden</color> for player {playerNameFormatted}:");
            SendMessages_Privileges(ctx, forbiddenPrivNames, PrivColorForbidden);
        }
    }

    [Command("grant", description: "Grant castle privileges to your clan.", usage: "clan \"build.all tp.all doors.all\"")]
    public void CommandGrantClan(ChatCommandContext ctx, string targetType, string privileges)
    {
        if (!targetType.ToLowerInvariant().Equals("clan"))
        {
            // most likely the user meant to grant to a player
            var playerName = targetType;
            CommandGrantPlayer(ctx, "player", playerName, privileges);
            return;
        }

        if (!ParseAndSendValidationMessages(ctx, privileges, out var parseResult))
        {
            return;
        }

        Core.CastlePrivilegesService.GrantClanPrivileges(ctx.User.PlatformId, parseResult.Privs);

        var privNamesStr = string.Join(PrivSeparator, parseResult.ValidPrivNames);
        ctx.Reply($"Granted privileges to clan:\n<color={PrivColorValid}>{privNamesStr}</color>");
    }

    [Command("grant", description: "Grant extra castle privileges to a player. Privileges only apply while they are in your clan.", usage: "player Bilbo \"build.all tp.all doors.all\"")]
    public void CommandGrantPlayer(ChatCommandContext ctx, string targetType, string playerName, string privileges)
    {
        if (!targetType.ToLowerInvariant().Equals("player"))
        {
            var exampleUsage = ".castlePrivs grant player Bilbo \"build.all tp.all doors.all\"";
            SendMessage_FormatInvalid(ctx, exampleUsage);
            return;
        }

        if (!CheckNotReferringToSelf_SendValidationMessages(ctx, playerName))
        {
            return;
        }

        if (!ParseAndSendValidationMessages(ctx, privileges, out var parseResult))
        {
            return;
        }

        if (!TryFindUserAndSendValidationMessages(ctx, playerName, out var userModel))
        {
            return;
        }

        Core.CastlePrivilegesService.GrantPlayerPrivileges(ctx.User.PlatformId, userModel.User.PlatformId, parseResult.Privs);

        ctx.Reply($"Granted extra privileges to player <color={ColorGold}>{playerName}:</color>");
        SendMessages_Privileges(ctx, parseResult.ValidPrivNames);
    }

    [Command("ungrant", description: "Revoke castle privileges granted to your clan.", usage: "clan \"build.all tp.all doors.all\"")]
    public void CommandUnGrantClan(ChatCommandContext ctx, string targetType, string privileges)
    {
        if (!targetType.ToLowerInvariant().Equals("clan"))
        {
            // most likely the user meant to ungrant to a player
            var playerName = targetType;
            CommandUnGrantPlayer(ctx, "player", playerName, privileges);
            return;
        }

        if (!ParseAndSendValidationMessages(ctx, privileges, out var parseResult))
        {
            return;
        }

        Core.CastlePrivilegesService.UnGrantClanPrivileges(ctx.User.PlatformId, parseResult.Privs);

        var privNamesStr = string.Join(PrivSeparator, parseResult.ValidPrivNames);
        ctx.Reply($"Revoked privileges for your clan:\n<color={PrivColorValid}>{privNamesStr}</color>");
    }

    [Command("ungrant", description: "Revoke extra castle privileges granted to a player.", usage: "player Bilbo \"build.all tp.all doors.all\"")]
    public void CommandUnGrantPlayer(ChatCommandContext ctx, string targetType, string playerName, string privileges)
    {
        if (!targetType.ToLowerInvariant().Equals("player"))
        {
            var exampleUsage = ".castlePrivs ungrant player Bilbo \"build.all tp.all doors.all\"";
            SendMessage_FormatInvalid(ctx, exampleUsage);
            return;
        }

        if (!CheckNotReferringToSelf_SendValidationMessages(ctx, playerName))
        {
            return;
        }

        if (!ParseAndSendValidationMessages(ctx, privileges, out var parseResult))
        {
            return;
        }

        if (!TryFindUserAndSendValidationMessages(ctx, playerName, out var userModel))
        {
            return;
        }

        Core.CastlePrivilegesService.UnGrantPlayerPrivileges(ctx.User.PlatformId, userModel.User.PlatformId, parseResult.Privs);
        
        ctx.Reply($"Revoked extra privileges for player <color={ColorGold}>{playerName}:</color>");
        SendMessages_Privileges(ctx, parseResult.ValidPrivNames);
    }

    /// <summary>
    /// This particular version of "forbid" is for convenience / consistency.
    /// During development, I got so used to typing "player" for the other commands,
    /// that I kept typing it for "forbid" too, even though it's for players only.
    /// I'm sure it would happen to somebody else as well.
    /// </summary>
    [Command("forbid", description: "Forbid a player from getting specific castle privileges while in your clan.", usage: "player Gollum \"build.all tp.all doors.all\"")]
    public void CommandForbid(ChatCommandContext ctx, string target, string playerName, string privileges)
    {
        switch (target.ToLowerInvariant())
        {
            case "clan":
                ctx.Reply($"<color=red>\"forbid\" is only for players</color>");
                break;

            case "player":
                CommandForbid(ctx, playerName, privileges);
                break;

            default:
                var exampleUsage = ".castlePrivs forbid player Gollum \"build.all tp.all doors.all\"";
                SendMessage_FormatInvalid(ctx, exampleUsage);
                break;
        }
    }

    [Command("forbid", description: "Forbid a player from getting specific castle privileges while in your clan.", usage: "Gollum \"build.all tp.all doors.all\"")]
    public void CommandForbid(ChatCommandContext ctx, string playerName, string privileges)
    {
        if (!CheckNotReferringToSelf_SendValidationMessages(ctx, playerName))
        {
            return;
        }

        if (!ParseAndSendValidationMessages(ctx, privileges, out var parseResult))
        {
            return;
        }

        if (!TryFindUserAndSendValidationMessages(ctx, playerName, out var userModel))
        {
            return;
        }

        Core.CastlePrivilegesService.ForbidPlayerPrivileges(ctx.User.PlatformId, userModel.User.PlatformId, parseResult.Privs);

        var privNamesStr = string.Join(PrivSeparator, parseResult.ValidPrivNames);
        ctx.Reply($"Disqualified player <color={ColorGold}>{playerName}</color> from potential clan privileges:\n<color={PrivColorForbidden}>{privNamesStr}</color>");
    }

    /// <summary>
    /// This particular version of "unforbid" is for convenience / consistency.
    /// During development, I got so used to typing "player" for the other commands,
    /// that I kept typing it for "unforbid" too, even though it's for players only.
    /// I'm sure it would happen to somebody else as well.
    /// </summary>
    [Command("unforbid", description: "UnForbid a player from getting specific castle privileges while in your clan.", usage: "player Gollum \"build.all tp.all doors.all\"")]
    public void CommandUnForbid(ChatCommandContext ctx, string target, string playerName, string privileges)
    {
        switch (target.ToLowerInvariant())
        {
            case "clan":
                ctx.Reply($"<color=red>\"unforbid\" is only for players</color>");
                break;

            case "player":
                CommandUnForbid(ctx, playerName, privileges);
                break;

            default:
                var exampleUsage = ".castlePrivs unforbid player Gollum \"build.all tp.all doors.all\"";
                SendMessage_FormatInvalid(ctx, exampleUsage);
                break;
        }
    }

    [Command("unforbid", description: "UnForbid a player from getting specific castle privileges while in your clan.", usage: "Gollum \"build.all tp.all doors.all\"")]
    public void CommandUnForbid(ChatCommandContext ctx, string playerName, string privileges)
    {
        if (!CheckNotReferringToSelf_SendValidationMessages(ctx, playerName))
        {
            return;
        }

        if (!ParseAndSendValidationMessages(ctx, privileges, out var parseResult))
        {
            return;
        }

        if (!TryFindUserAndSendValidationMessages(ctx, playerName, out var userModel))
        {
            return;
        }

        Core.CastlePrivilegesService.UnForbidPlayerPrivileges(ctx.User.PlatformId, userModel.User.PlatformId, parseResult.Privs);

        var privNamesStr = string.Join(PrivSeparator, parseResult.ValidPrivNames);
        ctx.Reply($"Requalified player {playerName} for potential clan privileges:\n<color={PrivColorValid}>{privNamesStr}</color>");
    }

    [Command("settings", description: "Check global settings.")]
    public void CommandSettings(ChatCommandContext ctx)
    {
        var globalSettings = Core.GlobalSettingsService.GetGlobalSettings();
        var keyClanCooldownHours = Math.Round(globalSettings.KeyClanCooldownHours, 2);

        var sb = new StringBuilder();
        sb.AppendLine($"Global settings:<color={ColorGold}>");
        sb.AppendLine($"  KeyClanCooldownHours: <color={CommandColor}>{keyClanCooldownHours}</color>");
        if (ctx.IsAdmin)
        {
            sb.AppendLine($"  DebugLogRulings: <color={CommandColor}>{globalSettings.DebugLogRulings}</color>");
        }
        sb.Append("</color>");
        ctx.Reply(sb.ToString());
    }

    [Command("set", description: "Set a global setting", adminOnly: true)]
    public void CommandSettingsSet(ChatCommandContext ctx, string settingName, string value)
    {
        switch (settingName.ToLowerInvariant())
        {
            case "keyclancooldownhours":
            case "keyclancooldown":
                SubCommand_SettingsSet_KeyClanCooldownHours(ctx, value);
                break;

            case "debuglogrulings":
            case "logrulings":
                SubCommand_SettingsSet_DebugLogRulings(ctx, value);
                break;

            default:
                var invalidSettingName = $"<color={ColorGold}>\"{settingName}\"</color>";
                ctx.Reply($"<color=red>unknown setting {invalidSettingName}</color>");
                break;
        }
    }

    private void SubCommand_SettingsSet_KeyClanCooldownHours(ChatCommandContext ctx, string value)
    {
        if (!float.TryParse(value, out var hours))
        {
            ctx.Reply($"<color=red>\"{value}\" doesn't look like a number. Expected format like <color={CommandColor}>48.0</color>");
            return;
        }
        Core.GlobalSettingsService.SetGlobalSetting_KeyClanCooldownHours(hours);
        SendMessage_SettingsSet_Success(ctx, "KeyClanCooldownHours", Math.Round(hours, 2).ToString());
    }

    private void SubCommand_SettingsSet_DebugLogRulings(ChatCommandContext ctx, string value)
    {
        if (!bool.TryParse(value, out var enabled))
        {
            var sb = new StringBuilder();
            sb.Append($"<color=red>");
            sb.Append($"\"{value}\" invalid. <color={ColorGold}>DebugLogRulings</color> can be either ");
            sb.Append($"<color={CommandColor}>True</color> or <color={CommandColor}>False</color>");
            sb.Append($"</color>");
            ctx.Reply(sb.ToString());
            return;
        }
        Core.GlobalSettingsService.SetGlobalSetting_DebugLogRulings(enabled);
        SendMessage_SettingsSet_Success(ctx, "DebugLogRulings", enabled.ToString());
    }

    private bool ParseAndSendValidationMessages(ChatCommandContext ctx, string privileges, out PrivilegeParser.ParseResult parseResult)
    {
        parseResult = Core.PrivilegeParser.ParsePrivilegesFromCommandString(privileges);
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

    private bool TryFindUserAndSendValidationMessages(ChatCommandContext ctx, string userName, out UserUtil.UserModel userModel)
    {
        if (UserUtil.TryFindUserByName(userName, out userModel))
        {
            return true;
        }
        ctx.Reply($"<color=red>Could not find any player named <color={ColorGold}>\"{userName}\"</color></color>");
        return false;
    }

    private void SendMessage_FormatInvalid(ChatCommandContext ctx, string exampleCommand)
    {
        ctx.Reply($"<color=red>Invalid format.</color> Example usage:\n<color={CommandColor}>{exampleCommand}</color>");
    }

    private void SendMessages_Privileges(ChatCommandContext ctx, List<string> privNames)
    {
        SendMessages_Privileges(ctx, privNames, PrivColorValid);
    }

    private void SendMessages_Privileges(ChatCommandContext ctx, List<string> privNames, string color)
    {
        var privNamesChunks = privNames.Chunk(PrivsPerChunk).ToList();
        for (var i = 0; i < privNamesChunks.Count; i++)
        {
            var chunkString = string.Join(PrivSeparator, privNamesChunks[i]);
            ctx.Reply($"<color={color}>{chunkString}</color>");
        }
    }

    private void SendMessage_SettingsSet_Success(ChatCommandContext ctx, string settingName, string value)
    {
        ctx.Reply($"Global setting <color={ColorGold}>{settingName}</color> set to <color={CommandColor}>{value}</color>");
    }

    private bool CheckNotReferringToSelf_SendValidationMessages(ChatCommandContext ctx, string userName)
    {
        if (IsSelfReferential(ctx, userName))
        {
            ctx.Reply($"Hey <color={ColorGold}>{ctx.Name}</color>, you always have full privileges for your own castle!");
            return false;
        }
        return true;
    }
    
    private bool IsSelfReferential(ChatCommandContext ctx, string userName)
    {
        return ctx.Name.ToLowerInvariant().Equals(userName.ToLowerInvariant());
    }

}
