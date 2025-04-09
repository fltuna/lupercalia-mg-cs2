using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore.modules;

public class DuckFix(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "DuckFix";

    public override string ModuleChatPrefix => "[DuckFix]";

    protected override void OnInitialize()
    {
        Plugin.RegisterListener<Listeners.OnTick>(OnTickListener);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveListener<Listeners.OnTick>(OnTickListener);
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

            CCSPlayer_MovementServices unused = new CCSPlayer_MovementServices(pmService.Handle)
            {
                LastDuckTime = 0.0f,
                DuckSpeed = 8.0f
            };
        }
    }
}