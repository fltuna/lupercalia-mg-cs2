using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Cvars;
using LupercaliaMGCore.interfaces;

namespace LupercaliaMGCore.model;

public abstract class AbstractDebugLogger: IDebugLogger
{
    public abstract int DebugLogLevel { get; }
    
    public abstract bool PrintToAdminClientsConsole { get; }

    public abstract string RequiredFlagForPrintToConsole { get; }
    
    public abstract string LogPrefix { get; }
    
    public void LogInformation(string information)
    {
        if (DebugLogLevel < 1)
            return;
        
        PrintInformation("[INFO] ", information);
    }

    public void LogWarning(string information)
    {
        if (DebugLogLevel < 1)
            return;
        
        PrintInformation("[WARN] ", information);
    }

    public void LogError(string information)
    {
        if (DebugLogLevel < 1)
            return;
        
        PrintInformation("[ERROR] ", information);
    }

    public void LogDebug(string information)
    {
        if (DebugLogLevel < 2)
            return;
        
        PrintInformation("[DEBUG] ", information);
    }

    public void LogTrace(string information)
    {
        if (DebugLogLevel < 3)
            return;
        
        PrintInformation("[TRACE] ", information);
    }


    private void PrintInformation(string debugLevelPrefix ,string information)
    {
        string msg = $"{LogPrefix} {debugLevelPrefix} {information}";
        
        Server.PrintToConsole(msg);
        
        if (!PrintToAdminClientsConsole)
            return;
        
        foreach (var client in Utilities.GetPlayers())
        {
            if (client.IsBot || client.IsHLTV)
                continue;
            
            if (!AdminManager.PlayerHasPermissions(client, RequiredFlagForPrintToConsole))
                continue;

            client.PrintToConsole(msg);
        }
    }
}