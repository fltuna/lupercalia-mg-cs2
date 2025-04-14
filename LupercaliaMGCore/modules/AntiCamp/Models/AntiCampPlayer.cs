using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace LupercaliaMGCore.modules.AntiCamp.Models;

public class AntiCampPlayer: PluginBasicFeatureBase
{
    private AntiCampGlowingService GlowingService { get; }

    public CCSPlayerController Controller { get; }

    public PlayerPositionHistory PositionHistory { get; set; }

    public float CampingTime { get; set; } = 0.0F;
    
    public bool IsWarned { get; set; } = false;
    
    
    public float GlowingTime { get; set; }
    
    private AntiCampSettings AntiCampSettings { get; set; }


    public AntiCampPlayer(int playerSlot, IServiceProvider provider, PlayerPositionHistory positionHistory, AntiCampSettings antiCampSettings): base(provider)
    {
        Controller = Utilities.GetPlayerFromSlot(playerSlot)!;
        PositionHistory = positionHistory;
        AntiCampSettings = antiCampSettings;
        GlowingService = new AntiCampGlowingService(provider, Controller);
    }


    public bool CheckIsCamping()
    {
        if (Controller.Team is CsTeam.None or CsTeam.Spectator)
            return false;

        if (!PlayerUtil.IsPlayerAlive(Controller))
            return false;

        Vector clientOrigin = Controller.PlayerPawn.Value!.AbsOrigin!;
        
        PositionHistory.Update(new Vector(clientOrigin.X, clientOrigin.Y, clientOrigin.Z));
        
        TimedPosition oldestPosition = PositionHistory.GetOldestPosition();

        double distance = oldestPosition.Vector.Distance3D(clientOrigin);
        if (distance <= AntiCampSettings.DetectionRadius)
        {
            CampingTime += AntiCampSettings.DetectionInterval;
        }
        else
        {
            CampingTime = 0.0F;
        }

        // Check is player camping over the time.
        if (CampingTime >= AntiCampSettings.DetectionTime)
            return true;


        return false;
    }

    public void StartGlowing()
    {
        if(GlowingService.IsGlowing())
            return;
        
        GlowingTime = AntiCampSettings.GlowingTime;
        
        bool success = GlowingService.StartGlow();

        if (success)
        {
            DebugLogger.LogDebug($"[Anti Camp] [Player {Controller.PlayerName}]  [Player {{Controller.PlayerName}}] Glowing started");
        }
        else
        {
            DebugLogger.LogError($"[Anti Camp] [Player {Controller.PlayerName}] Start Glowing failed");
        }
    }

    public void StopGlowing()
    {
        if(!GlowingService.IsGlowing())
            return;
        
        GlowingTime = 0.0F;
        
        GlowingService.StopGlow();
        DebugLogger.LogDebug($"[Anti Camp] [Player {Controller.PlayerName}] Glowing stopped");
    }
    


    public void RecreateGlowingTimer()
    {
        void Check()
        {
            Plugin.AddTimer(AntiCampSettings.DetectionInterval, () =>
            {
                if (GlowingTime <= 0.0)
                {
                    IsWarned = false;
                    DebugLogger.LogDebug($"[Anti Camp] [Player {Controller.PlayerName}] Glowing timer has ended.");
                    StopGlowing();
                    return;
                }

                GlowingTime -= AntiCampSettings.DetectionInterval;
                Check();
            }, TimerFlags.STOP_ON_MAPCHANGE);
        }
        
        Check();
    }
}