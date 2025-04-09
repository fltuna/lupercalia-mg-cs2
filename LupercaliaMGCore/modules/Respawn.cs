using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore.modules;

public class Respawn(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "Respawn";
    
    public override string ModuleChatPrefix => "[Respawn]";
    
    private static readonly string ChatPrefix = $" {ChatColors.Green}[Respawn]{ChatColors.Default}";

    private readonly Dictionary<int, double> playerLastRespawnTime = new();
    private bool repeatKillDetected = false;

    
    public readonly FakeConVar<bool> IsAutoRespawnEnabled =
        new("lp_mg_auto_respawn_enabled", "Auto respawn feature is enabled", false);

    public readonly FakeConVar<float> SpawnKillingDetectionTime =
        new("lp_mg_auto_respawn_repeat_kill_time", "Seconds to detect as spawn killing.", 1.0F);

    public readonly FakeConVar<float> AutoRespawnTime =
        new("lp_mg_auto_respawn_time", "How long to respawn after death.", 1.0F);
    
    protected override void OnInitialize()
    {
        TrackConVar(IsAutoRespawnEnabled);
        TrackConVar(SpawnKillingDetectionTime);
        TrackConVar(AutoRespawnTime);
        
        Plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        Plugin.RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);
        Plugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
        Plugin.AddCommand("css_reset_respawn", "Reset the current repeat kill detection status", CommandRemoveRepeatKill);
        Plugin.AddCommand("css_rrs", "Reset the current repeat kill detection status", CommandRemoveRepeatKill);
    }

    protected override void OnUnloadModule()
    {
        Plugin.DeregisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        Plugin.DeregisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        Plugin.DeregisterEventHandler<EventPlayerTeam>(OnPlayerTeam);
        Plugin.DeregisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
        Plugin.RemoveCommand("css_reset_respawn", CommandRemoveRepeatKill);
        Plugin.RemoveCommand("css_rrs", CommandRemoveRepeatKill);
    }

    [RequiresPermissions(@"css/slay")]
    private void CommandRemoveRepeatKill(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        DebugLogger.LogDebug($"Admin {client.PlayerName} is enabled auto respawn.");

        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            if (PlayerUtil.IsPlayerAlive(cl))
                continue;

            RespawnPlayer(cl);
        }

        repeatKillDetected = false;
        DebugLogger.LogTrace($"Repeat kill status: {repeatKillDetected}");
        Server.PrintToChatAll($"{ChatPrefix} {Plugin.Localizer["Respawn.Notification.AdminEnabledRespawn", client.PlayerName]}");
    }

    private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info)
    {
        repeatKillDetected = false;
        SetIgnoreRoundWinCondition(IsAutoRespawnEnabled.Value);

        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (!IsAutoRespawnEnabled.Value || repeatKillDetected)
            return HookResult.Continue;

        var player = @event.Userid;

        if (player == null)
            return HookResult.Continue;

        if (player.IsBot || player.IsHLTV)
            return HookResult.Continue;

        DebugLogger.LogDebug($"{ChatPrefix} [Player {player.PlayerName}] Trying to respawn.");

        int index = (int)player.Index;

        playerLastRespawnTime.TryAdd(index, 0.0D);

        if (Server.EngineTime - playerLastRespawnTime[index] <= SpawnKillingDetectionTime.Value)
        {
            repeatKillDetected = true;
            
            DebugLogger.LogDebug($"{ChatPrefix} [Player {player.PlayerName}] Repeat kill is detected.");
            Server.PrintToChatAll($"{ChatPrefix} {ChatUtil.ReplaceColorStrings(Plugin.Localizer["Respawn.Notification.RepeatKillDetected"])}");
            
            SetIgnoreRoundWinCondition(false);
            return HookResult.Continue;
        }

        DebugLogger.LogDebug($"{ChatPrefix} [Player {player.PlayerName}] Respawning player.");
        Plugin.AddTimer(AutoRespawnTime.Value, () =>
        {
            DebugLogger.LogDebug($"{ChatPrefix} [Player {player.PlayerName}] Respawned.");
            RespawnPlayer(player);
        }, TimerFlags.STOP_ON_MAPCHANGE);

        DebugLogger.LogDebug($"{ChatPrefix} [Player {player.PlayerName}] Done.");
        return HookResult.Continue;
    }

    private HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        if (!IsAutoRespawnEnabled.Value || repeatKillDetected)
            return HookResult.Continue;

        var player = @event.Userid;

        if (player == null)
            return HookResult.Continue;

        if (player.IsBot || player.IsHLTV)
            return HookResult.Continue;

        var team = (CsTeam)@event.Team;

        if (team == CsTeam.None || team == CsTeam.Spectator)
        {
            return HookResult.Continue;
        }

        DebugLogger.LogDebug($"{ChatPrefix} [Player {player.PlayerName}] has joined the team {team.ToString()}.");

        Server.NextFrame(() =>
        {
            DebugLogger.LogDebug($"{ChatPrefix} [Player {player.PlayerName}] Respawned due to team change.");
            player.Respawn();
        });

        return HookResult.Continue;
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (!IsAutoRespawnEnabled.Value || repeatKillDetected)
            return HookResult.Continue;

        var player = @event.Userid;

        if (player == null)
            return HookResult.Continue;

        if (player.IsBot || player.IsHLTV)
            return HookResult.Continue;

        int index = (int)player.Index;

        playerLastRespawnTime.TryAdd(index, 0.0D);

        playerLastRespawnTime[index] = Server.EngineTime;
        return HookResult.Continue;
    }

    private void RespawnPlayer(CCSPlayerController client)
    {
        if (client.Team == CsTeam.None || client.Team == CsTeam.Spectator)
            return;

        client.Respawn();
        client.PrintToChat($"{ChatPrefix} {Plugin.Localizer["Respawn.Notification.Respawned"]}");
    }

    private void SetIgnoreRoundWinCondition(bool isIgnored)
    {
        ConVar? mp_ignore_round_win_conditions = ConVar.Find("mp_ignore_round_win_conditions");

        if (mp_ignore_round_win_conditions == null)
        {
            Plugin.Logger.LogError("Failed to find mp_ignore_round_win_conditions!");
            return;
        }

        mp_ignore_round_win_conditions.SetValue(isIgnored);
    }
}