using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;
using Microsoft.Extensions.DependencyInjection;

namespace LupercaliaMGCore.modules;

public class Omikuji(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "Omikuji";

    public override string ModuleChatPrefix => $" {ChatColors.Gold}[Omikuji]{ChatColors.Default}";

    private readonly Random Random = new();

    private OmikujiEvents omikujiEvents = null!;

    private readonly List<(OmikujiType omikujiType, double weight)> omikujiTypes = new();

    private readonly Dictionary<CCSPlayerController, double> lastCommandUseTime = new();

    private readonly Dictionary<CCSPlayerController, bool> isWaitingForEventExecution = new();


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
        Plugin.AddCommand("css_omikuji", "draw a fortune.", CommandOmikuji);
        Plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);

        omikujiTypes.Add((OmikujiType.EventBad, PluginSettings.m_CVOmikujiEventWeightBad.Value));
        omikujiTypes.Add((OmikujiType.EventLucky, PluginSettings.m_CVOmikujiEventWeightLucky.Value));
        omikujiTypes.Add((OmikujiType.EventMisc, PluginSettings.m_CVOmikujiEventWeightMisc.Value));

        // For hot reload and server startup
        Plugin.AddTimer(0.1F, () =>
        {
            SimpleLogging.LogDebug("Late initialization for hot reloading omikuji.");
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
        SimpleLogging.LogDebug("Omikuji: Resetting player information");
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
            PluginSettings.m_CVOmikujiCommandCooldown.Value)
        {
            
            string currentCooldownText = (PluginSettings.m_CVOmikujiCommandCooldown.Value - (Server.EngineTime - lastCommandUseTime[client])).ToString("#.#");
            
            client.PrintToChat(LocalizeWithModulePrefix("Omikuji.Command.Notification.Cooldown", currentCooldownText));
            return;
        }

        SimpleLogging.LogDebug($"[Omikuji] [Player {client.PlayerName}] trying to draw omikuji.");
        SimpleLogging.LogTrace($"[Omikuji] [Player {client.PlayerName}] Picking random omikuji type.");
        OmikujiType randomOmikujiType = GetRandomOmikujiType();
        var events = omikujiEvents.GetEvents()[randomOmikujiType];
        bool isPlayerAlive = PlayerUtil.IsPlayerAlive(client);

        OmikujiEventBase omikuji;

        SimpleLogging.LogTrace($"[Omikuji] [Player {client.PlayerName}] Picking random omikuji.");
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
            Random.Next(PluginSettings.m_CVOmikujiCommandExecutionDelayMin.Value,
                PluginSettings.m_CVOmikujiCommandExecutionDelayMax.Value), () =>
            {
                SimpleLogging.LogTrace($"[Omikuji] [Player {client.PlayerName}] Executing omikuji...");
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