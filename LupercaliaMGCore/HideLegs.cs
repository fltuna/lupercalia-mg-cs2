using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using System.Drawing;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace LupercaliaMGCore
{
    public class HideLegs
    {
        private LupercaliaMGCore m_CSSPlugin;

        private Dictionary<ulong, bool> m_steamIdToIsHideLegsActive = new Dictionary<ulong, bool>();

        public HideLegs(LupercaliaMGCore plugin)
        {
            m_CSSPlugin = plugin;

            m_CSSPlugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);

            m_CSSPlugin.AddCommand("css_legs", "Toggles the visibility of the firstperson legs view model", CommandLegs);
        }

        private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;

            if (player == null || !player.IsValid)
            {
                return HookResult.Continue;
            }

            UpdateHideLegs(player);

            return HookResult.Continue;
        }

        private void CommandLegs(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.IsValid)
            {
                return;
            }

            bool isHideLegsActive = !m_steamIdToIsHideLegsActive.GetValueOrDefault(player.SteamID, false);
            m_steamIdToIsHideLegsActive[player.SteamID] = isHideLegsActive;

            var messageName = isHideLegsActive ? "HideLegs.Command.Notification.HideLegs" : "HideLegs.Command.Notification.ShowLegs";
            player.PrintToChat(LupercaliaMGCore.MessageWithPrefix(m_CSSPlugin.Localizer[messageName]));

            UpdateHideLegs(player);
        }

        private void UpdateHideLegs(CCSPlayerController player)
        {
            bool isHideLegsActive = m_steamIdToIsHideLegsActive.GetValueOrDefault(player.SteamID, false);
            SetLegsVisibility(player, !isHideLegsActive);
        }

        // Borrowed from
        // - https://github.com/dran1x/CS2-HideLowerBody
        // - https://github.com/1Mack/CS2-HideLegs
        private void SetLegsVisibility(CCSPlayerController player, bool isVisible)
        {
            CBasePlayerPawn? playerPawn = player.PlayerPawn.Value;
            if (playerPawn == null)
            {
                return;
            }

            playerPawn.Render = Color.FromArgb(
                isVisible ? 255 : 254,
                playerPawn.Render.R,
                playerPawn.Render.G,
                playerPawn.Render.B
            );
            Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
        }
    }
}
