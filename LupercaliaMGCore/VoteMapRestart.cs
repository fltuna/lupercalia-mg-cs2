using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using LupercaliaMGCore.model;
using Microsoft.Extensions.Logging;
using NativeVoteAPI;
using NativeVoteAPI.API;

namespace LupercaliaMGCore;

public class VoteMapRestart : IPluginModule
{
    private LupercaliaMGCore m_CSSPlugin;

    public string PluginModuleName => "VoteMapRestart";

    private double mapStartTime = 0.0D;
    private bool isMapRestarting = false;
    
    private INativeVoteApi? nativeVoteApi;

    private const string NativeVoteIdentifier = "LupercaliaMGCore:VoteMapRestart";

    public VoteMapRestart(LupercaliaMGCore plugin)
    {
        m_CSSPlugin = plugin;

        mapStartTime = Server.EngineTime;
        m_CSSPlugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        m_CSSPlugin.AddCommand("css_vmr", "Vote map restart command", CommandVoteRestartMap);
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
        
        m_CSSPlugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
    }

    private void OnVotePass(YesNoVoteInfo? info)
    {
        if (info == null)
            return;

        if (info.VoteInfo.voteIdentifier != NativeVoteIdentifier)
            return;

        InitiateMapRestart();
    }

    private void OnVoteFail(YesNoVoteInfo? info)
    {
        if (info == null)
            return;

        if (info.VoteInfo.voteIdentifier != NativeVoteIdentifier)
            return;

        Server.PrintToChatAll(
            LupercaliaMGCore.MessageWithPrefix(m_CSSPlugin.Localizer["VoteMapRestart.Notification.VoteFailed"]));
    }

    private void CommandVoteRestartMap(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        SimpleLogging.LogDebug($"[Vote Map Restart] [Player {client.PlayerName}] trying to vote for restart map.");
        if (isMapRestarting)
        {
            SimpleLogging.LogDebug(
                $"[Vote Map Restart] [Player {client.PlayerName}] map is already restarting in progress.");
            client.PrintToChat(LupercaliaMGCore.MessageWithPrefix(
                m_CSSPlugin.Localizer["VoteMapRestart.Command.Notification.AlreadyRestarting"]));
            return;
        }

        if (Server.EngineTime - mapStartTime > PluginSettings.GetInstance.m_CVVoteMapRestartAllowedTime.Value)
        {
            SimpleLogging.LogDebug($"[Vote Map Restart] [Player {client.PlayerName}] restart time is ended");
            client.PrintToChat(LupercaliaMGCore.MessageWithPrefix(
                m_CSSPlugin.Localizer["VoteMapRestart.Command.Notification.AllowedTimeIsEnded"]));
            return;
        }

        if (nativeVoteApi!.GetCurrentVoteState() != NativeVoteState.NoActiveVote)
        {
            SimpleLogging.LogDebug($"[Vote Map Restart] [Player {client.PlayerName}] Already an active vote.");
            client.PrintToChat(LupercaliaMGCore.MessageWithPrefix(
                m_CSSPlugin.Localizer["General.Command.Vote.Notification.AnotherVoteInProgress"]));
            return;
        }

        var potentialClients = Utilities.GetPlayers().Where(p => p is { IsBot: false, IsHLTV: false }).ToList();
        var potentialClientsIndex = potentialClients.Select(p => p.Index).ToList();

        string detailsString =
            NativeVoteTextUtil.GenerateReadableNativeVoteText(
                m_CSSPlugin.Localizer["VoteMapRestart.Vote.SubjectText"]);

        NativeVoteInfo nInfo = new NativeVoteInfo(NativeVoteIdentifier, NativeVoteTextUtil.VoteDisplayString,
            detailsString, potentialClientsIndex, VoteThresholdType.Percentage,
            PluginSettings.GetInstance.m_CVVoteMapRestartThreshold.Value, 15.0f, initiator: client.Slot);

        NativeVoteState state = nativeVoteApi!.InitiateVote(nInfo);


        if (state == NativeVoteState.InitializeAccepted)
        {
            SimpleLogging.LogDebug(
                $"[Vote Map Restart] [Player {client.PlayerName}] Map reload vote initiated. Vote Identifier: {nInfo.voteIdentifier}");
            Server.PrintToChatAll(
                LupercaliaMGCore.MessageWithPrefix(
                    m_CSSPlugin.Localizer["VoteMapRestart.Notification.VoteInitiated"]));
        }
        else
        {
            client.PrintToChat(
                LupercaliaMGCore.MessageWithPrefix(
                    m_CSSPlugin.Localizer["General.Command.Vote.Notification.FailedToInitiate"]));
        }
    }


    private void InitiateMapRestart()
    {
        SimpleLogging.LogDebug("[Vote Map Restart] Initiating map restart...");
        isMapRestarting = true;
        Server.PrintToChatAll(LupercaliaMGCore.MessageWithPrefix(m_CSSPlugin.Localizer[
            "VoteMapRestart.Notification.MapRestart",
            PluginSettings.GetInstance.m_CVVoteMapRestartRestartTime.Value]));
        m_CSSPlugin.AddTimer(PluginSettings.GetInstance.m_CVVoteMapRestartRestartTime.Value, () =>
        {
            SimpleLogging.LogDebug("[Vote Map Restart] Changing map.");
            Server.ExecuteCommand($"changelevel {Server.MapName}");
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void OnMapStart(string mapName)
    {
        mapStartTime = Server.EngineTime;
        isMapRestarting = false;
    }
}