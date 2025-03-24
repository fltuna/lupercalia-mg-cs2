using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class PlayerHealEvent : IOmikujiEvent
{
    public string EventName => "Player Heal Event";

    public OmikujiType OmikujiType => OmikujiType.EVENT_LUCKY;

    public OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PLAYER_ALIVE;

    public void execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Player heal event.");

        string msg;

        bool isPlayerAlive = client.PlayerPawn.Value != null &&
                             client.PlayerPawn.Value.LifeState == (byte)LifeState_t.LIFE_ALIVE;

        if (isPlayerAlive)
        {
            msg =
                $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {LupercaliaMGCore.getInstance().Localizer["Omikuji.LuckyEvent.PlayerHealEvent.Notification.Healed", client.PlayerName, PluginSettings.GetInstance.m_CVOmikujiEventPlayerHeal.Value]}";
        }
        else
        {
            msg =
                $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {LupercaliaMGCore.getInstance().Localizer["Omikuji.LuckyEvent.PlayerHealEvent.Notification.PlayerIsDead", client.PlayerName]}";
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

    public void initialize()
    {
    }

    public double getOmikujiWeight()
    {
        return PluginSettings.GetInstance.m_CVOmikujiEventPlayerHealSelectionWeight.Value;
    }
}