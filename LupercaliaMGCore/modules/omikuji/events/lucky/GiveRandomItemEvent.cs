using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using LupercaliaMGCore.util;
using TNCSSPluginFoundation.Utils.Entity;

namespace LupercaliaMGCore.modules.omikuji.events.lucky;

public class GiveRandomItemEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Give Random Item Event";

    public override OmikujiType OmikujiType => OmikujiType.EventLucky;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.Anytime;

    private static readonly Dictionary<CCSPlayerController, FixedSizeQueue<CsItem>> RecentlyPickedUpItems = new();

    
    public readonly FakeConVar<int> DuplicationAvoidCount = new(
        "lp_mg_omikuji_event_give_random_item_avoid_duplication_history",
        "How many histories save to avoid give duplicated item.", 10);

    public readonly FakeConVar<double> EventSelectionWeight =
        new("lp_mg_omikuji_event_give_random_item_selection_weight", "Selection weight of this event", 30.0D);
    
    public override void Initialize()
    {
        TrackConVar(DuplicationAvoidCount);
        TrackConVar(EventSelectionWeight);
        
        // Late Initialize this event to avoid CounterStrikeSharp.API.Core.NativeException: Global Variables not initialized yet.
        // This is a temporary workaround until get better solutions
        Plugin.AddTimer(0.01F, () =>
        {
            DebugLogger.LogDebug("Initializing the Give Random Item Event. This is a late initialization for avoid error.");

            DebugLogger.LogDebug("Registering the Player Spawn event for initialize late joiners recently picked up items list");
            Plugin.RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
            {
                CCSPlayerController? client = @event.Userid;

                if (client == null)
                    return HookResult.Continue;

                RecentlyPickedUpItems[client] =
                    new FixedSizeQueue<CsItem>(DuplicationAvoidCount
                        .Value);

                return HookResult.Continue;
            });
            DebugLogger.LogDebug("Registered the Player Spawn event");

            DebugLogger.LogDebug("Initializing the recently picked up items list for connected players");
            foreach (CCSPlayerController cl in Utilities.GetPlayers())
            {
                if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                    continue;

                if (!RecentlyPickedUpItems.TryGetValue(cl, out _))
                {
                    RecentlyPickedUpItems[cl] = new FixedSizeQueue<CsItem>(DuplicationAvoidCount.Value);
                }
            }

            DebugLogger.LogDebug("Finished initializing the recently picked up items list");
        });
    }
    
    public override void Execute(CCSPlayerController client)
    {
        DebugLogger.LogDebug("Player drew a omikuji and invoked Give random item event");

        DebugLogger.LogDebug("Iterating the all player");
        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            if (!PlayerUtil.IsPlayerAlive(cl))
                continue;

            DebugLogger.LogDebug("Picking random item");
            CsItem randomItem = PickRandomItem(cl);

            cl.GiveNamedItem(randomItem);

            DebugLogger.LogDebug("Enqueue a picked up item to recently picked up items list");
            RecentlyPickedUpItems[cl].Enqueue(randomItem);
            cl.PrintToChat(LocalizeOmikujiResult(client, OmikujiType, "Omikuji.LuckyEvent.GiveRandomItemEvent.Notification.ItemReceived", randomItem));
        }

        DebugLogger.LogDebug("Give random item event finished");
    }

    public override double GetOmikujiWeight()
    {
        return EventSelectionWeight.Value;
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
        DebugLogger.LogDebug($"PickRandomItem() called. caller: {client.PlayerName}");
        CsItem item;

        string[] items = Enum.GetNames(typeof(CsItem));
        int itemsCount = items.Length;

        DebugLogger.LogTrace($"CsItems item counts are {itemsCount}");

        DebugLogger.LogTrace("Picking random item");
        while (true)
        {
            int randomNum = Random.Next(0, itemsCount);

            item = (CsItem)Enum.Parse(typeof(CsItem), items[randomNum]);

            if (!InvalidItems.Contains(item) && !RecentlyPickedUpItems[client].Contains(item))
            {
                break;
            }

            DebugLogger.LogTrace("Random item are duplicated with recently picked up items");
        }

        DebugLogger.LogTrace($"Random item are picked: {item}");
        return item;
    }
}