using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using LupercaliaMGCore.model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NativeVoteAPI;
using NativeVoteAPI.API;

namespace LupercaliaMGCore.modules;

public class VoteMapRestart(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "VoteMapRestart";
    
    public override string ModuleChatPrefix => "[VoteMapRestart]";

    private double mapStartTime = 0.0D;
    private bool isMapRestarting = false;
    
    private INativeVoteApi? nativeVoteApi;

    private const string NativeVoteIdentifier = "LupercaliaMGCore:VoteMapRestart";

    protected override void OnInitialize()
    {
        mapStartTime = Server.EngineTime;
        Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        Plugin.AddCommand("css_vmr", "Vote map restart command", CommandVoteRestartMap);
    }

    protected override void OnAllPluginsLoaded()
    {
        try
        {
            nativeVoteApi = Plugin.ServiceProvider.GetRequiredService<INativeVoteApi>();
        }
        catch (Exception)
        {
            // Unused
        }

        if (nativeVoteApi == null)
        {
            Logger.LogError("Failed to find required service: NativeVoteAPI. Unloading module...");
            UnloadModule();
            return;
        }
        
        
        nativeVoteApi.OnVotePass += OnVotePass;
        nativeVoteApi.OnVoteFail += OnVoteFail;
    }

    protected override void OnUnloadModule()
    {
        if (nativeVoteApi != null)
        {
            nativeVoteApi.OnVotePass -= OnVotePass;
            nativeVoteApi.OnVoteFail -= OnVoteFail;
        }
        
        Plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
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

        PrintLocalizedChatToAll("VoteMapRestart.Notification.VoteFailed");
    }

    private void CommandVoteRestartMap(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        SimpleLogging.LogDebug($"[Vote Map Restart] [Player {client.PlayerName}] trying to vote for restart map.");
        if (isMapRestarting)
        {
            SimpleLogging.LogDebug($"[Vote Map Restart] [Player {client.PlayerName}] map is already restarting in progress.");
            client.PrintToChat(LocalizeWithPluginPrefix("VoteMapRestart.Command.Notification.AlreadyRestarting"));
            return;
        }

        if (Server.EngineTime - mapStartTime > PluginSettings.m_CVVoteMapRestartAllowedTime.Value)
        {
            SimpleLogging.LogDebug($"[Vote Map Restart] [Player {client.PlayerName}] restart time is ended");
            client.PrintToChat(LocalizeWithPluginPrefix("VoteMapRestart.Command.Notification.AllowedTimeIsEnded"));
            return;
        }

        if (nativeVoteApi!.GetCurrentVoteState() != NativeVoteState.NoActiveVote)
        {
            SimpleLogging.LogDebug($"[Vote Map Restart] [Player {client.PlayerName}] Already an active vote.");
            client.PrintToChat(Plugin.Localizer["General.Command.Vote.Notification.AnotherVoteInProgress"]);
            return;
        }

        var potentialClients = Utilities.GetPlayers().Where(p => p is { IsBot: false, IsHLTV: false }).ToList();
        var potentialClientsIndex = potentialClients.Select(p => p.Index).ToList();

        string detailsString =
            NativeVoteTextUtil.GenerateReadableNativeVoteText(Plugin.Localizer["VoteMapRestart.Vote.SubjectText"]);

        NativeVoteInfo nInfo = new NativeVoteInfo(NativeVoteIdentifier, NativeVoteTextUtil.VoteDisplayString,
            detailsString, potentialClientsIndex, VoteThresholdType.Percentage,
            PluginSettings.m_CVVoteMapRestartThreshold.Value, 15.0f, initiator: client.Slot);

        NativeVoteState state = nativeVoteApi!.InitiateVote(nInfo);


        if (state == NativeVoteState.InitializeAccepted)
        {
            SimpleLogging.LogDebug($"[Vote Map Restart] [Player {client.PlayerName}] Map reload vote initiated. Vote Identifier: {nInfo.voteIdentifier}");
            PrintLocalizedChatToAll("VoteMapRestart.Notification.VoteInitiated");
        }
        else
        {
            client.PrintToChat(LocalizeWithPluginPrefix("General.Command.Vote.Notification.FailedToInitiate"));
        }
    }


    private void InitiateMapRestart()
    {
        SimpleLogging.LogDebug("[Vote Map Restart] Initiating map restart...");
        isMapRestarting = true;

        float mapRestartTime = PluginSettings.m_CVVoteMapRestartRestartTime.Value;
        PrintLocalizedChatToAll("VoteMapRestart.Notification.MapRestart", mapRestartTime);
        
        Plugin.AddTimer(PluginSettings.m_CVVoteMapRestartRestartTime.Value, () =>
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