using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class PlayerWishingEvent(Omikuji omikuji, LupercaliaMGCore plugin) : OmikujiEventBase(omikuji, plugin)
{
    public override string EventName => "Player Wishing Event";

    public override OmikujiType OmikujiType => OmikujiType.EventMisc;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.Anytime;

    public override void Execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Player wishing event");
        
        Server.PrintToChatAll($"{Omikuji.ChatPrefix} {LupercaliaMGCore.getInstance().Localizer["Omikuji.MiscEvent.PlayerWishingEvent.Notification.Wishing", client.PlayerName]}");
    }

    public override double GetOmikujiWeight()
    {
        return PluginSettings.m_CVOmikujiEventPlayerWishingSelectionWeight.Value;
    }
}