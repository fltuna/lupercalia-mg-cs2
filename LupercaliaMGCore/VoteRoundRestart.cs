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

public class VoteRoundRestart : IPluginModule
{
    private LupercaliaMGCore m_CSSPlugin;
    private bool isRoundRestarting = false;
    
    private INativeVoteApi? nativeVoteApi;

    public string PluginModuleName => "VoteRoundRestart";

    private const string NativeVoteIdentifier = "LupercaliaMGCore:VoteRoundRestart";

    public VoteRoundRestart(LupercaliaMGCore plugin)
    {
        m_CSSPlugin = plugin;

        m_CSSPlugin.AddCommand("css_vrr", "Vote round restart command.", CommandVoteRestartRound);
        m_CSSPlugin.AddCommand("css_restart", "Restarts round (admins only)", CommandForceRestartRound);
        m_CSSPlugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
    }

    public void AllPluginsLoaded()
    {
        nativeVoteApi = m_CSSPlugin.GetNativeVoteApi();

        if (nativeVoteApi == null)
        {
            m_CSSPlugin.Logger.LogError("[VoteMapRestart] Failed to get native vote api.");
            UnloadModule();
            return;
        }


        nativeVoteApi.OnVotePass += OnVotePass;
        nativeVoteApi.OnVoteFail += OnVoteFail;
    }

    public void UnloadModule()
    {
        if (nativeVoteApi != null)
        {
            nativeVoteApi.OnVotePass -= OnVotePass;
            nativeVoteApi.OnVoteFail -= OnVoteFail; 
        }
        
        
        m_CSSPlugin.RemoveCommand("css_vrr", CommandVoteRestartRound);
        m_CSSPlugin.RemoveCommand("css_restart", CommandForceRestartRound);
        m_CSSPlugin.DeregisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
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

        Server.PrintToChatAll(
            LupercaliaMGCore.MessageWithPrefix(m_CSSPlugin.Localizer["VoteRoundRestart.Notification.VoteFailed"]));
    }


    private void CommandVoteRestartRound(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        SimpleLogging.LogDebug(
            $"[Vote Round Restart] [Player {client.PlayerName}] trying to vote for restart round.");
        if (isRoundRestarting)
        {
            SimpleLogging.LogDebug(
                $"[Vote Round Restart] [Player {client.PlayerName}] Round is already restarting in progress.");
            client.PrintToChat(LupercaliaMGCore.MessageWithPrefix(
                m_CSSPlugin.Localizer["VoteRoundRestart.Command.Notification.AlreadyRestarting"]));
            return;
        }

        if (nativeVoteApi!.GetCurrentVoteState() != NativeVoteState.NoActiveVote)
        {
            SimpleLogging.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] Already an active vote.");
            client.PrintToChat(LupercaliaMGCore.MessageWithPrefix(
                m_CSSPlugin.Localizer["General.Command.Vote.Notification.AnotherVoteInProgress"]));
            return;
        }

        var potentialClients = Utilities.GetPlayers().Where(p => p is { IsBot: false, IsHLTV: false }).ToList();
        var potentialClientsIndex = potentialClients.Select(p => p.Index).ToList();

        string detailsString =
            NativeVoteTextUtil.GenerateReadableNativeVoteText(
                m_CSSPlugin.Localizer["VoteRoundRestart.Vote.SubjectText"]);

        NativeVoteInfo nInfo = new NativeVoteInfo(NativeVoteIdentifier, NativeVoteTextUtil.VoteDisplayString,
            detailsString, potentialClientsIndex, VoteThresholdType.Percentage,
            PluginSettings.GetInstance.m_CVVoteMapRestartThreshold.Value, 15.0f, initiator: client.Slot);

        NativeVoteState state = nativeVoteApi!.InitiateVote(nInfo);


        if (state == NativeVoteState.InitializeAccepted)
        {
            SimpleLogging.LogDebug(
                $"[Vote Round Restart] [Player {client.PlayerName}] Round restart vote initiated. Vote Identifier: {nInfo.voteIdentifier}");
            Server.PrintToChatAll(
                LupercaliaMGCore.MessageWithPrefix(
                    m_CSSPlugin.Localizer["VoteRoundRestart.Notification.VoteInitiated"]));
        }
        else
        {
            client.PrintToChat(
                LupercaliaMGCore.MessageWithPrefix(
                    m_CSSPlugin.Localizer["General.Command.Vote.Notification.FailedToInitiate"]));
        }
    }

    [RequiresPermissions(@"css/root")]
    private void CommandForceRestartRound(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        SimpleLogging.LogDebug("[Force Round Restart] Initiating round restart...");
        isRoundRestarting = true;

        Server.PrintToChatAll(
            LupercaliaMGCore.MessageWithPrefix(
                m_CSSPlugin.Localizer["VoteRoundRestart.Notification.ForceRoundRestart", 1]));
        m_CSSPlugin.AddTimer(1, () =>
        {
            SimpleLogging.LogDebug("[Vote Round Restart] Restarting round.");
            Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules
                ?.TerminateRound(0.0F, RoundEndReason.RoundDraw);
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void InitiateRoundRestart()
    {
        SimpleLogging.LogDebug("[Vote Round Restart] Initiating round restart...");
        isRoundRestarting = true;

        Server.PrintToChatAll(LupercaliaMGCore.MessageWithPrefix(m_CSSPlugin.Localizer[
            "VoteRoundRestart.Notification.RoundRestart",
            PluginSettings.GetInstance.m_CVVoteRoundRestartRestartTime.Value]));
        m_CSSPlugin.AddTimer(PluginSettings.GetInstance.m_CVVoteRoundRestartRestartTime.Value, () =>
        {
            SimpleLogging.LogDebug("[Vote Round Restart] Restarting round.");
            Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules
                ?.TerminateRound(0.0F, RoundEndReason.RoundDraw);
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }


    private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info)
    {
        isRoundRestarting = false;
        return HookResult.Continue;
    }
}