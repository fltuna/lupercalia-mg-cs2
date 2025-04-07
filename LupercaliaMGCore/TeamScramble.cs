using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore;

public class TeamScramble(LupercaliaMGCore plugin) : PluginModuleBase(plugin)
{
    public override string PluginModuleName => "TeamScramble";

    private static readonly Random Random = new();

    public override void Initialize()
    {
        Plugin.RegisterEventHandler<EventRoundPrestart>(OnRoundEnd);
    }

    public override void UnloadModule()
    {
        Plugin.DeregisterEventHandler<EventRoundPrestart>(OnRoundEnd);
    }

    private HookResult OnRoundEnd(EventRoundPrestart @event, GameEventInfo info)
    {
        if (!PluginSettings.m_CVIsScrambleEnabled.Value)
            return HookResult.Continue;

        SimpleLogging.LogDebug("[Team Scramble] Called");

        List<CCSPlayerController> players = Utilities.GetPlayers()
            .Where(p => p.Team != CsTeam.None && p.Team != CsTeam.Spectator).ToList();

        int playerCount = players.Count;
        int playerCountHalf = playerCount / 2;
        SimpleLogging.LogTrace($"[Team Scramble] player count: {playerCount}, half: {playerCountHalf}");

        int teamCountCT = 0;
        int teamCountT = 0;

        foreach (var client in players)
        {
            int randomTeam = Random.Next(0, 5000);
            if (randomTeam >= 2500)
            {
                if (teamCountCT >= playerCountHalf)
                {
                    SimpleLogging.LogTrace($"Player {client.PlayerName} moved to Terrorist");
                    client.SwitchTeam(CsTeam.Terrorist);
                }
                else
                {
                    SimpleLogging.LogTrace($"Player {client.PlayerName} moved to CT");
                    client.SwitchTeam(CsTeam.CounterTerrorist);
                    teamCountCT++;
                }
            }
            else
            {
                if (teamCountT >= playerCountHalf)
                {
                    SimpleLogging.LogTrace($"Player {client.PlayerName} moved to CT");
                    client.SwitchTeam(CsTeam.CounterTerrorist);
                }
                else
                {
                    SimpleLogging.LogTrace($"Player {client.PlayerName} moved to Terrorist");
                    client.SwitchTeam(CsTeam.Terrorist);
                    teamCountT++;
                }
            }
        }

        SimpleLogging.LogDebug("[Team Scramble] Done");
        return HookResult.Continue;
    }
}