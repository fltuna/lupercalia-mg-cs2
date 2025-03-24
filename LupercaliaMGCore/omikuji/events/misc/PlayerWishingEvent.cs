using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class PlayerWishingEvent : IOmikujiEvent
{
    public string EventName => "Player Wishing Event";

    public OmikujiType OmikujiType => OmikujiType.EVENT_MISC;

    public OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.ANYTIME;

    public void execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Player wishing event");
        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            cl.PrintToChat(
                $"{Omikuji.ChatPrefix} {LupercaliaMGCore.getInstance().Localizer["Omikuji.MiscEvent.PlayerWishingEvent.Notification.Wishing", client.PlayerName]}");
        }
    }

    public void initialize()
    {
    }

    public double getOmikujiWeight()
    {
        return PluginSettings.GetInstance.m_CVOmikujiEventPlayerWishingSelectionWeight.Value;
    }
}