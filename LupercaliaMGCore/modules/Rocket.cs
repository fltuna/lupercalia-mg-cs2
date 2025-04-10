using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using TNCSSPluginFoundation.Models.Plugin;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace LupercaliaMGCore.modules;

public sealed class Rocket(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "Rocket";

    public override string ModuleChatPrefix => $" {ChatColors.Gold}[Rocket]{ChatColors.Default}";

    private Dictionary<CCSPlayerController, bool> isRocketLaunched = new Dictionary<CCSPlayerController, bool>();

    
    public readonly FakeConVar<bool> IsModuleEnabled =
        new("lp_mg_rocket", "Is rocket command enabled?", false);
    
    protected override void OnInitialize()
    {
        Plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        Plugin.AddCommand("css_rocket", "launch to space.", CommandRocket);

        Plugin.AddTimer(0.1F, () =>
        {
            DebugLogger.LogDebug("Late initialization for hot reloading rocket.");
            foreach (CCSPlayerController client in Utilities.GetPlayers())
            {
                if (!client.IsValid || client.IsBot || client.IsHLTV)
                    continue;

                isRocketLaunched[client] = false;
            }
        });
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_rocket", CommandRocket);
        Plugin.DeregisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);

        isRocketLaunched.Clear();
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? client = @event.Userid;

        if (client == null)
            return HookResult.Continue;

        isRocketLaunched[client] = false;
        return HookResult.Continue;
    }

    private void CommandRocket(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null || !PlayerUtil.IsPlayerAlive(client) || isRocketLaunched[client])
            return;
        
        if (!IsModuleEnabled.Value)
            return;

        RocketPerform(client);
    }

    private void RocketPerform(CCSPlayerController client)
    {
        if (!PlayerUtil.IsPlayerAlive(client) || isRocketLaunched[client]) return;

        CBasePlayerPawn pawn = client.Pawn.Value!;

        Server.PrintToChatAll($" {ChatColors.Lime} {client.PlayerName} {ChatColors.Default}lancuhed to space!");

        var vel = new Vector(0.0f, 0.0f, 350f);
        pawn.EmitSound("C4.ExplodeWarning");
        pawn.Teleport(null, null, vel);
        pawn.GravityScale = 0.1f;

        isRocketLaunched[client] = true;

        Timer timer = new Timer(2.0f, () =>
        {
            if (!PlayerUtil.IsPlayerAlive(client)) return;

            pawn.GravityScale = 1.0f;
            pawn.CommitSuicide(false, true);
            pawn.EmitSound("c4.explode");
            MakeExplosive(client);
            isRocketLaunched[client] = false;
        });
    }

    private void MakeExplosive(CCSPlayerController client)
    {
        if (!PlayerUtil.IsPlayerAlive(client)) return;

        CBasePlayerPawn pawn = client.Pawn.Value!;
        var pEffect = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;
        var vecOrign = pawn.AbsOrigin!;

        pEffect.EffectName = "particles/explosions_fx/explosion_basic.vpcf";
        pEffect.AbsOrigin!.X = vecOrign.X;
        pEffect.AbsOrigin.Y = vecOrign.Y;
        pEffect.AbsOrigin.Z = vecOrign.Z + 10;
        pEffect.StartActive = true;
        pEffect.DispatchSpawn();

        pEffect.AddEntityIOEvent("kill", null, null, "", 2.0f);
    }
}
