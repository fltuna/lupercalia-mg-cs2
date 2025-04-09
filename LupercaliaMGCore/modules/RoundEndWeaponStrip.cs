using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore.modules;

public class RoundEndWeaponStrip(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "RoundEndWeaponStrip";

    public override string ModuleChatPrefix => "[RoundEndWeaponStrip]";

    
    public readonly FakeConVar<bool> IsModuleEnabled = new("lp_mg_rews_enabled",
        "Should player's weapons are removed before new round starts.", true);
    
    protected override void OnInitialize()
    {
        TrackConVar(IsModuleEnabled);
        
        Plugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart, HookMode.Pre);
    }

    protected override void OnUnloadModule()
    {
        Plugin.DeregisterEventHandler<EventRoundPrestart>(OnRoundPreStart, HookMode.Pre);
    }

    private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info)
    {
        if (!IsModuleEnabled.Value)
            return HookResult.Continue;

        SimpleLogging.LogDebug("[Round End Weapon Strip] Removing all players weapons.");
        foreach (var player in Utilities.GetPlayers())
        {
            if (player.IsBot || player.IsHLTV)
                continue;

            player.RemoveWeapons();
        }

        SimpleLogging.LogDebug("[Round End Weapon Strip] Done.");
        return HookResult.Continue;
    }
}