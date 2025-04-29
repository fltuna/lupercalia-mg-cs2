using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using TNCSSPluginFoundation.Models.Plugin;

namespace LupercaliaMGCore.modules;

public sealed class TeamScramble(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "TeamScramble";
    
    public override string ModuleChatPrefix => "[TeamScramble]";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private static readonly Random Random = new();
    
    public readonly FakeConVar<bool> IsModuleEnabled =
        new("lp_mg_teamscramble_enabled", "Should team is scrambled after round end", true);
    

    protected override void OnInitialize()
    {
        TrackConVar(IsModuleEnabled);
        Plugin.RegisterEventHandler<EventRoundPrestart>(OnRoundEnd);
    }

    protected override void OnUnloadModule()
    {
        Plugin.DeregisterEventHandler<EventRoundPrestart>(OnRoundEnd);
    }

    private HookResult OnRoundEnd(EventRoundPrestart @event, GameEventInfo info)
    {
        if (!IsModuleEnabled.Value)
            return HookResult.Continue;

        DebugLogger.LogDebug("[Team Scramble] Called");

        List<CCSPlayerController> players = Utilities.GetPlayers()
            .Where(p => p.Team != CsTeam.None && p.Team != CsTeam.Spectator).ToList();

        int playerCount = players.Count;
        int playerCountHalf = playerCount / 2;
        DebugLogger.LogTrace($"[Team Scramble] player count: {playerCount}, half: {playerCountHalf}");

        int teamCountCT = 0;
        int teamCountT = 0;

        foreach (var client in players)
        {
            int randomTeam = Random.Next(0, 5000);
            if (randomTeam >= 2500)
            {
                if (teamCountCT >= playerCountHalf)
                {
                    DebugLogger.LogTrace($"Player {client.PlayerName} moved to Terrorist");
                    client.SwitchTeam(CsTeam.Terrorist);
                }
                else
                {
                    DebugLogger.LogTrace($"Player {client.PlayerName} moved to CT");
                    client.SwitchTeam(CsTeam.CounterTerrorist);
                    teamCountCT++;
                }
            }
            else
            {
                if (teamCountT >= playerCountHalf)
                {
                    DebugLogger.LogTrace($"Player {client.PlayerName} moved to CT");
                    client.SwitchTeam(CsTeam.CounterTerrorist);
                }
                else
                {
                    DebugLogger.LogTrace($"Player {client.PlayerName} moved to Terrorist");
                    client.SwitchTeam(CsTeam.Terrorist);
                    teamCountT++;
                }
            }
        }

        DebugLogger.LogDebug("[Team Scramble] Done");
        return HookResult.Continue;
    }
}