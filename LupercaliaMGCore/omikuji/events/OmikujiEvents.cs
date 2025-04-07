namespace LupercaliaMGCore;

public class OmikujiEvents(Omikuji omikuji, LupercaliaMGCore plugin)
{
    private readonly LupercaliaMGCore Plugin = plugin;

    public Dictionary<OmikujiType, List<OmikujiEventBase>> GetEvents()
    {
        if (!isEventsInitialized)
            throw new InvalidOperationException("Omikuji Events list are not initialized yet.");

        return events;
    }

    private bool isEventsInitialized = false;

    private readonly Dictionary<OmikujiType, List<OmikujiEventBase>> events = new();

    public void InitializeOmikujiEvents()
    {
        InitializeEventsList();
        
        // Bad events
        InitializeEvent(new GravityChangeEvent(omikuji, Plugin));
        InitializeEvent(new PlayerFreezeEvent(omikuji, Plugin));
        InitializeEvent(new PlayerLocationSwapEvent(omikuji, Plugin));
        InitializeEvent(new PlayerSlapEvent(omikuji, Plugin));

        
        // Lucky events
        InitializeEvent(new GiveRandomItemEvent(omikuji, Plugin));
        InitializeEvent(new PlayerHealEvent(omikuji, Plugin));
        InitializeEvent(new PlayerRespawnAllEvent(omikuji, Plugin));
        InitializeEvent(new PlayerRespawnEvent(omikuji, Plugin));

        // Misc events
        InitializeEvent(new ChickenSpawnEvent(omikuji, Plugin));
        InitializeEvent(new NothingEvent(omikuji, Plugin));
        InitializeEvent(new PlayerWishingEvent(omikuji, Plugin));
        InitializeEvent(new ScreenShakeEvent(omikuji, Plugin));

        isEventsInitialized = true;
    }

    private void InitializeEvent(OmikujiEventBase omikujiEvent)
    {
        events[omikujiEvent.OmikujiType].Add(omikujiEvent);
        omikujiEvent.Initialize();
        SimpleLogging.LogDebug($"[Omikuji {omikujiEvent.EventName}] initialized");
    }

    private void InitializeEventsList()
    {
        foreach (OmikujiType type in Enum.GetValues(typeof(OmikujiType)))
        {
            events[type] = new List<OmikujiEventBase>();
        }
    }
}