using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;

namespace LupercaliaMGCore;

public class EasySpectate
{
    private LupercaliaMGCore m_CSSPlugin;

    private readonly Dictionary<CCSPlayerController, double> playerLastButtonPressedTime = new();

    private readonly double buttonCooldown = 1.0D;
    private int OFFSET_CGameRules_FindPickerEntity;

    public EasySpectate(LupercaliaMGCore plugin, bool hotReload)
    {
        m_CSSPlugin = plugin;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            OFFSET_CGameRules_FindPickerEntity = 27;
        }
        else
        {
            OFFSET_CGameRules_FindPickerEntity = 28;
        }

        if (hotReload)
        {
            List<CCSPlayerController> players = Utilities.GetPlayers();
            foreach (CCSPlayerController player in players)
            {
                if (player == null || !player.IsValid)
                    continue;

                Server.PrintToChatAll("Hooking!");
                hookPlayerButton(player);
            }
        }
    }

    private void hookPlayerButton(CCSPlayerController player)
    {
        if (player == null)
            return;

        playerLastButtonPressedTime[player] = Server.EngineTime;

        m_CSSPlugin.RegisterListener<Listeners.OnTick>(() =>
        {
            if ((player.Buttons & PlayerButtons.Use) == 0)
                return;

            if (Server.EngineTime - playerLastButtonPressedTime[player] < buttonCooldown)
                return;

            // TODO. Will be implemented after Trace Ray feature implemented in CounterStrikeSharp.

            CCSPlayerController? targetPlayer = GetClientAimTarget(player);

            if (targetPlayer == null)
                return;

            player.ExecuteClientCommandFromServer($"spec_player {targetPlayer.PlayerName}");
            player.PrintToChat($"{targetPlayer.PlayerName} is in your LOS!");
        });
    }

    // Code from CounterStrikeSharp official Discord
    // Thanks to xstage.
    // https://discord.com/channels/1160907911501991946/1175947333880524962/1230542480903110716
    public CCSPlayerController? GetClientAimTarget(CCSPlayerController player)
    {
        var GameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()
            ?.GameRules;

        if (GameRules is null)
            return null;

        VirtualFunctionWithReturn<IntPtr, IntPtr, IntPtr> findPickerEntity =
            new(GameRules.Handle, OFFSET_CGameRules_FindPickerEntity);
        var target = new CBaseEntity(findPickerEntity.Invoke(GameRules.Handle, player.Handle));

        if (target.DesignerName is "player")
        {
            return target.As<CCSPlayerPawn>().OriginalController.Value;
        }

        return null;
    }
}