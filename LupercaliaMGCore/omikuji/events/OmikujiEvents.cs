namespace LupercaliaMGCore;

public static partial class OmikujiEvents
{
    public static Random random = new();

    public static Dictionary<OmikujiType, List<IOmikujiEvent>> getEvents()
    {
        if (!isEventsInitialized)
            throw new InvalidOperationException("Omikuji Events list are not initialized yet.");

        return events;
    }

    private static bool isEventsInitialized = false;

    private static Dictionary<OmikujiType, List<IOmikujiEvent>> events = new();

    public static void initializeOmikujiEvents()
    {
        events[OmikujiType.EVENT_BAD] = new List<IOmikujiEvent>();
        events[OmikujiType.EVENT_LUCKY] = new List<IOmikujiEvent>();
        events[OmikujiType.EVENT_MISC] = new List<IOmikujiEvent>();


        var badEvents = events[OmikujiType.EVENT_BAD];

        badEvents.Add(new GravityChangeEvent());
        badEvents.Add(new PlayerFreezeEvent());
        badEvents.Add(new PlayerLocationSwapEvent());
        badEvents.Add(new PlayerSlapEvent());


        var luckyEvents = events[OmikujiType.EVENT_LUCKY];

        luckyEvents.Add(new GiveRandomItemEvent());
        luckyEvents.Add(new PlayerHealEvent());
        luckyEvents.Add(new PlayerRespawnAllEvent());
        luckyEvents.Add(new PlayerRespawnEvent());

        var miscEvents = events[OmikujiType.EVENT_MISC];

        miscEvents.Add(new ChickenSpawnEvent());
        miscEvents.Add(new NothingEvent());
        miscEvents.Add(new PlayerWishingEvent());
        miscEvents.Add(new ScreenShakeEvent());

        foreach (var evt in events)
        {
            foreach (var e in evt.Value)
            {
                e.initialize();
            }
        }

        isEventsInitialized = true;
    }
}