using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class PlayerHealEvent(Omikuji omikuji, LupercaliaMGCore plugin) : OmikujiEventBase(omikuji, plugin)
{
    public override string EventName => "Player Heal Event";

    public override OmikujiType OmikujiType => OmikujiType.EventLucky;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PlayerAlive;

    public override void Execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Player heal event.");

        string msg;

        bool isPlayerAlive = PlayerUtil.IsPlayerAlive(client);

        if (isPlayerAlive)
        {
            msg = $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {Plugin.Localizer["Omikuji.LuckyEvent.PlayerHealEvent.Notification.Healed", client.PlayerName, PluginSettings.GetInstance.m_CVOmikujiEventPlayerHeal.Value]}";
        }
        else
        {
            msg = $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {Plugin.Localizer["Omikuji.LuckyEvent.PlayerHealEvent.Notification.PlayerIsDead", client.PlayerName]}";
        }


        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            cl.PrintToChat(msg);
        }

        if (!isPlayerAlive)
            return;

        CCSPlayerPawn playerPawn = client.PlayerPawn.Value!;

        if (playerPawn.MaxHealth < playerPawn.Health + PluginSettings.GetInstance.m_CVOmikujiEventPlayerHeal.Value)
        {
            playerPawn.Health = playerPawn.MaxHealth;
        }
        else
        {
            playerPawn.Health += PluginSettings.GetInstance.m_CVOmikujiEventPlayerHeal.Value;
        }

        Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
    }

    public override double GetOmikujiWeight()
    {
        return PluginSettings.m_CVOmikujiEventPlayerHealSelectionWeight.Value;
    }
}