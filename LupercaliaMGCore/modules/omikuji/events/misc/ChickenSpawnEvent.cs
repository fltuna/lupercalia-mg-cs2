using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using TNCSSPluginFoundation.Utils.Entity;

namespace LupercaliaMGCore.modules.omikuji.events.misc;

public class ChickenSpawnEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Chicken Spawn Event";

    public override OmikujiType OmikujiType => OmikujiType.EventMisc;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.Anytime;

    
    public readonly FakeConVar<float> ChickenAliveTime =
        new("lp_mg_omikuji_event_chicken_time", "How long that chicken alive?", 5.0F);

    public readonly FakeConVar<float> ChickenBodyScale =
        new("lp_mg_omikuji_event_chicken_body_scale", "Body size of the chicken. Default size is 1.0", 1.0F);

    public readonly FakeConVar<double> EventSelectionWeight =
        new("lp_mg_omikuji_event_chicken_selection_weight", "Selection weight of this event", 160.0D);

    public override void Initialize()
    {
        TrackConVar(ChickenAliveTime);
        TrackConVar(ChickenBodyScale);
        TrackConVar(EventSelectionWeight);
    }

    public override void Execute(CCSPlayerController client)
    {
        DebugLogger.LogDebug("Player drew a omikuji and invoked Chicken spawn event");
        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            if (PlayerUtil.IsPlayerAlive(client))
                CreateGamingChicken(cl);
        }
        
        Server.PrintToChatAll(LocalizeWithPrefix(null, "Omikuji.MiscEvent.ChickenSpawnEvent.Notification.ChickenSpawned"));
    }

    public override double GetOmikujiWeight()
    {
        return EventSelectionWeight.Value;
    }

    private void CreateGamingChicken(CCSPlayerController client)
    {
        CChicken? ent = Utilities.CreateEntityByName<CChicken>("chicken");

        if (ent == null)
            return;

        ent.DispatchSpawn();
        ent.Teleport(client.PlayerPawn.Value!.AbsOrigin!);
        ent.CBodyComponent!.SceneNode!.GetSkeletonInstance().Scale = ChickenBodyScale.Value;

        double hue = 0.0;
        Plugin.AddTimer(0.01f, () =>
        {
            if (!ent.IsValid)
                return;

            if (hue >= 360.0)
                hue = 0.0;

            ent.RenderMode = RenderMode_t.kRenderTransColor;
            ent.Render = colorFromHsv(hue, 1, 1);
            Utilities.SetStateChanged(ent, "CBaseModelEntity", "m_clrRender");
            hue += 30.0F;
        }, TimerFlags.REPEAT);

        Plugin.AddTimer(ChickenAliveTime.Value, () =>
        {
            if (ent.IsValid)
                ent.AcceptInput("Break");
        });
    }

    private static Color colorFromHsv(double hue, double saturation, double brightness)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        brightness = brightness * 255;
        int v = Convert.ToInt32(brightness);
        int p = Convert.ToInt32(brightness * (1 - saturation));
        int q = Convert.ToInt32(brightness * (1 - f * saturation));
        int t = Convert.ToInt32(brightness * (1 - (1 - f) * saturation));

        if (hi == 0)
            return Color.FromArgb(255, v, t, p);
        else if (hi == 1)
            return Color.FromArgb(255, q, v, p);
        else if (hi == 2)
            return Color.FromArgb(255, p, v, t);
        else if (hi == 3)
            return Color.FromArgb(255, p, q, v);
        else if (hi == 4)
            return Color.FromArgb(255, t, p, v);
        else
            return Color.FromArgb(255, v, p, q);
    }
}