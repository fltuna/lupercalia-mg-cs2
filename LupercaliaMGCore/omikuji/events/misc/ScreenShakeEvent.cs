using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class ScreenShakeEvent : IOmikujiEvent
{
    public string EventName => "Screen Shake Event";

    public OmikujiType OmikujiType => OmikujiType.EVENT_MISC;

    public OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.ANYTIME;

    public void execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Screen shake event");

        CEnvShake? shakeEnt = Utilities.CreateEntityByName<CEnvShake>("env_shake");
        if (shakeEnt == null)
        {
            LupercaliaMGCore.getInstance().Logger.LogError("Failed to create env_shake!");
            return;
        }

        shakeEnt.Spawnflags = 5U;
        shakeEnt.Amplitude = PluginSettings.GetInstance.m_CVOmikujiEventScreenShakeAmplitude.Value;
        shakeEnt.Duration = PluginSettings.GetInstance.m_CVOmikujiEventScreenShakeDuration.Value;
        shakeEnt.Frequency = PluginSettings.GetInstance.m_CVOmikujiEventScreenShakeFrequency.Value;

        shakeEnt.DispatchSpawn();

        shakeEnt.AcceptInput("StartShake");

        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            cl.PrintToChat(
                $"{Omikuji.ChatPrefix} {LupercaliaMGCore.getInstance().Localizer["Omikuji.MiscEvent.ScreenShakeEvent.Notification.PrepareForImpact", client.PlayerName]}");
        }

        shakeEnt.Remove();
    }

    public void initialize()
    {
    }

    public double getOmikujiWeight()
    {
        return PluginSettings.GetInstance.m_CVOmikujiEventScreenShakeSelectionWeight.Value;
    }
}