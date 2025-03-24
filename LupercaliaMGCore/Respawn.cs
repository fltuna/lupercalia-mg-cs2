using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Cvars;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore {
    public class Respawn {
        private LupercaliaMGCore m_CSSPlugin;
        private bool repeatKillDetected = false;

        private static string CHAT_PREFIX = $" {ChatColors.Green}[Respawn]{ChatColors.Default}";

        private Dictionary<int ,double> playerLastRespawnTime = new Dictionary<int, double>();

        public Respawn(LupercaliaMGCore plugin) {
            m_CSSPlugin = plugin;
            m_CSSPlugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            m_CSSPlugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            m_CSSPlugin.RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);
            m_CSSPlugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
            m_CSSPlugin.AddCommand("css_reset_respawn", "Reset the current repeat kill detection status", CommandRemoveRepeatKill);
            m_CSSPlugin.AddCommand("css_rrs", "Reset the current repeat kill detection status", CommandRemoveRepeatKill);
        }

        [RequiresPermissions(@"css/slay")]
        private void CommandRemoveRepeatKill(CCSPlayerController? client, CommandInfo info) {
            if(client == null) 
                return;
            
            SimpleLogging.LogDebug($"Admin {client.PlayerName} is enabled auto respawn.");

            foreach(CCSPlayerController cl in Utilities.GetPlayers()) {
                if(!cl.IsValid || cl.IsBot || cl.IsHLTV)
                    continue;

                if(cl.PlayerPawn.Value == null || cl.PlayerPawn.Value.LifeState == (byte)LifeState_t.LIFE_ALIVE)
                    continue;
                
                respawnPlayer(cl);
            }
            repeatKillDetected = false;
            SimpleLogging.LogTrace($"Repeat kill status: {repeatKillDetected}");
            Server.PrintToChatAll($"{CHAT_PREFIX} {m_CSSPlugin.Localizer["Respawn.Notification.AdminEnabledRespawn", client.PlayerName]}");
        }

        private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info) {
            repeatKillDetected = false;
            if(PluginSettings.getInstance.m_CVAutoRespawnEnabled.Value) {
                setIgnoreRoundWinCondition(true);
            }
            else {
                setIgnoreRoundWinCondition(false);
            }
            return HookResult.Continue;
        }

        private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info) {
            if(!PluginSettings.getInstance.m_CVAutoRespawnEnabled.Value || repeatKillDetected)
                return HookResult.Continue;
            
            var player = @event.Userid;

            if(player == null)
                return HookResult.Continue;

            if(player.IsBot || player.IsHLTV)
                return HookResult.Continue;

            SimpleLogging.LogDebug($"{CHAT_PREFIX} [Player {player.PlayerName}] Trying to respawn.");

            int index = (int)player.Index;

            if(!playerLastRespawnTime.ContainsKey(index)) {
                playerLastRespawnTime[index] = 0.0D;
            }

            if(Server.EngineTime - playerLastRespawnTime[index] <= PluginSettings.getInstance.m_CVAutoRespawnSpawnKillingDetectionTime.Value) {
                repeatKillDetected = true;
                SimpleLogging.LogDebug($"{CHAT_PREFIX} [Player {player.PlayerName}] Repeat kill is detected.");
                Server.PrintToChatAll($"{CHAT_PREFIX} {ChatUtil.ReplaceColorStrings(m_CSSPlugin.Localizer["Respawn.Notification.RepeatKillDetected"])}");
                setIgnoreRoundWinCondition(false);
                return HookResult.Continue;
            }

            SimpleLogging.LogDebug($"{CHAT_PREFIX} [Player {player.PlayerName}] Respawning player.");
            m_CSSPlugin.AddTimer(PluginSettings.getInstance.m_CVAutoRespawnSpawnTime.Value, () => {
                SimpleLogging.LogDebug($"{CHAT_PREFIX} [Player {player.PlayerName}] Respawned.");
                respawnPlayer(player);
            }, TimerFlags.STOP_ON_MAPCHANGE);

            SimpleLogging.LogDebug($"{CHAT_PREFIX} [Player {player.PlayerName}] Done.");
            return HookResult.Continue;
        }

        private HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info) {
            if (!PluginSettings.getInstance.m_CVAutoRespawnEnabled.Value || repeatKillDetected)
                return HookResult.Continue;

            var player = @event.Userid;

            if (player == null)
                return HookResult.Continue;

            if (player.IsBot || player.IsHLTV)
                return HookResult.Continue;

            var team = (CsTeam)@event.Team;

            if (team == CsTeam.None || team == CsTeam.Spectator) {
                return HookResult.Continue;
            }

            SimpleLogging.LogDebug($"{CHAT_PREFIX} [Player {player.PlayerName}] has joined the team {team.ToString()}.");

            Server.NextFrame(() => {
                SimpleLogging.LogDebug($"{CHAT_PREFIX} [Player {player.PlayerName}] Respawned due to team change.");
                player.Respawn();
            });

            return HookResult.Continue;
        }
        
        private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info) {
            if(!PluginSettings.getInstance.m_CVAutoRespawnEnabled.Value || repeatKillDetected)
                return HookResult.Continue;

            var player = @event.Userid;

            if(player == null)
                return HookResult.Continue;

            if(player.IsBot || player.IsHLTV)
                return HookResult.Continue;

            int index = (int)player.Index;

            if(!playerLastRespawnTime.ContainsKey(index)) {
                playerLastRespawnTime[index] = 0.0D;
            }

            playerLastRespawnTime[index] = Server.EngineTime;
            return HookResult.Continue;
        }

        private void respawnPlayer(CCSPlayerController client) {
            if(client.Team == CsTeam.None || client.Team == CsTeam.Spectator)
                return;

            client.Respawn();
            client.PrintToChat($"{CHAT_PREFIX} {m_CSSPlugin.Localizer["Respawn.Notification.Respawned"]}");
        }

        private void setIgnoreRoundWinCondition(bool isIgnored) {
            ConVar? mp_ignore_round_win_conditions = ConVar.Find("mp_ignore_round_win_conditions");

            if(mp_ignore_round_win_conditions == null) {
                m_CSSPlugin.Logger.LogError("Failed to find mp_ignore_round_win_conditions!");
                return;
            }

            mp_ignore_round_win_conditions.SetValue(isIgnored);
        }
    }
}