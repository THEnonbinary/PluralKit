using Humanizer;

using PluralKit.Core;

namespace PluralKit.Bot;

public partial class CommandTree
{
    public Task ExecuteCommand(Context ctx)
    {
        if (ctx.Match("system", "s"))
            return HandleSystemCommand(ctx);
        if (ctx.Match("member", "m"))
            return HandleMemberCommand(ctx);
        if (ctx.Match("group", "g"))
            return HandleGroupCommand(ctx);
        if (ctx.Match("switch", "sw"))
            return HandleSwitchCommand(ctx);
        if (ctx.Match("commands", "cmd", "c"))
            return CommandHelpRoot(ctx);
        if (ctx.Match("ap", "autoproxy", "auto"))
            return HandleAutoproxyCommand(ctx);
        if (ctx.Match("config", "cfg"))
            return HandleConfigCommand(ctx);
        if (ctx.Match("list", "find", "members", "search", "query", "l", "f", "fd"))
            return ctx.Execute<SystemList>(SystemList, m => m.MemberList(ctx, ctx.System));
        if (ctx.Match("link"))
            return ctx.Execute<SystemLink>(Link, m => m.LinkSystem(ctx));
        if (ctx.Match("unlink"))
            return ctx.Execute<SystemLink>(Unlink, m => m.UnlinkAccount(ctx));
        if (ctx.Match("token"))
            if (ctx.Match("refresh", "renew", "invalidate", "reroll", "regen"))
                return ctx.Execute<Api>(TokenRefresh, m => m.RefreshToken(ctx));
            else
                return ctx.Execute<Api>(TokenGet, m => m.GetToken(ctx));
        if (ctx.Match("import"))
            return ctx.Execute<ImportExport>(Import, m => m.Import(ctx));
        if (ctx.Match("export"))
            return ctx.Execute<ImportExport>(Export, m => m.Export(ctx));
        if (ctx.Match("help"))
            if (ctx.Match("commands"))
                return ctx.Reply("For the list of commands, see the website: <https://pluralkit.me/commands>");
            else if (ctx.Match("proxy"))
                return ctx.Reply(
                    "The proxy help page has been moved! See the website: https://pluralkit.me/guide#proxying");
            else return ctx.Execute<Help>(Help, m => m.HelpRoot(ctx));
        if (ctx.Match("explain"))
            return ctx.Execute<Help>(Explain, m => m.Explain(ctx));
        if (ctx.Match("message", "msg"))
            return ctx.Execute<ProxiedMessage>(Message, m => m.GetMessage(ctx));
        if (ctx.Match("edit", "e"))
            return ctx.Execute<ProxiedMessage>(MessageEdit, m => m.EditMessage(ctx));
        if (ctx.Match("log"))
            if (ctx.Match("channel"))
                return ctx.Execute<ServerConfig>(LogChannel, m => m.SetLogChannel(ctx));
            else if (ctx.Match("enable", "on"))
                return ctx.Execute<ServerConfig>(LogEnable, m => m.SetLogEnabled(ctx, true));
            else if (ctx.Match("disable", "off"))
                return ctx.Execute<ServerConfig>(LogDisable, m => m.SetLogEnabled(ctx, false));
            else if (ctx.Match("commands"))
                return PrintCommandList(ctx, "message logging", LogCommands);
            else return PrintCommandExpectedError(ctx, LogCommands);
        if (ctx.Match("logclean"))
            return ctx.Execute<ServerConfig>(LogClean, m => m.SetLogCleanup(ctx));
        if (ctx.Match("blacklist", "bl"))
            if (ctx.Match("enable", "on", "add", "deny"))
                return ctx.Execute<ServerConfig>(BlacklistAdd, m => m.SetBlacklisted(ctx, true));
            else if (ctx.Match("disable", "off", "remove", "allow"))
                return ctx.Execute<ServerConfig>(BlacklistRemove, m => m.SetBlacklisted(ctx, false));
            else if (ctx.Match("list", "show"))
                return ctx.Execute<ServerConfig>(BlacklistShow, m => m.ShowBlacklisted(ctx));
            else if (ctx.Match("commands"))
                return PrintCommandList(ctx, "channel blacklisting", BlacklistCommands);
            else return PrintCommandExpectedError(ctx, BlacklistCommands);
        if (ctx.Match("proxy"))
            if (ctx.Match("debug"))
                return ctx.Execute<Checks>(ProxyCheck, m => m.MessageProxyCheck(ctx));
            else
                return ctx.Execute<SystemEdit>(SystemProxy, m => m.SystemProxy(ctx));
        if (ctx.Match("invite")) return ctx.Execute<Misc>(Invite, m => m.Invite(ctx));
        if (ctx.Match("mn")) return ctx.Execute<Fun>(null, m => m.Mn(ctx));
        if (ctx.Match("fire")) return ctx.Execute<Fun>(null, m => m.Fire(ctx));
        if (ctx.Match("thunder")) return ctx.Execute<Fun>(null, m => m.Thunder(ctx));
        if (ctx.Match("freeze")) return ctx.Execute<Fun>(null, m => m.Freeze(ctx));
        if (ctx.Match("starstorm")) return ctx.Execute<Fun>(null, m => m.Starstorm(ctx));
        if (ctx.Match("flash")) return ctx.Execute<Fun>(null, m => m.Flash(ctx));
        if (ctx.Match("error")) return ctx.Execute<Fun>(null, m => m.Error(ctx));
        if (ctx.Match("stats")) return ctx.Execute<Misc>(null, m => m.Stats(ctx));
        if (ctx.Match("permcheck"))
            return ctx.Execute<Checks>(PermCheck, m => m.PermCheckGuild(ctx));
        if (ctx.Match("proxycheck"))
            return ctx.Execute<Checks>(ProxyCheck, m => m.MessageProxyCheck(ctx));
        if (ctx.Match("debug"))
            return HandleDebugCommand(ctx);
        if (ctx.Match("admin"))
            return HandleAdminCommand(ctx);
        if (ctx.Match("random", "r"))
            if (ctx.Match("group", "g") || ctx.MatchFlag("group", "g"))
                return ctx.Execute<Random>(GroupRandom, r => r.Group(ctx));
            else
                return ctx.Execute<Random>(MemberRandom, m => m.Member(ctx));

        // remove compiler warning
        return ctx.Reply(
            $"{Emojis.Error} Unknown command {ctx.PeekArgument().AsCode()}. For a list of possible commands, see <https://pluralkit.me/commands>.");
    }

    private async Task HandleAdminCommand(Context ctx)
    {
        if (ctx.Match("usid", "updatesystemid"))
            await ctx.Execute<Admin>(Admin, a => a.UpdateSystemId(ctx));
        else if (ctx.Match("umid", "updatememberid"))
            await ctx.Execute<Admin>(Admin, a => a.UpdateMemberId(ctx));
        else if (ctx.Match("ugid", "updategroupid"))
            await ctx.Execute<Admin>(Admin, a => a.UpdateGroupId(ctx));
        else if (ctx.Match("uml", "updatememberlimit"))
            await ctx.Execute<Admin>(Admin, a => a.SystemMemberLimit(ctx));
        else if (ctx.Match("ugl", "updategrouplimit"))
            await ctx.Execute<Admin>(Admin, a => a.SystemGroupLimit(ctx));
        else
            await ctx.Reply($"{Emojis.Error} Unknown command.");
    }

    private async Task HandleDebugCommand(Context ctx)
    {
        var availableCommandsStr = "Available debug targets: `permissions`, `proxying`";

        if (ctx.Match("permissions", "perms", "permcheck"))
            if (ctx.Match("channel", "ch"))
                await ctx.Execute<Checks>(PermCheck, m => m.PermCheckChannel(ctx));
            else
                await ctx.Execute<Checks>(PermCheck, m => m.PermCheckGuild(ctx));
        else if (ctx.Match("channel"))
            await ctx.Execute<Checks>(PermCheck, m => m.PermCheckChannel(ctx));
        else if (ctx.Match("proxy", "proxying", "proxycheck"))
            await ctx.Execute<Checks>(ProxyCheck, m => m.MessageProxyCheck(ctx));
        else if (!ctx.HasNext())
            await ctx.Reply($"{Emojis.Error} You need to pass a command. {availableCommandsStr}");
        else
            await ctx.Reply(
                $"{Emojis.Error} Unknown debug command {ctx.PeekArgument().AsCode()}. {availableCommandsStr}");
    }

    private async Task HandleSystemCommand(Context ctx)
    {
        // If we have no parameters, default to self-target
        if (!ctx.HasNext())
            await ctx.Execute<System>(SystemInfo, m => m.Query(ctx, ctx.System));

        // First, we match own-system-only commands (ie. no target system parameter)
        else if (ctx.Match("new", "create", "make", "add", "register", "init", "n"))
            await ctx.Execute<System>(SystemNew, m => m.New(ctx));
        else if (ctx.Match("name", "rename", "changename"))
            await ctx.Execute<SystemEdit>(SystemRename, m => m.Name(ctx));
        else if (ctx.Match("tag"))
            await ctx.Execute<SystemEdit>(SystemTag, m => m.Tag(ctx));
        else if (ctx.Match("servertag"))
            await ctx.Execute<SystemEdit>(SystemServerTag, m => m.ServerTag(ctx));
        else if (ctx.Match("description", "desc", "bio"))
            await ctx.Execute<SystemEdit>(SystemDesc, m => m.Description(ctx));
        else if (ctx.Match("color", "colour"))
            await ctx.Execute<SystemEdit>(SystemColor, m => m.Color(ctx));
        else if (ctx.Match("banner", "splash", "cover"))
            await ctx.Execute<SystemEdit>(SystemBannerImage, m => m.BannerImage(ctx));
        else if (ctx.Match("avatar", "picture", "icon", "image", "pic", "pfp"))
            await ctx.Execute<SystemEdit>(SystemAvatar, m => m.Avatar(ctx));
        else if (ctx.Match("delete", "remove", "destroy", "erase", "yeet"))
            await ctx.Execute<SystemEdit>(SystemDelete, m => m.Delete(ctx));
        else if (ctx.Match("webhook", "hook"))
            await ctx.Execute<Api>(null, m => m.SystemWebhook(ctx));
        else if (ctx.Match("timezone", "tz"))
            await ctx.Execute<Config>(ConfigTimezone, m => m.SystemTimezone(ctx), true);
        else if (ctx.Match("proxy"))
            await ctx.Execute<SystemEdit>(SystemProxy, m => m.SystemProxy(ctx));
        else if (ctx.Match("list", "l", "members"))
            await ctx.Execute<SystemList>(SystemList, m => m.MemberList(ctx, ctx.System));
        else if (ctx.Match("find", "search", "query", "fd", "s"))
            await ctx.Execute<SystemList>(SystemFind, m => m.MemberList(ctx, ctx.System));
        else if (ctx.Match("f", "front", "fronter", "fronters"))
        {
            if (ctx.Match("h", "history"))
                await ctx.Execute<SystemFront>(SystemFrontHistory, m => m.SystemFrontHistory(ctx, ctx.System));
            else if (ctx.Match("p", "percent", "%"))
                await ctx.Execute<SystemFront>(SystemFrontPercent, m => m.SystemFrontPercent(ctx, ctx.System));
            else
                await ctx.Execute<SystemFront>(SystemFronter, m => m.SystemFronter(ctx, ctx.System));
        }
        else if (ctx.Match("fh", "fronthistory", "history", "switches"))
            await ctx.Execute<SystemFront>(SystemFrontHistory, m => m.SystemFrontHistory(ctx, ctx.System));
        else if (ctx.Match("fp", "frontpercent", "front%", "frontbreakdown"))
            await ctx.Execute<SystemFront>(SystemFrontPercent, m => m.SystemFrontPercent(ctx, ctx.System));
        else if (ctx.Match("privacy"))
            await ctx.Execute<SystemEdit>(SystemPrivacy, m => m.SystemPrivacy(ctx));
        else if (ctx.Match("ping"))
            await ctx.Execute<Config>(ConfigPing, m => m.SystemPing(ctx), true);
        else if (ctx.Match("commands", "help"))
            await PrintCommandList(ctx, "systems", SystemCommands);
        else if (ctx.Match("groups", "gs", "g"))
            await ctx.Execute<Groups>(GroupList, g => g.ListSystemGroups(ctx, null));
        else
            await HandleSystemCommandTargeted(ctx);
    }

    private async Task HandleSystemCommandTargeted(Context ctx)
    {
        // Commands that have a system target (eg. pk;system <system> fronthistory)
        var target = await ctx.MatchSystem();
        if (target == null)
        {
            var list = CreatePotentialCommandList(SystemInfo, SystemNew, SystemRename, SystemTag, SystemDesc,
                SystemAvatar, SystemDelete, SystemList, SystemFronter, SystemFrontHistory, SystemFrontPercent);
            await ctx.Reply(
                $"{Emojis.Error} {await CreateSystemNotFoundError(ctx)}\n\nPerhaps you meant to use one of the following commands?\n{list}");
        }
        else if (ctx.Match("avatar", "picture", "icon", "image", "pic", "pfp"))
            await ctx.Execute<SystemEdit>(SystemAvatar, m => m.Avatar(ctx, target));
        else if (ctx.Match("list", "l", "members"))
            await ctx.Execute<SystemList>(SystemList, m => m.MemberList(ctx, target));
        else if (ctx.Match("find", "search", "query", "fd", "s"))
            await ctx.Execute<SystemList>(SystemFind, m => m.MemberList(ctx, target));
        else if (ctx.Match("f", "front", "fronter", "fronters"))
        {
            if (ctx.Match("h", "history"))
                await ctx.Execute<SystemFront>(SystemFrontHistory, m => m.SystemFrontHistory(ctx, target));
            else if (ctx.Match("p", "percent", "%"))
                await ctx.Execute<SystemFront>(SystemFrontPercent, m => m.SystemFrontPercent(ctx, target));
            else
                await ctx.Execute<SystemFront>(SystemFronter, m => m.SystemFronter(ctx, target));
        }
        else if (ctx.Match("fh", "fronthistory", "history", "switches"))
            await ctx.Execute<SystemFront>(SystemFrontHistory, m => m.SystemFrontHistory(ctx, target));
        else if (ctx.Match("fp", "frontpercent", "front%", "frontbreakdown"))
            await ctx.Execute<SystemFront>(SystemFrontPercent, m => m.SystemFrontPercent(ctx, target));
        else if (ctx.Match("info", "view", "show"))
            await ctx.Execute<System>(SystemInfo, m => m.Query(ctx, target));
        else if (ctx.Match("groups", "gs"))
            await ctx.Execute<Groups>(GroupList, g => g.ListSystemGroups(ctx, target));
        else if (!ctx.HasNext())
            await ctx.Execute<System>(SystemInfo, m => m.Query(ctx, target));
        else
            await PrintCommandNotFoundError(ctx, SystemList, SystemFronter, SystemFrontHistory, SystemFrontPercent,
                SystemInfo);
    }

    private async Task HandleMemberCommand(Context ctx)
    {
        if (ctx.Match("new", "n", "add", "create", "register"))
            await ctx.Execute<Member>(MemberNew, m => m.NewMember(ctx));
        else if (ctx.Match("list"))
            await ctx.Execute<SystemList>(SystemList, m => m.MemberList(ctx, ctx.System));
        else if (ctx.Match("commands", "help"))
            await PrintCommandList(ctx, "members", MemberCommands);
        else if (await ctx.MatchMember() is PKMember target)
            await HandleMemberCommandTargeted(ctx, target);
        else if (!ctx.HasNext())
            await PrintCommandExpectedError(ctx, MemberNew, MemberInfo, MemberRename, MemberDisplayName,
                MemberServerName, MemberDesc, MemberPronouns,
                MemberColor, MemberBirthday, MemberProxy, MemberDelete, MemberAvatar);
        else
            await ctx.Reply($"{Emojis.Error} {ctx.CreateNotFoundError("Member", ctx.PopArgument())}");
    }

    private async Task HandleMemberCommandTargeted(Context ctx, PKMember target)
    {
        // Commands that have a member target (eg. pk;member <member> delete)
        if (ctx.Match("rename", "name", "changename", "setname"))
            await ctx.Execute<MemberEdit>(MemberRename, m => m.Name(ctx, target));
        else if (ctx.Match("description", "info", "bio", "text", "desc"))
            await ctx.Execute<MemberEdit>(MemberDesc, m => m.Description(ctx, target));
        else if (ctx.Match("pronouns", "pronoun"))
            await ctx.Execute<MemberEdit>(MemberPronouns, m => m.Pronouns(ctx, target));
        else if (ctx.Match("color", "colour"))
            await ctx.Execute<MemberEdit>(MemberColor, m => m.Color(ctx, target));
        else if (ctx.Match("birthday", "bday", "birthdate", "cakeday", "bdate"))
            await ctx.Execute<MemberEdit>(MemberBirthday, m => m.Birthday(ctx, target));
        else if (ctx.Match("proxy", "tags", "proxytags", "brackets"))
            await ctx.Execute<MemberProxy>(MemberProxy, m => m.Proxy(ctx, target));
        else if (ctx.Match("delete", "remove", "destroy", "erase", "yeet"))
            await ctx.Execute<MemberEdit>(MemberDelete, m => m.Delete(ctx, target));
        else if (ctx.Match("avatar", "profile", "picture", "icon", "image", "pfp", "pic"))
            await ctx.Execute<MemberAvatar>(MemberAvatar, m => m.Avatar(ctx, target));
        else if (ctx.Match("banner", "splash", "cover"))
            await ctx.Execute<MemberEdit>(MemberBannerImage, m => m.BannerImage(ctx, target));
        else if (ctx.Match("group", "groups"))
            if (ctx.Match("add", "a"))
                await ctx.Execute<GroupMember>(MemberGroupAdd,
                    m => m.AddRemoveGroups(ctx, target, Groups.AddRemoveOperation.Add));
            else if (ctx.Match("remove", "rem"))
                await ctx.Execute<GroupMember>(MemberGroupRemove,
                    m => m.AddRemoveGroups(ctx, target, Groups.AddRemoveOperation.Remove));
            else
                await ctx.Execute<GroupMember>(MemberGroups, m => m.ListMemberGroups(ctx, target));
        else if (ctx.Match("serveravatar", "servericon", "serverimage", "serverpfp", "serverpic", "savatar", "spic",
                     "guildavatar", "guildpic", "guildicon", "sicon"))
            await ctx.Execute<MemberAvatar>(MemberServerAvatar, m => m.ServerAvatar(ctx, target));
        else if (ctx.Match("displayname", "dn", "dname", "nick", "nickname", "dispname"))
            await ctx.Execute<MemberEdit>(MemberDisplayName, m => m.DisplayName(ctx, target));
        else if (ctx.Match("servername", "sn", "sname", "snick", "snickname", "servernick", "servernickname",
                     "serverdisplayname", "guildname", "guildnick", "guildnickname", "serverdn"))
            await ctx.Execute<MemberEdit>(MemberServerName, m => m.ServerName(ctx, target));
        else if (ctx.Match("autoproxy", "ap"))
            await ctx.Execute<MemberEdit>(MemberAutoproxy, m => m.MemberAutoproxy(ctx, target));
        else if (ctx.Match("keepproxy", "keeptags", "showtags"))
            await ctx.Execute<MemberEdit>(MemberKeepProxy, m => m.KeepProxy(ctx, target));
        else if (ctx.Match("privacy"))
            await ctx.Execute<MemberEdit>(MemberPrivacy, m => m.Privacy(ctx, target, null));
        else if (ctx.Match("private", "hidden", "hide"))
            await ctx.Execute<MemberEdit>(MemberPrivacy, m => m.Privacy(ctx, target, PrivacyLevel.Private));
        else if (ctx.Match("public", "shown", "show"))
            await ctx.Execute<MemberEdit>(MemberPrivacy, m => m.Privacy(ctx, target, PrivacyLevel.Public));
        else if (ctx.Match("soulscream"))
            await ctx.Execute<Member>(MemberInfo, m => m.Soulscream(ctx, target));
        else if (!ctx.HasNext()) // Bare command
            await ctx.Execute<Member>(MemberInfo, m => m.ViewMember(ctx, target));
        else
            await PrintCommandNotFoundError(ctx, MemberInfo, MemberRename, MemberDisplayName, MemberServerName,
                MemberDesc, MemberPronouns, MemberColor, MemberBirthday, MemberProxy, MemberDelete, MemberAvatar,
                SystemList);
    }

    private async Task HandleGroupCommand(Context ctx)
    {
        // Commands with no group argument
        if (ctx.Match("n", "new"))
            await ctx.Execute<Groups>(GroupNew, g => g.CreateGroup(ctx));
        else if (ctx.Match("list", "l"))
            await ctx.Execute<Groups>(GroupList, g => g.ListSystemGroups(ctx, null));
        else if (ctx.Match("commands", "help"))
            await PrintCommandList(ctx, "groups", GroupCommands);
        else if (await ctx.MatchGroup() is { } target)
        {
            // Commands with group argument
            if (ctx.Match("rename", "name", "changename", "setname"))
                await ctx.Execute<Groups>(GroupRename, g => g.RenameGroup(ctx, target));
            else if (ctx.Match("nick", "dn", "displayname", "nickname"))
                await ctx.Execute<Groups>(GroupDisplayName, g => g.GroupDisplayName(ctx, target));
            else if (ctx.Match("description", "info", "bio", "text", "desc"))
                await ctx.Execute<Groups>(GroupDesc, g => g.GroupDescription(ctx, target));
            else if (ctx.Match("add", "a"))
                await ctx.Execute<GroupMember>(GroupAdd,
                    g => g.AddRemoveMembers(ctx, target, Groups.AddRemoveOperation.Add));
            else if (ctx.Match("remove", "rem", "r"))
                await ctx.Execute<GroupMember>(GroupRemove,
                    g => g.AddRemoveMembers(ctx, target, Groups.AddRemoveOperation.Remove));
            else if (ctx.Match("members", "list", "ms", "l"))
                await ctx.Execute<GroupMember>(GroupMemberList, g => g.ListGroupMembers(ctx, target));
            else if (ctx.Match("random"))
                await ctx.Execute<Random>(GroupMemberRandom, r => r.GroupMember(ctx, target));
            else if (ctx.Match("privacy"))
                await ctx.Execute<Groups>(GroupPrivacy, g => g.GroupPrivacy(ctx, target, null));
            else if (ctx.Match("public", "pub"))
                await ctx.Execute<Groups>(GroupPrivacy, g => g.GroupPrivacy(ctx, target, PrivacyLevel.Public));
            else if (ctx.Match("private", "priv"))
                await ctx.Execute<Groups>(GroupPrivacy, g => g.GroupPrivacy(ctx, target, PrivacyLevel.Private));
            else if (ctx.Match("delete", "remove", "destroy", "erase", "yeet"))
                await ctx.Execute<Groups>(GroupDelete, g => g.DeleteGroup(ctx, target));
            else if (ctx.Match("avatar", "picture", "icon", "image", "pic", "pfp"))
                await ctx.Execute<Groups>(GroupIcon, g => g.GroupIcon(ctx, target));
            else if (ctx.Match("banner", "splash", "cover"))
                await ctx.Execute<Groups>(GroupBannerImage, g => g.GroupBannerImage(ctx, target));
            else if (ctx.Match("fp", "frontpercent", "front%", "frontbreakdown"))
                await ctx.Execute<Groups>(GroupFrontPercent, g => g.GroupFrontPercent(ctx, target));
            else if (ctx.Match("color", "colour"))
                await ctx.Execute<Groups>(GroupColor, g => g.GroupColor(ctx, target));
            else if (!ctx.HasNext())
                await ctx.Execute<Groups>(GroupInfo, g => g.ShowGroupCard(ctx, target));
            else
                await PrintCommandNotFoundError(ctx, GroupCommandsTargeted);
        }
        else if (!ctx.HasNext())
            await PrintCommandExpectedError(ctx, GroupCommands);
        else
            await ctx.Reply($"{Emojis.Error} {ctx.CreateNotFoundError("Group", ctx.PopArgument())}");
    }

    private async Task HandleSwitchCommand(Context ctx)
    {
        if (ctx.Match("out"))
            await ctx.Execute<Switch>(SwitchOut, m => m.SwitchOut(ctx));
        else if (ctx.Match("move", "shift", "offset"))
            await ctx.Execute<Switch>(SwitchMove, m => m.SwitchMove(ctx));
        else if (ctx.Match("edit", "replace"))
            if (ctx.Match("out"))
                await ctx.Execute<Switch>(SwitchEditOut, m => m.SwitchEditOut(ctx));
            else
                await ctx.Execute<Switch>(SwitchEdit, m => m.SwitchEdit(ctx));
        else if (ctx.Match("delete", "remove", "erase", "cancel", "yeet"))
            await ctx.Execute<Switch>(SwitchDelete, m => m.SwitchDelete(ctx));
        else if (ctx.Match("commands", "help"))
            await PrintCommandList(ctx, "switching", SwitchCommands);
        else if (ctx.HasNext()) // there are following arguments
            await ctx.Execute<Switch>(Switch, m => m.SwitchDo(ctx));
        else
            await PrintCommandNotFoundError(ctx, Switch, SwitchOut, SwitchMove, SwitchEdit, SwitchEditOut,
                SwitchDelete, SystemFronter, SystemFrontHistory);
    }

    private async Task CommandHelpRoot(Context ctx)
    {
        if (!ctx.HasNext())
        {
            await ctx.Reply(
                "Available command help targets: `system`, `member`, `group`, `switch`, `autoproxy`, `log`, `blacklist`."
                + "\n- **pk;commands <target>** - *View commands related to a help target.*"
                + "\n\nFor the full list of commands, see the website: <https://pluralkit.me/commands>");
            return;
        }

        switch (ctx.PeekArgument())
        {
            case "system":
            case "systems":
            case "s":
                await PrintCommandList(ctx, "systems", SystemCommands);
                break;
            case "member":
            case "members":
            case "m":
                await PrintCommandList(ctx, "members", MemberCommands);
                break;
            case "group":
            case "groups":
            case "g":
                await PrintCommandList(ctx, "groups", GroupCommands);
                break;
            case "switch":
            case "switches":
            case "switching":
            case "sw":
                await PrintCommandList(ctx, "switching", SwitchCommands);
                break;
            case "log":
                await PrintCommandList(ctx, "message logging", LogCommands);
                break;
            case "blacklist":
            case "bl":
                await PrintCommandList(ctx, "channel blacklisting", BlacklistCommands);
                break;
            case "config":
            case "cfg":
                await PrintCommandList(ctx, "settings", ConfigCommands);
                break;
            // todo: are there any commands that still need to be added?
            default:
                await ctx.Reply("For the full list of commands, see the website: <https://pluralkit.me/commands>");
                break;
        }
    }

    private Task HandleAutoproxyCommand(Context ctx)
    {
        // ctx.CheckSystem();
        // oops, that breaks stuff! PKErrors before ctx.Execute don't actually do anything.
        // so we just emulate checking and throwing an error.
        if (ctx.System == null)
            return ctx.Reply($"{Emojis.Error} {Errors.NoSystemError.Message}");

        // todo: move this whole block to Autoproxy.cs when these are removed

        if (ctx.Match("account", "ac"))
            return ctx.Execute<Config>(ConfigAutoproxyAccount, m => m.AutoproxyAccount(ctx), true);
        if (ctx.Match("timeout", "tm"))
            return ctx.Execute<Config>(ConfigAutoproxyTimeout, m => m.AutoproxyTimeout(ctx), true);

        return ctx.Execute<Autoproxy>(AutoproxySet, m => m.SetAutoproxyMode(ctx));
    }

    private Task HandleConfigCommand(Context ctx)
    {
        if (ctx.System == null)
            return ctx.Reply($"{Emojis.Error} {Errors.NoSystemError.Message}");

        if (!ctx.HasNext())
            return ctx.Execute<Config>(null, m => m.ShowConfig(ctx));

        if (ctx.MatchMultiple(new[] { "autoproxy", "ap" }, new[] { "account", "ac" }))
            return ctx.Execute<Config>(null, m => m.AutoproxyAccount(ctx));
        if (ctx.MatchMultiple(new[] { "autoproxy", "ap" }, new[] { "timeout", "tm" }))
            return ctx.Execute<Config>(null, m => m.AutoproxyTimeout(ctx));
        if (ctx.Match("timezone", "zone", "tz"))
            return ctx.Execute<Config>(null, m => m.SystemTimezone(ctx));
        if (ctx.Match("ping"))
            return ctx.Execute<Config>(null, m => m.SystemPing(ctx));

        // todo: maybe add the list of configuration keys here?
        return ctx.Reply($"{Emojis.Error} Could not find a setting with that name. Please see `pk;commands config` for the list of possible config settings.");
    }

    private async Task PrintCommandNotFoundError(Context ctx, params Command[] potentialCommands)
    {
        var commandListStr = CreatePotentialCommandList(potentialCommands);
        await ctx.Reply(
            $"{Emojis.Error} Unknown command `pk;{ctx.FullCommand().Truncate(100)}`. Perhaps you meant to use one of the following commands?\n{commandListStr}\n\nFor a full list of possible commands, see <https://pluralkit.me/commands>.");
    }

    private async Task PrintCommandExpectedError(Context ctx, params Command[] potentialCommands)
    {
        var commandListStr = CreatePotentialCommandList(potentialCommands);
        await ctx.Reply(
            $"{Emojis.Error} You need to pass a command. Perhaps you meant to use one of the following commands?\n{commandListStr}\n\nFor a full list of possible commands, see <https://pluralkit.me/commands>.");
    }

    private static string CreatePotentialCommandList(params Command[] potentialCommands)
    {
        return string.Join("\n", potentialCommands.Select(cmd => $"- **pk;{cmd.Usage}** - *{cmd.Description}*"));
    }

    private async Task PrintCommandList(Context ctx, string subject, params Command[] commands)
    {
        var str = CreatePotentialCommandList(commands);
        await ctx.Reply($"Here is a list of commands related to {subject}: \n{str}\nFor a full list of possible commands, see <https://pluralkit.me/commands>.");
    }

    private async Task<string> CreateSystemNotFoundError(Context ctx)
    {
        var input = ctx.PopArgument();
        if (input.TryParseMention(out var id))
        {
            // Try to resolve the user ID to find the associated account,
            // so we can print their username.
            var user = await ctx.Rest.GetUser(id);
            if (user != null)
                return $"Account **{user.Username}#{user.Discriminator}** does not have a system registered.";
            return $"Account with ID `{id}` not found.";
        }

        return $"System with ID {input.AsCode()} not found.";
    }
}