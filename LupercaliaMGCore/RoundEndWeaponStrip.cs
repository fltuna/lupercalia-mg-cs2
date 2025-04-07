using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore;

public class RoundEndWeaponStrip(LupercaliaMGCore plugin) : PluginModuleBase(plugin)
{
    public override string PluginModuleName => "RoundEndWeaponStrip";

    public override void Initialize()
    {
        Plugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart, HookMode.Pre);
    }

    public override void UnloadModule()
    {
        Plugin.DeregisterEventHandler<EventRoundPrestart>(OnRoundPreStart, HookMode.Pre);
    }

    private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info)
    {
        if (!PluginSettings.m_CVIsRoundEndWeaponStripEnabled.Value)
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