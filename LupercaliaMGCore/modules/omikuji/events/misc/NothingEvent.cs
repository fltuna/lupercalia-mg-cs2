using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using LupercaliaMGCore.model;
using LupercaliaMGCore.modules;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class NothingEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Nothing Event";

    public override OmikujiType OmikujiType => OmikujiType.EventMisc;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.Anytime;

    public override void Execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Nothing event");
    
        Server.PrintToChatAll(LocalizeOmikujiResult(client, OmikujiType, "Omikuji.MiscEvent.NothingEvent.Notification.ButNothingHappened"));
    }

    public override double GetOmikujiWeight()
    {
        return PluginSettings.m_CVOmikujiEventNothingSelectionWeight.Value;
    }
}