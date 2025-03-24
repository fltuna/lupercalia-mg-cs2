using CounterStrikeSharp.API;

namespace LupercaliaMGCore;

public static class SimpleLogging
{
    public static void LogDebug(string information)
    {
        if (PluginSettings.GetInstance.m_CVPluginDebugLevel.Value < 1)
        {
            return;
        }

        if (PluginSettings.GetInstance.m_CVPluginDebugShowClientConsole.Value)
        {
            foreach (var client in Utilities.GetPlayers())
            {
                if (!client.IsValid || client.IsBot || client.IsHLTV)
                    continue;

                client.PrintToConsole("[LPR MG DEBUG] " + information);
            }
        }

        Server.PrintToConsole("[LPR MG DEBUG] " + information);
    }

    public static void LogTrace(string information)
    {
        if (PluginSettings.GetInstance.m_CVPluginDebugLevel.Value < 2)
        {
            return;
        }

        if (PluginSettings.GetInstance.m_CVPluginDebugShowClientConsole.Value)
        {
            foreach (var client in Utilities.GetPlayers())
            {
                if (!client.IsValid || client.IsBot || client.IsHLTV)
                    continue;

                client.PrintToConsole("[LPR MG TRACE] " + information);
            }
        }

        Server.PrintToConsole("[LPR MG TRACE] " + information);
    }
}