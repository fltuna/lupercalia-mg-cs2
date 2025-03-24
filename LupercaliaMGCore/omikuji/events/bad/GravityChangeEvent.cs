using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class GravityChangeEvent : IOmikujiEvent
{
    public string EventName => "Gravity Change Event";

    public OmikujiType OmikujiType => OmikujiType.EVENT_BAD;

    public OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.ANYTIME;

    private static bool isInGravityChangeEvent = false;

    private static Random random = OmikujiEvents.random;

    public void execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Gravity change event");

        int randomGravity = random.Next(
            PluginSettings.GetInstance.m_CVOmikujiEventGravityMin.Value,
            PluginSettings.GetInstance.m_CVOmikujiEventGravityMax.Value
        );

        string msg;

        if (isInGravityChangeEvent)
        {
            msg =
                $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {LupercaliaMGCore.getInstance().Localizer["Omikuji.BadEvent.GravityChangeEvent.Notification.AnotherEventOnGoing"]}";
        }
        else
        {
            msg =
                $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {LupercaliaMGCore.getInstance().Localizer["Omikuji.BadEvent.GravityChangeEvent.Notification.GravityChanged", randomGravity]}";
        }

        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            cl.PrintToChat(msg);
        }

        if (isInGravityChangeEvent)
            return;

        isInGravityChangeEvent = true;
        ConVar? sv_gravity = ConVar.Find("sv_gravity");

        float oldGravity = sv_gravity!.GetPrimitiveValue<float>();

        sv_gravity.SetValue((float)randomGravity);

        float TIMER_INTERVAL_PLACE_HOLDER = PluginSettings.GetInstance.m_CVOmikujiEventGravityRestoreTime.Value;

        LupercaliaMGCore.getInstance().AddTimer(TIMER_INTERVAL_PLACE_HOLDER, () =>
        {
            sv_gravity.SetValue(oldGravity);
            foreach (CCSPlayerController cl in Utilities.GetPlayers())
            {
                if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                    continue;

                cl.PrintToChat(
                    $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {LupercaliaMGCore.getInstance().Localizer["Omikuji.BadEvent.GravityChangeEvent.Notification.GravityRestored", oldGravity]}");
                isInGravityChangeEvent = false;
            }
        });
    }

    public void initialize()
    {
    }

    public double getOmikujiWeight()
    {
        return PluginSettings.GetInstance.m_CVOmikujiEventGravitySelectionWeight.Value;
    }
}