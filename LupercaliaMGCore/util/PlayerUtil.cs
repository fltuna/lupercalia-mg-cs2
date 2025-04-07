using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

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
    
    public static readonly string ServerConsoleName = $" {ChatColors.DarkRed}CONSOLE{ChatColors.Default}";
    
    public static string GetPlayerName(CCSPlayerController? client)
    {
        if (client == null)
            return ServerConsoleName;
        
        return client.PlayerName;
    }
}