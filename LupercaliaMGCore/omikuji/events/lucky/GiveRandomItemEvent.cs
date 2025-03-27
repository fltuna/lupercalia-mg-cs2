using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class GiveRandomItemEvent : IOmikujiEvent
{
    public string EventName => "Give Random Item Event";

    public OmikujiType OmikujiType => OmikujiType.EVENT_LUCKY;

    public OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.ANYTIME;

    private static Random random = OmikujiEvents.random;

    private static Dictionary<CCSPlayerController, FixedSizeQueue<CsItem>> recentlyPickedUpItems =
        new Dictionary<CCSPlayerController, FixedSizeQueue<CsItem>>();

    public void execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Give random item event");

        CsItem randomItem;
        SimpleLogging.LogDebug("Iterating the all player");
        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            if (!PlayerUtil.IsPlayerAlive(cl))
                continue;

            SimpleLogging.LogDebug("Picking random item");
            randomItem = pickRandomItem(cl);

            cl.GiveNamedItem(randomItem);

            SimpleLogging.LogDebug("Enqueue a picked up item to recently picked up items list");
            recentlyPickedUpItems[cl].Enqueue(randomItem);
            cl.PrintToChat(
                $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {LupercaliaMGCore.getInstance().Localizer["Omikuji.LuckyEvent.GiveRandomItemEvent.Notification.ItemReceived", randomItem]}");
        }

        SimpleLogging.LogDebug("Give random item event finished");
    }

    public void initialize()
    {
        // Late Initialize the this event to avoid CounterStrikeSharp.API.Core.NativeException: Global Variables not initialized yet.
        // This is a temporary workaround until get better solutions
        LupercaliaMGCore.getInstance().AddTimer(0.01F, () =>
        {
            SimpleLogging.LogDebug(
                $"Initializing the Give Random Item Event. This is a late initialization for avoid error.");

            SimpleLogging.LogDebug(
                "Registering the Player Spawn event for initialize late joiners recently picked up items list");
            LupercaliaMGCore.getInstance().RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
            {
                CCSPlayerController? client = @event.Userid;

                if (client == null)
                    return HookResult.Continue;

                recentlyPickedUpItems[client] =
                    new FixedSizeQueue<CsItem>(PluginSettings.GetInstance.m_CVOmikujiEventGiveRandomItemAvoidCount
                        .Value);

                return HookResult.Continue;
            });
            SimpleLogging.LogDebug("Registered the Player Spawn event");

            SimpleLogging.LogDebug("Initializing the recently picked up items list for connected players");
            foreach (CCSPlayerController cl in Utilities.GetPlayers())
            {
                if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                    continue;

                if (!recentlyPickedUpItems.TryGetValue(cl, out _))
                {
                    recentlyPickedUpItems[cl] = new FixedSizeQueue<CsItem>(PluginSettings.GetInstance
                        .m_CVOmikujiEventGiveRandomItemAvoidCount.Value);
                }
            }

            SimpleLogging.LogDebug("Finished initializing the recently picked up items list");
        });
    }

    public double getOmikujiWeight()
    {
        return PluginSettings.GetInstance.m_CVOmikujiEventGiveRandomItemSelectionWeight.Value;
    }

    private static List<CsItem> invalidItems = new List<CsItem>()
    {
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
        CsItem.Snowball,
    };

    // HE Grenade giving rate is definitely low. investigate later.
    private static CsItem pickRandomItem(CCSPlayerController client)
    {
        SimpleLogging.LogDebug($"pickRandomItem() called. caller: {client.PlayerName}");
        CsItem item;

        string[] items = Enum.GetNames(typeof(CsItem));
        int itemsCount = items.Count();

        SimpleLogging.LogTrace($"CsItems item counts are {itemsCount}");

        int randomNum;

        SimpleLogging.LogTrace("Picking random item");
        while (true)
        {
            randomNum = random.Next(0, itemsCount);

            item = (CsItem)Enum.Parse(typeof(CsItem), items[randomNum]);

            if (!invalidItems.Contains(item) && !recentlyPickedUpItems[client].Contains(item))
            {
                break;
            }

            SimpleLogging.LogTrace("Random item are duplicated with recently picked up items");
        }

        SimpleLogging.LogTrace($"Random item are picked: {item}");
        return item;
    }
}