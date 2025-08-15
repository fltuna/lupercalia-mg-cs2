using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore.modules.omikuji.events.misc;

public class ScreenShakeEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Screen Shake Event";

    public override OmikujiType OmikujiType => OmikujiType.EventMisc;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.Anytime;

    
    public readonly FakeConVar<float> ScreenShakeAmplitude = new(
        "lp_mg_omikuji_event_screen_shake_amplitude",
        "How far away from the normal position the camera will wobble. Should be a range between 0 and 16.",
        1000.0F);

    public readonly FakeConVar<float> ShakeDuration = new(
        "lp_mg_omikuji_event_screen_shake_duration", "The length of time in which to shake the player's screens.",
        5.0F);

    public readonly FakeConVar<float> ShakeFrequency = new(
        "lp_mg_omikuji_event_screen_shake_frequency",
        "How many times per second to change the direction of the camera wobble. 40 is generally enough; values higher are hardly distinguishable.",
        1000.0F);

    public readonly FakeConVar<double> EventSelectionWeight =
        new("lp_mg_omikuji_event_screen_shake_selection_weight", "Selection weight of this event", 30.0D);

    public override void Initialize()
    {
        TrackConVar(ScreenShakeAmplitude);
        TrackConVar(ShakeDuration);
        TrackConVar(ShakeFrequency);
        TrackConVar(EventSelectionWeight);
    }


    public override void Execute(CCSPlayerController client)
    {
        DebugLogger.LogDebug("Player drew a omikuji and invoked Screen shake event");

        CEnvShake? shakeEnt = Utilities.CreateEntityByName<CEnvShake>("env_shake");
        if (shakeEnt == null)
        {
            Logger.LogError("Failed to create env_shake!");
            return;
        }

        shakeEnt.Spawnflags = 5U;
        shakeEnt.Amplitude = ScreenShakeAmplitude.Value;
        shakeEnt.Duration = ShakeDuration.Value;
        shakeEnt.Frequency = ShakeFrequency.Value;

        shakeEnt.DispatchSpawn();

        shakeEnt.AcceptInput("StartShake");
        
        Server.PrintToChatAll(LocalizeWithPrefix(null, "Omikuji.MiscEvent.ScreenShakeEvent.Notification.PrepareForImpact", client.PlayerName));

        shakeEnt.Remove();
    }

    public override double GetOmikujiWeight()
    {
        return EventSelectionWeight.Value;
    }
}