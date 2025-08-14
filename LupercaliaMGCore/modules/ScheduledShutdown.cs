using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace LupercaliaMGCore.modules;

public sealed class ScheduledShutdown(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "ScheduledShutdown";
    
    public override string ModuleChatPrefix => "[ScheduledShutdown]";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private Timer shutdownTimer = null!;
    private Timer? warningTimer;
    private bool shutdownAfterRoundEnd = false;
    
    public readonly FakeConVar<string> ShutdownTime = new("lp_mg_scheduled_shutdown_time",
        "Server will be shutdown in specified time. Format is HHmm", "0500");

    public readonly FakeConVar<int> ShutdownWarningTime = new("lp_mg_scheduled_shutdown_warn_time",
        "Show shutdown warning countdown if lp_mg_scheduled_shutdown_round_end is false.", 10);

    public readonly FakeConVar<bool> ShouldShutdownAfterRoundEnd = new("lp_mg_scheduled_shutdown_round_end",
        "When set to true server will be shutdown after round end.", true);

    private const float TimeCheckInterval = 30.0F;
    
    protected override void OnInitialize()
    {
        TrackConVar(ShutdownTime);
        TrackConVar(ShutdownWarningTime);
        TrackConVar(ShouldShutdownAfterRoundEnd);
        
        Plugin.AddCommand("css_cancelshutdown", "Cancel the initiated shutdown.", CommandCancelShutdown);
        Plugin.AddCommand("css_startshutdown", "Initiate the shutdown.", CommandStartShutdown);

        Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        shutdownTimer = Plugin.AddTimer(TimeCheckInterval, () =>
        {
            if (DateTime.Now.ToString("HHmm").Equals(ShutdownTime.Value, StringComparison.InvariantCulture))
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
        
        if (ShouldShutdownAfterRoundEnd.Value)
        {
            shutdownAfterRoundEnd = true;
            PrintLocalizedChatToAll("ScheduledShutdown.Notification.AfterRoundEnd");
        }
        else
        {
            int time = ShutdownWarningTime.Value;
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

        DebugLogger.LogDebug($"[Scheduled Shutdown] Shutdown initiated.");
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

        shutdownTimer = Plugin.AddTimer(TimeCheckInterval, () =>
        {
            if (DateTime.Now.ToString("HHmm").Equals(ShutdownTime.Value))
            {
                initiateShutdown();
            }
        }, TimerFlags.REPEAT);
        DebugLogger.LogDebug($"[Scheduled Shutdown] Cancelled shutdown.");
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
        Server.PrintToChatAll(LocalizeWithPluginPrefix(client, "ScheduledShutdown.Notification.InitiateShutdown", executorName));
    }
}