using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System.Drawing;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Admin;

namespace LupercaliaMGCore {
    public class VoteRoundRestart
    {
        private LupercaliaMGCore m_CSSPlugin;
        private List<CCSPlayerController> votedPlayers = new List<CCSPlayerController>();
        private int playersRequiredToRestart = 0;
        private bool isRoundRestarting = false;

        public VoteRoundRestart(LupercaliaMGCore plugin) {
            m_CSSPlugin = plugin;

            m_CSSPlugin.AddCommand("css_vrr", "Vote round restart command.", CommandVoteRestartRound);
            m_CSSPlugin.AddCommand("css_restart", "Restarts round (admins only)", CommandForceRestartRound);
            m_CSSPlugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
        }

        private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info) {
            isRoundRestarting = false;
            votedPlayers.Clear();
            return HookResult.Continue;
        }
        private void CommandVoteRestartRound(CCSPlayerController? client, CommandInfo info) {
            if(client == null)
                return;
            
            SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] trying to vote for restart round.");
            if(isRoundRestarting) {
                SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] Round is already restarting in progress.");
                client.PrintToChat(LupercaliaMGCore.MessageWithPrefix(m_CSSPlugin.Localizer["VoteRoundRestart.Command.Notification.AlreadyRestarting"]));
                return;
            }

            if(votedPlayers.Contains(client)) {
                SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] trying to vote for restart round.");
                client.PrintToChat(LupercaliaMGCore.MessageWithPrefix(m_CSSPlugin.Localizer["VoteRoundRestart.Command.Notification.AlreadyVoted"]));
                return;
            }

            votedPlayers.Add(client);
            SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] voted to restart round.");
            playersRequiredToRestart = (int)Math.Ceiling(Utilities.GetPlayers().Count(player => !player.IsBot && !player.IsHLTV) * PluginSettings.getInstance.m_CVVoteRoundRestartThreshold.Value);

            SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] players count: {votedPlayers.Count}, Requires to restart: {playersRequiredToRestart}");
            Server.PrintToChatAll(LupercaliaMGCore.MessageWithPrefix(m_CSSPlugin.Localizer["VoteRoundRestart.Notification.PlayerVote", client.PlayerName, votedPlayers.Count(), playersRequiredToRestart]));

            if(votedPlayers.Count < playersRequiredToRestart)
                return;
            
            InitiateRoundRestart();
        }

        [RequiresPermissions(@"css/root")]
        private void CommandForceRestartRound(CCSPlayerController? client, CommandInfo info)
        {
            if (client == null)
                return;

            SimpleLogging.LogDebug("[Force Round Restart] Initiating round restart...");
            isRoundRestarting = true;

            Server.PrintToChatAll(LupercaliaMGCore.MessageWithPrefix(m_CSSPlugin.Localizer["VoteRoundRestart.Notification.ForceRoundRestart", 1]));
            m_CSSPlugin.AddTimer(1, () => {
                SimpleLogging.LogDebug("[Vote Round Restart] Restarting round.");
                Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules?.TerminateRound(0.0F, RoundEndReason.RoundDraw);
            }, TimerFlags.STOP_ON_MAPCHANGE);
        }

        private void InitiateRoundRestart() {
            SimpleLogging.LogDebug("[Vote Round Restart] Initiating round restart...");
            isRoundRestarting = true;

            Server.PrintToChatAll(LupercaliaMGCore.MessageWithPrefix(m_CSSPlugin.Localizer["VoteRoundRestart.Notification.RoundRestart", PluginSettings.getInstance.m_CVVoteRoundRestartRestartTime.Value]));
            m_CSSPlugin.AddTimer(PluginSettings.getInstance.m_CVVoteRoundRestartRestartTime.Value, () => {
                SimpleLogging.LogDebug("[Vote Round Restart] Restarting round.");
                Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules?.TerminateRound(0.0F, RoundEndReason.RoundDraw);
            }, TimerFlags.STOP_ON_MAPCHANGE);
        }
    }
}