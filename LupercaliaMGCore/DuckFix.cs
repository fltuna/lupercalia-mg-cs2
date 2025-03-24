using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore;

public class DuckFix : IPluginModule
{
    private LupercaliaMGCore m_CSSPlugin;

    public string PluginModuleName => "DuckFix";

    public DuckFix(LupercaliaMGCore plugin, bool hotReload)
    {
        m_CSSPlugin = plugin;
        m_CSSPlugin.RegisterListener<Listeners.OnTick>(OnTickListener);
    }

    public void AllPluginsLoaded()
    {
    }

    public void UnloadModule()
    {
        m_CSSPlugin.RemoveListener<Listeners.OnTick>(OnTickListener);
    }

    private void OnTickListener()
    {
        foreach (CCSPlayerController client in Utilities.GetPlayers())
        {
            if (!client.IsValid || client.IsBot || client.IsHLTV)
                continue;

            if ((client.Buttons & PlayerButtons.Duck) == 0)
                continue;

            CCSPlayerPawn? playerPawn = client.PlayerPawn.Value;

            if (playerPawn == null)
                continue;

            CPlayer_MovementServices? pmService = playerPawn.MovementServices;

            if (pmService == null)
                continue;

            CCSPlayer_MovementServices movementServices = new CCSPlayer_MovementServices(pmService.Handle);
            if (movementServices != null)
            {
                movementServices.LastDuckTime = 0.0f;
                movementServices.DuckSpeed = 8.0f;
            }
        }
    }
}