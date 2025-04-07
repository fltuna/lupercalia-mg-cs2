using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class NothingEvent(Omikuji omikuji, LupercaliaMGCore plugin) : OmikujiEventBase(omikuji, plugin)
{
    public override string EventName => "Nothing Event";

    public override OmikujiType OmikujiType => OmikujiType.EventMisc;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.Anytime;

    public override void Execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Nothing event");
    
        Server.PrintToChatAll($"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {Plugin.Localizer["Omikuji.MiscEvent.NothingEvent.Notification.ButNothingHappened"]}");
    }

    public override double GetOmikujiWeight()
    {
        return PluginSettings.m_CVOmikujiEventNothingSelectionWeight.Value;
    }
}