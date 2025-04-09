using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using LupercaliaMGCore.model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NativeVoteAPI;
using NativeVoteAPI.API;

namespace LupercaliaMGCore.modules;

public class VoteRoundRestart(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    private bool isRoundRestarting = false;
    
    private INativeVoteApi? nativeVoteApi;

    public override string PluginModuleName => "VoteRoundRestart";
    
    public override string ModuleChatPrefix => "[RoundRestart]";

    private const string NativeVoteIdentifier = "LupercaliaMGCore:VoteRoundRestart";

    public FakeConVar<float> RoundRestartVoteThreshold = new(
        "lp_mg_vrr_vote_threshold",
        "How percent of votes required to initiate the round restart.",
        0.7F,
        ConVarFlags.FCVAR_NONE,
        new RangeValidator<float>(0.0F, 1.0F));
    
    public FakeConVar<float> RoundRestartTime = new(
        "lp_mg_vrr_restart_time",
        "How long to take restarting round after vote passed.",
        10.0F,
        ConVarFlags.FCVAR_NONE,
        new RangeValidator<float>(0.0F, float.MaxValue));

    protected override void OnInitialize()
    {
        TrackConVar(RoundRestartVoteThreshold);
        TrackConVar(RoundRestartTime);
        
        Plugin.AddCommand("css_vrr", "Vote round restart command.", CommandVoteRestartRound);
        Plugin.AddCommand("css_restart", "Restarts round (admins only)", CommandForceRestartRound);
        Plugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
    }

    protected override void OnAllPluginsLoaded()
    {
        try
        {
            nativeVoteApi = ServiceProvider.GetRequiredService<INativeVoteApi>();
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

        DebugLogger.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] trying to vote for restart round.");
        
        if (isRoundRestarting)
        {
            DebugLogger.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] Round is already restarting in progress.");
            
            client.PrintToChat(LocalizeWithPluginPrefix("VoteRoundRestart.Command.Notification.AlreadyRestarting"));
            return;
        }

        if (nativeVoteApi!.GetCurrentVoteState() != NativeVoteState.NoActiveVote)
        {
            DebugLogger.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] Already an active vote.");
            client.PrintToChat(LocalizeWithPluginPrefix("General.Command.Vote.Notification.AnotherVoteInProgress"));
            return;
        }

        var potentialClients = Utilities.GetPlayers().Where(p => p is { IsBot: false, IsHLTV: false }).ToList();
        var potentialClientsIndex = potentialClients.Select(p => p.Index).ToList();

        string detailsString =
            NativeVoteTextUtil.GenerateReadableNativeVoteText(Plugin.Localizer["VoteRoundRestart.Vote.SubjectText"]);

        NativeVoteInfo nInfo = new NativeVoteInfo(NativeVoteIdentifier, NativeVoteTextUtil.VoteDisplayString,
            detailsString, potentialClientsIndex, VoteThresholdType.Percentage,
            RoundRestartVoteThreshold.Value, 15.0f, initiator: client.Slot);

        NativeVoteState state = nativeVoteApi!.InitiateVote(nInfo);


        if (state == NativeVoteState.InitializeAccepted)
        {
            DebugLogger.LogDebug($"[Vote Round Restart] [Player {client.PlayerName}] Round restart vote initiated. Vote Identifier: {nInfo.voteIdentifier}");
            
            PrintLocalizedChatToAll("VoteRoundRestart.Notification.VoteInitiated");
        }
        else
        {
            client.PrintToChat(LocalizeWithPluginPrefix("General.Command.Vote.Notification.FailedToInitiate"));
        }
    }

    [RequiresPermissions(@"css/root")]
    private void CommandForceRestartRound(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        DebugLogger.LogDebug("[Force Round Restart] Initiating round restart...");
        isRoundRestarting = true;

        PrintLocalizedChatToAll("VoteRoundRestart.Notification.ForceRoundRestart", 1);
        
        Plugin.AddTimer(1, () =>
        {
            DebugLogger.LogDebug("[Vote Round Restart] Restarting round.");
            EntityUtil.GetGameRules()?.TerminateRound(0.0F, RoundEndReason.RoundDraw);
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void InitiateRoundRestart()
    {
        DebugLogger.LogDebug("[Vote Round Restart] Initiating round restart...");
        isRoundRestarting = true;

        PrintLocalizedChatToAll("VoteRoundRestart.Notification.RoundRestart", RoundRestartTime.Value);
        
        Plugin.AddTimer(RoundRestartTime.Value, () =>
        {
            DebugLogger.LogDebug("[Vote Round Restart] Restarting round.");
            EntityUtil.GetGameRules()?.TerminateRound(0.0F, RoundEndReason.RoundDraw);
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }


    private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info)
    {
        isRoundRestarting = false;
        return HookResult.Continue;
    }
}