using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System.Drawing;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;

namespace LupercaliaMGCore {
    public class TeamBasedBodyColor
    {
        private LupercaliaMGCore m_CSSPlugin;

        public TeamBasedBodyColor(LupercaliaMGCore plugin) {
            m_CSSPlugin = plugin;
            
            m_CSSPlugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        }

        private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info) {
            CCSPlayerController? player = @event.Userid;

            if(player == null || !player.IsValid)
                return HookResult.Continue;

            if(player.Team == CsTeam.None || player.Team == CsTeam.Spectator)
                return HookResult.Continue;

            SimpleLogging.LogDebug($"[Team Based Body Color] [Player {player.PlayerName}] spawned");

            //
            // Default player color = A255 R255 G255 B255.
            //
            // We will compare `newColor` and `playerPawn.Render` to check if we need to send the model state change to clients.
            //
            // `playerPawn.Render` always returns an actual ARGB value rather than the color name (e.g., "white").
            // So keep your mind that if you assign `Color.White` to `newColor`, `playerPawn.Render != newColor` becomes
            // always negative since `Color.White` is not equal to `Color.FromArgb(255, 255, 255, 255)`.
            //
            Color newColor = Color.FromArgb(255, 255, 255, 255);
            RenderMode_t renderMode = RenderMode_t.kRenderNormal;

            if (PluginSettings.getInstance.m_CVIsTeamColorEnabled.Value)
            {
                // Use team color
                List<int> rgb = new List<int>();
                if (player.Team == CsTeam.CounterTerrorist)
                {
                    rgb = PluginSettings.getInstance.m_CVTeamColorCT.Value.Split(',').Select(s => int.Parse(s.Trim())).ToList();
                }
                else if (player.Team == CsTeam.Terrorist)
                {
                    rgb = PluginSettings.getInstance.m_CVTeamColorT.Value.Split(',').Select(s => int.Parse(s.Trim())).ToList();
                }
                newColor = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                SimpleLogging.LogTrace($"[Team Based Body Color] Player {player.PlayerName}'s team: {player.Team}, color: {newColor.R} {newColor.G} {newColor.B}");

                renderMode = RenderMode_t.kRenderTransColor;
            }
            
            CBasePlayerPawn playerPawn = player.PlayerPawn.Value!;

            if (playerPawn.Render != newColor || playerPawn.RenderMode != renderMode)
            {
                SimpleLogging.LogTrace($"[Team Based Body Color] [Player {player.PlayerName}] render mode changed from {playerPawn.RenderMode} to {renderMode}");
                SimpleLogging.LogTrace($"[Team Based Body Color] [Player {player.PlayerName}] color changed from {playerPawn.Render} to {newColor}");
                playerPawn.RenderMode = renderMode;
                playerPawn.Render = newColor;

                SimpleLogging.LogTrace($"[Team Based Body Color] [Player {player.PlayerName}] Sending state change.");
                Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
                SimpleLogging.LogDebug($"[Team Based Body Color] [Player {player.PlayerName}] Done.");
            }
            else
            {
                SimpleLogging.LogTrace($"[Team Based Body Color] [Player {player.PlayerName}] Nothing has been changed.");
            }
            
            return HookResult.Continue;
        }
    }
}