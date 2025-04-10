using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace LupercaliaMGCore.modules;

public sealed class Omikuji(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "Omikuji";

    public override string ModuleChatPrefix => $" {ChatColors.Gold}[Omikuji]{ChatColors.Default}";

    private readonly Random Random = new();

    private OmikujiEvents omikujiEvents = null!;

    private readonly List<(OmikujiType omikujiType, double weight)> omikujiTypes = new();

    private readonly Dictionary<CCSPlayerController, double> lastCommandUseTime = new();

    private readonly Dictionary<CCSPlayerController, bool> isWaitingForEventExecution = new();

    
    public readonly FakeConVar<int> EventWeightMisc = new("lp_mg_omikuji_event_weight_misc",
        "Weight of misc event. You can set to any value.", 90);

    public readonly FakeConVar<int> EventWeightBad = new("lp_mg_omikuji_event_weight_bad",
        "Weight of bad event. You can set to any value.", 5);

    public readonly FakeConVar<int> EventWeightLucky = new("lp_mg_omikuji_event_weight_lucky",
        "Weight of lucky event. You can set to any value.", 5);

    public readonly FakeConVar<double> CommandCooldown =
        new("lp_mg_omikuji_command_cooldown", "Cooldown of omikuji command.", 60.0D);

    public readonly FakeConVar<int> CommandExecutionDelayMin = new(
        "lp_mg_omikuji_command_execution_delay_min",
        "Minimum time of omikuji event executed after execution of command.", 5);

    public readonly FakeConVar<int> CommandExecutionDelayMax = new(
        "lp_mg_omikuji_command_execution_delay_max",
        "Maximum time of omikuji event executed after execution of command.", 10);
    

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }

    public override void UpdateServices(IServiceProvider services)
    {
        ServiceProvider = services;
        omikujiEvents = new OmikujiEvents(services);
    }


    protected override void OnInitialize()
    {
        TrackConVar(EventWeightMisc);
        TrackConVar(EventWeightBad);
        TrackConVar(EventWeightLucky);
        TrackConVar(CommandCooldown);
        TrackConVar(CommandExecutionDelayMin);
        TrackConVar(CommandExecutionDelayMax);
        
        Plugin.AddCommand("css_omikuji", "draw a fortune.", CommandOmikuji);
        Plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);

        omikujiTypes.Add((OmikujiType.EventBad, EventWeightBad.Value));
        omikujiTypes.Add((OmikujiType.EventLucky, EventWeightLucky.Value));
        omikujiTypes.Add((OmikujiType.EventMisc, EventWeightMisc.Value));

        // For hot reload and server startup
        Plugin.AddTimer(0.1F, () =>
        {
            DebugLogger.LogDebug("Late initialization for hot reloading omikuji.");
            foreach (CCSPlayerController client in Utilities.GetPlayers())
            {
                if (!client.IsValid || client.IsBot || client.IsHLTV)
                    continue;

                lastCommandUseTime[client] = 0.0D;
                ResetPlayerInformation(client);
            }
        });
        
    }

    protected override void OnAllPluginsLoaded()
    {
        omikujiEvents.InitializeOmikujiEvents();
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_omikuji", CommandOmikuji);
        Plugin.DeregisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? client = @event.Userid;

        if (client == null)
            return HookResult.Continue;

        ResetPlayerInformation(client);
        return HookResult.Continue;
    }

    private void ResetPlayerInformation(CCSPlayerController client)
    {
        DebugLogger.LogDebug("Omikuji: Resetting player information");
        lastCommandUseTime[client] = 0.0D;
        isWaitingForEventExecution[client] = false;
    }

    private void CommandOmikuji(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        if (isWaitingForEventExecution[client])
        {
            client.PrintToChat(LocalizeWithModulePrefix("Omikuji.Command.Notification.NotReady"));
            return;
        }

        if (Server.EngineTime - lastCommandUseTime[client] <
            CommandCooldown.Value)
        {
            
            string currentCooldownText = (CommandCooldown.Value - (Server.EngineTime - lastCommandUseTime[client])).ToString("#.#");
            
            client.PrintToChat(LocalizeWithModulePrefix("Omikuji.Command.Notification.Cooldown", currentCooldownText));
            return;
        }

        DebugLogger.LogDebug($"[Omikuji] [Player {client.PlayerName}] trying to draw omikuji.");
        DebugLogger.LogTrace($"[Omikuji] [Player {client.PlayerName}] Picking random omikuji type.");
        OmikujiType randomOmikujiType = GetRandomOmikujiType();
        var events = omikujiEvents.GetEvents()[randomOmikujiType];
        bool isPlayerAlive = PlayerUtil.IsPlayerAlive(client);

        OmikujiEventBase omikuji;

        DebugLogger.LogTrace($"[Omikuji] [Player {client.PlayerName}] Picking random omikuji.");
        while (true)
        {
            omikuji = SelectWeightedRandom(events);

            if (omikuji.OmikujiCanInvokeWhen == OmikujiCanInvokeWhen.Anytime)
            {
                break;
            }
            if (omikuji.OmikujiCanInvokeWhen == OmikujiCanInvokeWhen.PlayerDied && !isPlayerAlive)
            {
                break;
            }
            if (omikuji.OmikujiCanInvokeWhen == OmikujiCanInvokeWhen.PlayerAlive && isPlayerAlive)
            {
                break;
            }
        }

        isWaitingForEventExecution[client] = true;
        Server.PrintToChatAll(LocalizeWithModulePrefix("Omikuji.Command.Notification.Drawing", client.PlayerName));
        Plugin.AddTimer(
            Random.Next(CommandExecutionDelayMin.Value,
                CommandExecutionDelayMax.Value), () =>
            {
                DebugLogger.LogTrace($"[Omikuji] [Player {client.PlayerName}] Executing omikuji...");
                lastCommandUseTime[client] = Server.EngineTime;
                isWaitingForEventExecution[client] = false;
                omikuji.Execute(client);
            });
    }

    private OmikujiType GetRandomOmikujiType()
    {
        return SelectWeightedRandom(omikujiTypes);
    }


    private T SelectWeightedRandom<T>(List<(T item, double weight)> weightedItems)
    {
        double totalWeight = 0.0D;
        foreach (var item in weightedItems)
        {
            totalWeight += item.weight;
        }

        double randomVal = Random.NextDouble() * totalWeight;

        foreach (var item in weightedItems)
        {
            if (randomVal < item.weight)
            {
                return item.item;
            }

            randomVal -= item.weight;
        }

        return weightedItems[0].item;
    }

    private OmikujiEventBase SelectWeightedRandom(List<OmikujiEventBase> weightedItems)
    {
        double totalWeight = 0.0D;
        foreach (var item in weightedItems)
        {
            totalWeight += item.GetOmikujiWeight();
        }

        double randomVal = Random.NextDouble() * totalWeight;

        foreach (var item in weightedItems)
        {
            if (randomVal < item.GetOmikujiWeight())
            {
                return item;
            }

            randomVal -= item.GetOmikujiWeight();
        }

        return weightedItems[0];
    }

    public string GetOmikujiLuckMessage(OmikujiType type, CCSPlayerController client)
    {
        string text = "";

        switch (type)
        {
            case OmikujiType.EventBad:
            {
                text =
                    $"{Plugin.Localizer["Omikuji.Events.Notification.BadLuck", client.PlayerName]}";
                break;
            }
            case OmikujiType.EventLucky:
            {
                text =
                    $"{Plugin.Localizer["Omikuji.Events.Notification.Luck", client.PlayerName]}";
                break;
            }
            case OmikujiType.EventMisc:
            {
                text =
                    $"{Plugin.Localizer["Omikuji.Events.Notification.Misc", client.PlayerName]}";
                break;
            }
        }

        return text;
    }
}