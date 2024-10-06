using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;

namespace EntWatchSharp.Helpers;

public static class FindTarget
{
    public enum MultipleFlags
    {
        NORMAL = 0,
        IGNORE_DEAD_PLAYERS,
        IGNORE_ALIVE_PLAYERS
    }

    public static (List<CCSPlayerController> players, string targetname) Find
        (
            CCSPlayerController player,
            CommandInfo command,
            int numArg,
            bool singletarget,
            bool immunitycheck,
            MultipleFlags flags,
            bool shownomatching = true
        )
    {
        if (command.ArgCount < numArg)
        {
            return ([], string.Empty);
        }

		bool bConsole = command.CallingContext == CommandCallingContext.Console;

		TargetResult targetresult = command.GetArgTargetResult(numArg);
        if (targetresult.Players.Count == 0)
        {
            if (shownomatching) UI.EWReplyInfo(player, "Reply.No_matching_client", bConsole);

            return ([], string.Empty);
        }

        else if (singletarget && targetresult.Players.Count > 1)
        {
            UI.EWReplyInfo(player, "Reply.More_than_one_client_matched", bConsole);

            return ([], string.Empty);
        }

        if (immunitycheck)
        {
            targetresult.Players.RemoveAll(target => !AdminManager.CanPlayerTarget(player, target));

            if (targetresult.Players.Count == 0)
            {
                UI.EWReplyInfo(player, "Reply.You_cannot_target", bConsole);

                return ([], string.Empty);
            }
        }

        if (flags == MultipleFlags.IGNORE_DEAD_PLAYERS)
        {
            targetresult.Players.RemoveAll(target => !target.PawnIsAlive);

            if (targetresult.Players.Count == 0)
            {
                UI.EWReplyInfo(player, "Reply.You_can_target_only_alive_players", bConsole);

                return ([], string.Empty);
            }
        }
        else if (flags == MultipleFlags.IGNORE_ALIVE_PLAYERS)
        {
            targetresult.Players.RemoveAll(target => target.PawnIsAlive);

            if (targetresult.Players.Count == 0)
            {
                UI.EWReplyInfo(player, "Reply.You_can_target_only_dead_players", bConsole);

                return ([], string.Empty);
            }
        }

        string targetname;

        if (targetresult.Players.Count == 1)
        {
            targetname = targetresult.Players.Single().PlayerName;
        }
        else
        {
            Target.TargetTypeMap.TryGetValue(command.GetArg(1), out TargetType type);

            targetname = type switch
            {
                TargetType.GroupAll => EntWatchSharp.Strlocalizer["all"],
                TargetType.GroupBots => EntWatchSharp.Strlocalizer["bots"],
                TargetType.GroupHumans => EntWatchSharp.Strlocalizer["humans"],
                TargetType.GroupAlive => EntWatchSharp.Strlocalizer["alive"],
                TargetType.GroupDead => EntWatchSharp.Strlocalizer["dead"],
                TargetType.GroupNotMe => EntWatchSharp.Strlocalizer["notme"],
                TargetType.PlayerMe => targetresult.Players.First().PlayerName,
                TargetType.TeamCt => EntWatchSharp.Strlocalizer["ct"],
                TargetType.TeamT => EntWatchSharp.Strlocalizer["t"],
                TargetType.TeamSpec => EntWatchSharp.Strlocalizer["spec"],
                _ => targetresult.Players.First().PlayerName
            };
        }

        return (targetresult.Players, targetname);
    }
}