using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System.Drawing;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Admin;
using LupercaliaMGCore.model;
using Microsoft.Extensions.Logging;
using NativeVoteAPI;
using NativeVoteAPI.API;

namespace LupercaliaMGCore;

public class VoteRoundRestart(LupercaliaMGCore plugin) : PluginModuleBase(plugin)
{
    private bool isRoundRestarting = false;
    
    private INativeVoteApi? nativeVoteApi;

    public override string PluginModuleName => "VoteRoundRestart";

    private const string NativeVoteIdentifier = "LupercaliaMGCore:VoteRoundRestart";

    public override void Initialize()
    {
        Plugin.AddCommand("css_vrr", "Vote round restart command.", CommandVoteRestartRound);
        Plugin.AddCommand("css_restart", "Restarts round (admins only)", CommandForceRestartRound);
        Plugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
    }

    public override void AllPluginsLoaded()
    {
        nativeVoteApi = Plugin.GetNativeVoteApi();

        if (nativeVoteApi == null)
        {
            Logger.LogError("[VoteMapRestart] Failed to get native vote api.");
            UnloadModule();
            return;
        }


        nativeVoteApi.OnVotePass += OnVotePass;
        nativeVoteApi.OnVoteFail += OnVoteFail;
    }

    public override void UnloadModule()
    {
        if (nativeVoteApi != null)
        {
            nativeVoteApi.OnVotePass -= OnVotePass;
            nativeVoteApi.OnVoteFail -= OnVoteFail; 
        }
        
        
        Plugin.RemoveCommand("css_vrr", CommandVoteRestartRound);
        Plugin.RemoveCommand("css_restart", CommandForceRestartRound);
        Plugin.DeregisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
    }

    private void OnVotePass(YesNoVoteInfo? info)
    {
        if (info == null)
            return;

        if (info.VoteInfo.voteIdentifier != NativeVoteIdentifier)
            return;

        InitiateRoundRestart();
    }

    private void OnVoteFail(YesNoVoteInfo? info)
    {
        if (info == null)
            return;

        if (info.VoteInfo.voteIdentifier != NativeVoteIdentifier)
            return;

        PrintLocalizedChatToAll("VoteRoundRestart.Notification.VoteFailed");
    }


    private void CommandVoteRestartRound(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] trying to vote for restart round.");
        
        if (isRoundRestarting)
        {
            SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] Round is already restarting in progress.");
            
            client.PrintToChat(LocalizeWithPrefix("VoteRoundRestart.Command.Notification.AlreadyRestarting"));
            return;
        }

        if (nativeVoteApi!.GetCurrentVoteState() != NativeVoteState.NoActiveVote)
        {
            SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] Already an active vote.");
            client.PrintToChat(LocalizeWithPrefix("General.Command.Vote.Notification.AnotherVoteInProgress"));
            return;
        }

        var potentialClients = Utilities.GetPlayers().Where(p => p is { IsBot: false, IsHLTV: false }).ToList();
        var potentialClientsIndex = potentialClients.Select(p => p.Index).ToList();

        string detailsString =
            NativeVoteTextUtil.GenerateReadableNativeVoteText(Plugin.Localizer["VoteRoundRestart.Vote.SubjectText"]);

        NativeVoteInfo nInfo = new NativeVoteInfo(NativeVoteIdentifier, NativeVoteTextUtil.VoteDisplayString,
            detailsString, potentialClientsIndex, VoteThresholdType.Percentage,
            PluginSettings.m_CVVoteMapRestartThreshold.Value, 15.0f, initiator: client.Slot);

        NativeVoteState state = nativeVoteApi!.InitiateVote(nInfo);


        if (state == NativeVoteState.InitializeAccepted)
        {
            SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] Round restart vote initiated. Vote Identifier: {nInfo.voteIdentifier}");
            
            PrintLocalizedChatToAll("VoteRoundRestart.Notification.VoteInitiated");
        }
        else
        {
            client.PrintToChat(LocalizeWithPrefix("General.Command.Vote.Notification.FailedToInitiate"));
        }
    }

    [RequiresPermissions(@"css/root")]
    private void CommandForceRestartRound(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        SimpleLogging.LogDebug("[Force Round Restart] Initiating round restart...");
        isRoundRestarting = true;

        PrintLocalizedChatToAll("VoteRoundRestart.Notification.ForceRoundRestart", 1);
        
        Plugin.AddTimer(1, () =>
        {
            SimpleLogging.LogDebug("[Vote Round Restart] Restarting round.");
            EntityUtil.GetGameRules()?.TerminateRound(0.0F, RoundEndReason.RoundDraw);
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void InitiateRoundRestart()
    {
        SimpleLogging.LogDebug("[Vote Round Restart] Initiating round restart...");
        isRoundRestarting = true;

        float roundRestartTime = PluginSettings.m_CVVoteRoundRestartRestartTime.Value;
        PrintLocalizedChatToAll("VoteRoundRestart.Notification.RoundRestart", roundRestartTime);
        
        Plugin.AddTimer(PluginSettings.m_CVVoteRoundRestartRestartTime.Value, () =>
        {
            SimpleLogging.LogDebug("[Vote Round Restart] Restarting round.");
            EntityUtil.GetGameRules()?.TerminateRound(0.0F, RoundEndReason.RoundDraw);
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }


    private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info)
    {
        isRoundRestarting = false;
        return HookResult.Continue;
    }
}