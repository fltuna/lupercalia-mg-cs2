using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using LupercaliaMGCore.model;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore.modules;

public class ScheduledShutdown(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "ScheduledShutdown";
    
    public override string ModuleChatPrefix => "[ScheduledShutdown]";

    private CounterStrikeSharp.API.Modules.Timers.Timer shutdownTimer = null!;
    private CounterStrikeSharp.API.Modules.Timers.Timer? warningTimer;
    private bool shutdownAfterRoundEnd = false;

    protected override void OnInitialize()
    {
        Plugin.AddCommand("css_cancelshutdown", "Cancel the initiated shutdown.", CommandCancelShutdown);
        Plugin.AddCommand("css_startshutdown", "Initiate the shutdown.", CommandStartShutdown);

        Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        shutdownTimer = Plugin.AddTimer(60.0f, () =>
        {
            if (DateTime.Now.ToString("HHmm").Equals(PluginSettings.m_CVScheduledShutdownTime.Value))
            {
                initiateShutdown();
            }
        }, TimerFlags.REPEAT);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_cancelshutdown", CommandCancelShutdown);
        Plugin.RemoveCommand("css_startshutdown", CommandStartShutdown);
        Plugin.DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);

        shutdownTimer.Kill();
        warningTimer?.Kill();
    }

    private void initiateShutdown()
    {
        shutdownTimer.Kill();
        
        if (PluginSettings.m_CVScheduledShutdownRoundEnd.Value)
        {
            shutdownAfterRoundEnd = true;
            PrintLocalizedChatToAll("ScheduledShutdown.Notification.AfterRoundEnd");
        }
        else
        {
            int time = PluginSettings.m_CVScheduledShutdownWarningTime.Value;
            warningTimer = Plugin.AddTimer(1.0F, () =>
            {
                if (time < 1)
                {
                    Plugin.Logger.LogInformation("Server is shutting down...");
                    Server.ExecuteCommand("quit");
                    return;
                }

                PrintLocalizedChatToAll("ScheduledShutdown.Notification.Countdown", time);
                time--;
            }, TimerFlags.REPEAT);
        }

        SimpleLogging.LogDebug($"[Scheduled Shutdown] Shutdown initiated.");
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (!shutdownAfterRoundEnd)
            return HookResult.Continue;

        Logger.LogInformation("Server is shutting down...");
        Server.ExecuteCommand("quit");


        return HookResult.Continue;
    }

    private void cancelShutdown()
    {
        shutdownAfterRoundEnd = false;
        shutdownTimer.Kill();
        warningTimer?.Kill();

        shutdownTimer = Plugin.AddTimer(60.0f, () =>
        {
            if (DateTime.Now.ToString("HHmm").Equals(PluginSettings.m_CVScheduledShutdownTime.Value))
            {
                initiateShutdown();
            }
        }, TimerFlags.REPEAT);
        SimpleLogging.LogDebug($"[Scheduled Shutdown] Cancelled shutdown.");
    }

    [RequiresPermissions(@"css/root")]
    private void CommandCancelShutdown(CCSPlayerController? client, CommandInfo info)
    {
        string executorName = PlayerUtil.GetPlayerName(client);

        cancelShutdown();
        PrintLocalizedChatToAll("ScheduledShutdown.Notification.CancelShutdown", executorName);
    }

    [RequiresPermissions(@"css/root")]
    private void CommandStartShutdown(CCSPlayerController? client, CommandInfo info)
    {
        string executorName = PlayerUtil.GetPlayerName(client);

        initiateShutdown();
        Server.PrintToChatAll(LocalizeWithPluginPrefix("ScheduledShutdown.Notification.InitiateShutdown", executorName));
    }
}