using CounterStrikeSharp.API.Core;

namespace LupercaliaMGCore;

public static class PlayerUtil
{
    public static bool IsPlayerAlive(CCSPlayerController? client)
    {
        if (client == null)
            return false;
        
        var playerPawn = client.PlayerPawn.Value;
        
        if (playerPawn == null)
            return false;
        
        return playerPawn.LifeState == (byte)LifeState_t.LIFE_ALIVE;
    }
}