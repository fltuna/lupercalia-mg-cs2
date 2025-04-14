using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using LupercaliaMGCore.modules.AntiCamp.Models;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace LupercaliaMGCore.modules.AntiCamp;

public sealed class AntiCampModule(IServiceProvider serviceProvider, bool hotReload) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "AntiCamp";

    public override string ModuleChatPrefix => "[AntiCamp]";

    private Timer timer = null!;

    private readonly Dictionary<int, AntiCampPlayer> players = new();

    private bool isRoundStarted = false;
    
    private AntiCampSettings settings = null!;

    
    public readonly FakeConVar<bool> IsModuleEnabled = new("lp_mg_anti_camp_enabled",
        "Anti camp enabled", true);

    public readonly FakeConVar<float> CampDetectionTime = new("lp_mg_anti_camp_detection_time",
        "How long to take detected as camping in seconds.", 10.0F);

    public readonly FakeConVar<double> CampDetectionRadius = new("lp_mg_anti_camp_detection_radius",
        "Range of area for player should move for avoiding the detected as camping.", 200.0F);

    public readonly FakeConVar<float> CampDetectionInterval = new("lp_mg_anti_camp_detection_interval",
        "Interval to run camping check in seconds.", 0.1F);

    public readonly FakeConVar<float> CampMarkingTime = new("lp_mg_anti_camp_glowing_time",
        "How long to detected player keep glowing.", 10.0F);
    
    protected override void OnInitialize()
    {
        TrackConVar(IsModuleEnabled);
        TrackConVar(CampDetectionTime);
        TrackConVar(CampDetectionRadius);
        TrackConVar(CampDetectionInterval);
        TrackConVar(CampMarkingTime);

        Plugin.RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
        Plugin.RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);

        Plugin.RegisterEventHandler<EventRoundFreezeEnd>(OnRoundFreezeEnd, HookMode.Post);
        Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd, HookMode.Post);
        Plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath, HookMode.Post);
        Plugin.RegisterEventHandler<EventPlayerTeam>(OnSwitchTeam, HookMode.Post);

        settings = new AntiCampSettings()
        {
            DetectionInterval = CampDetectionInterval.Value,
            DetectionRadius = CampDetectionRadius.Value,
            DetectionTime = CampDetectionTime.Value,
            GlowingTime = CampMarkingTime.Value,
        };

        if (hotReload)
        {
            Logger.LogWarning("[AntiCamp] This module is hot-reloaded! Unknown problems may occur!");
            bool isFreezeTimeEnded = EntityUtil.GetGameRules()?.FreezePeriod ?? false;
            
            if (!isFreezeTimeEnded)
            {
                isRoundStarted = true;
            }
            
            foreach (CCSPlayerController cl in Utilities.GetPlayers())
            {
                if (cl.IsHLTV)
                    continue;

                CreateClientInformation(cl.Slot);
            }
        }

        timer = Plugin.AddTimer(CampDetectionInterval.Value, CheckPlayerIsCamping, TimerFlags.REPEAT);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
        Plugin.RemoveListener<Listeners.OnClientDisconnect>(OnClientDisconnect);

        Plugin.DeregisterEventHandler<EventRoundFreezeEnd>(OnRoundFreezeEnd, HookMode.Post);
        Plugin.DeregisterEventHandler<EventRoundEnd>(OnRoundEnd, HookMode.Post);
        Plugin.DeregisterEventHandler<EventPlayerDeath>(OnPlayerDeath, HookMode.Post);
        Plugin.DeregisterEventHandler<EventPlayerTeam>(OnSwitchTeam, HookMode.Post);
        timer.Kill();
        
        foreach (var (key, value) in players)
        {
            DisposeClientInformation(key);
        }
    }

    private void CheckPlayerIsCamping()
    {
        if (!isRoundStarted || !IsModuleEnabled.Value)
            return;

        foreach (var (key, value) in players)
        {
            bool isCamping = value.CheckIsCamping();

            if (!isCamping)
                continue;

            if (value.GlowingTime <= 0.0 && !value.IsWarned)
            {
                // Start player glowing
                // and recreate glowing timer
                value.StartGlowing();
                value.RecreateGlowingTimer();

                if (!value.Controller.IsBot)
                {
                    using var tempLang = new WithTemporaryCulture(PlayerLanguageManager.Instance.GetLanguage((SteamID)value.Controller.SteamID));
                    value.Controller.PrintToCenterAlert(Plugin.Localizer["AntiCamp.Notification.DetectedAsCamping"]);
                }
                
                value.IsWarned = true;
            }

            value.GlowingTime = settings.GlowingTime;
        }
    }

    private HookResult OnSwitchTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        CCSPlayerController? client = @event.Userid;

        if (client == null)
            return HookResult.Continue;

        if (!client.IsValid || /*client.IsBot ||*/ client.IsHLTV)
            return HookResult.Continue;
        
        if (client.Connected != PlayerConnectedState.PlayerConnected)
            return HookResult.Continue;

        players[client.Slot].StopGlowing();

        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? client = @event.Userid;

        if (client == null)
            return HookResult.Continue;

        if (!client.IsValid || /*client.IsBot ||*/ client.IsHLTV)
            return HookResult.Continue;
        
        if (client.Connected != PlayerConnectedState.PlayerConnected)
            return HookResult.Continue;

        players[client.Slot].StopGlowing();

        return HookResult.Continue;
    }

    private HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
    {
        isRoundStarted = true;
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        isRoundStarted = false;
        
        foreach (var (key, value) in players)
        {
            value.StopGlowing();
        }

        settings.GlowingTime = CampMarkingTime.Value;
        settings.DetectionRadius = CampDetectionRadius.Value;

        
        if (Math.Abs(settings.DetectionInterval - CampDetectionInterval.Value) > 0 ||
            Math.Abs(settings.DetectionInterval - CampDetectionInterval.Value) < 0)
        {
            settings.DetectionInterval = CampDetectionInterval.Value;

            foreach (var (key, value) in players)
            {
                var history = new PlayerPositionHistory((int)(CampDetectionTime.Value / CampDetectionInterval.Value));
                value.PositionHistory = history;
            }
        }

        if (Math.Abs(settings.DetectionTime - CampDetectionTime.Value) > 0 ||
            Math.Abs(settings.DetectionTime - CampDetectionTime.Value) < 0)
        {
            settings.DetectionTime = CampDetectionTime.Value;

            foreach (var (key, value) in players)
            {
                var history = new PlayerPositionHistory((int)(CampDetectionTime.Value / CampDetectionInterval.Value));
                value.PositionHistory = history;
            }
        }
        
        return HookResult.Continue;
    }

    private void OnClientPutInServer(int slot)
    {
        CreateClientInformation(slot);
    }

    private void OnClientDisconnect(int slot)
    {
        DisposeClientInformation(slot);
    }


    private void CreateClientInformation(int slot)
    {
        var history = new PlayerPositionHistory((int)(CampDetectionTime.Value / CampDetectionInterval.Value));
        players[slot] = new AntiCampPlayer(slot, ServiceProvider, history, settings);
    }

    private void DisposeClientInformation(int slot)
    {
        AntiCampPlayer player = players[slot];
        
        player.StopGlowing();
        player.CampingTime = 0.0F;
        player.GlowingTime = 0.0F;
        players.Remove(slot);
    }
}