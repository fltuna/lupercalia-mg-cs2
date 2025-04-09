using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using LupercaliaMGCore.model;
using LupercaliaMGCore.modules;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class ScreenShakeEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Screen Shake Event";

    public override OmikujiType OmikujiType => OmikujiType.EventMisc;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.Anytime;

    public override void Execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Screen shake event");

        CEnvShake? shakeEnt = Utilities.CreateEntityByName<CEnvShake>("env_shake");
        if (shakeEnt == null)
        {
            Logger.LogError("Failed to create env_shake!");
            return;
        }

        shakeEnt.Spawnflags = 5U;
        shakeEnt.Amplitude = PluginSettings.m_CVOmikujiEventScreenShakeAmplitude.Value;
        shakeEnt.Duration = PluginSettings.m_CVOmikujiEventScreenShakeDuration.Value;
        shakeEnt.Frequency = PluginSettings.m_CVOmikujiEventScreenShakeFrequency.Value;

        shakeEnt.DispatchSpawn();

        shakeEnt.AcceptInput("StartShake");
        
        Server.PrintToChatAll(LocalizeWithPrefix("Omikuji.MiscEvent.ScreenShakeEvent.Notification.PrepareForImpact", client.PlayerName));

        shakeEnt.Remove();
    }

    public override double GetOmikujiWeight()
    {
        return PluginSettings.m_CVOmikujiEventScreenShakeSelectionWeight.Value;
    }
}