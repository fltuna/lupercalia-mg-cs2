using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public static partial class OmikujiEvents
{
    private static float STATIC_PLACE_HOLDER = 5.0F;

    // [OmikujiFunc("Player movement invert event", OmikujiType.EVENT_BAD, OmikujiCanInvokeWhen.PLAYER_ALIVE)]
    public static void playerMovementInvertEvent(CCSPlayerController client)
    {
        isPlayerMovementInverted[client] = true;

        LupercaliaMGCore.getInstance()
            .AddTimer(STATIC_PLACE_HOLDER, () => { isPlayerMovementInverted[client] = false; });
    }


    private static Dictionary<CCSPlayerController, bool> isPlayerMovementInverted =
        new Dictionary<CCSPlayerController, bool>();

    // [OmikujiInitilizerFunc]
    private static void initPlayerMovementInvertEventListeners()
    {
        var plugin = LupercaliaMGCore.getInstance();

        SimpleLogging.LogDebug("Player Movement Invert Event initializer called");
        plugin.RegisterListener<Listeners.OnMapStart>(mapName =>
        {
            foreach (CCSPlayerController cl in Utilities.GetPlayers())
            {
                if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                    continue;

                isPlayerMovementInverted[cl] = false;
            }
        });

        SimpleLogging.LogDebug("Initialize cmd hook");

        MemoryFunctionVoid<CCSPlayer_MovementServices, IntPtr> runCmd =
            new("\x48\x8B\xC4\x44\x88\x48\x20\x44\x89\x40\x18\x48\x89\x50\x10\x56");

        runCmd.Hook(onRunCmd, HookMode.Pre);


        SimpleLogging.LogDebug("Initialize PlayerConnect Event Handler");

        plugin.RegisterEventHandler<EventPlayerConnect>((@event, info) =>
        {
            CCSPlayerController? client = @event.Userid;
            if (client == null)
                return HookResult.Continue;

            isPlayerMovementInverted[client] = false;

            return HookResult.Continue;
        }, HookMode.Post);

        SimpleLogging.LogDebug("Player Movement Invert Event initialized");
    }

    private static HookResult onRunCmd(DynamicHook hook)
    {
        CCSPlayerController? client = hook.GetParam<CCSPlayer_MovementServices>(0).Pawn.Value.Controller.Value
            ?.As<CCSPlayerController>();

        if (client == null || !client.IsValid || client.IsBot || client.IsHLTV)
            return HookResult.Continue;

        // TODO obtain buttons from hook

        return HookResult.Continue;
    }
}