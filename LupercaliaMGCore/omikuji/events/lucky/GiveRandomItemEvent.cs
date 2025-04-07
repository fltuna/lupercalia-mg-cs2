using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class GiveRandomItemEvent(Omikuji omikuji, LupercaliaMGCore plugin) : OmikujiEventBase(omikuji, plugin)
{
    public override string EventName => "Give Random Item Event";

    public override OmikujiType OmikujiType => OmikujiType.EventLucky;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.Anytime;

    private static readonly Dictionary<CCSPlayerController, FixedSizeQueue<CsItem>> RecentlyPickedUpItems = new();

    public override void Execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Give random item event");

        SimpleLogging.LogDebug("Iterating the all player");
        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            if (!PlayerUtil.IsPlayerAlive(cl))
                continue;

            SimpleLogging.LogDebug("Picking random item");
            CsItem randomItem = PickRandomItem(cl);

            cl.GiveNamedItem(randomItem);

            SimpleLogging.LogDebug("Enqueue a picked up item to recently picked up items list");
            RecentlyPickedUpItems[cl].Enqueue(randomItem);
            cl.PrintToChat($"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {Plugin.Localizer["Omikuji.LuckyEvent.GiveRandomItemEvent.Notification.ItemReceived", randomItem]}");
        }

        SimpleLogging.LogDebug("Give random item event finished");
    }

    public override void Initialize()
    {
        // Late Initialize this event to avoid CounterStrikeSharp.API.Core.NativeException: Global Variables not initialized yet.
        // This is a temporary workaround until get better solutions
        Plugin.AddTimer(0.01F, () =>
        {
            SimpleLogging.LogDebug("Initializing the Give Random Item Event. This is a late initialization for avoid error.");

            SimpleLogging.LogDebug("Registering the Player Spawn event for initialize late joiners recently picked up items list");
            Plugin.RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
            {
                CCSPlayerController? client = @event.Userid;

                if (client == null)
                    return HookResult.Continue;

                RecentlyPickedUpItems[client] =
                    new FixedSizeQueue<CsItem>(PluginSettings.m_CVOmikujiEventGiveRandomItemAvoidCount
                        .Value);

                return HookResult.Continue;
            });
            SimpleLogging.LogDebug("Registered the Player Spawn event");

            SimpleLogging.LogDebug("Initializing the recently picked up items list for connected players");
            foreach (CCSPlayerController cl in Utilities.GetPlayers())
            {
                if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                    continue;

                if (!RecentlyPickedUpItems.TryGetValue(cl, out _))
                {
                    RecentlyPickedUpItems[cl] = new FixedSizeQueue<CsItem>(PluginSettings.m_CVOmikujiEventGiveRandomItemAvoidCount.Value);
                }
            }

            SimpleLogging.LogDebug("Finished initializing the recently picked up items list");
        });
    }

    public override double GetOmikujiWeight()
    {
        return PluginSettings.m_CVOmikujiEventGiveRandomItemSelectionWeight.Value;
    }

    private static readonly List<CsItem> InvalidItems =
    [
        CsItem.XRayGrenade,
        CsItem.IncGrenade,
        CsItem.FragGrenade,
        CsItem.HE,
        CsItem.Taser,
        CsItem.Knife,
        CsItem.DefaultKnifeCT,
        CsItem.DefaultKnifeT,
        CsItem.Revolver,
        CsItem.P2K,
        CsItem.CZ,
        CsItem.AutoSniperCT,
        CsItem.AutoSniperT,
        CsItem.Diversion,
        CsItem.KevlarHelmet,
        CsItem.Dualies,
        CsItem.Firebomb,
        CsItem.Glock18,
        CsItem.Krieg,
        // Not implemented items
        CsItem.Bumpmine,
        CsItem.BreachCharge,
        CsItem.Shield,
        CsItem.Bomb,
        CsItem.Tablet,
        CsItem.Snowball
    ];

    // HE Grenade giving rate is definitely low. investigate later.
    private CsItem PickRandomItem(CCSPlayerController client)
    {
        SimpleLogging.LogDebug($"PickRandomItem() called. caller: {client.PlayerName}");
        CsItem item;

        string[] items = Enum.GetNames(typeof(CsItem));
        int itemsCount = items.Length;

        SimpleLogging.LogTrace($"CsItems item counts are {itemsCount}");

        SimpleLogging.LogTrace("Picking random item");
        while (true)
        {
            int randomNum = Random.Next(0, itemsCount);

            item = (CsItem)Enum.Parse(typeof(CsItem), items[randomNum]);

            if (!InvalidItems.Contains(item) && !RecentlyPickedUpItems[client].Contains(item))
            {
                break;
            }

            SimpleLogging.LogTrace("Random item are duplicated with recently picked up items");
        }

        SimpleLogging.LogTrace($"Random item are picked: {item}");
        return item;
    }
}