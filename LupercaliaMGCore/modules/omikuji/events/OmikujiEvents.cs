using LupercaliaMGCore.modules.omikuji.events.bad;
using LupercaliaMGCore.modules.omikuji.events.lucky;
using LupercaliaMGCore.modules.omikuji.events.misc;
using TNCSSPluginFoundation.Models.Plugin;

namespace LupercaliaMGCore.modules.omikuji.events;

public class OmikujiEvents(IServiceProvider serviceProvider): PluginBasicFeatureBase(serviceProvider)
{

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
        InitializeEvent(new GravityChangeEvent(ServiceProvider));
        InitializeEvent(new PlayerFreezeEvent(ServiceProvider));
        InitializeEvent(new PlayerLocationSwapEvent(ServiceProvider));
        InitializeEvent(new PlayerSlapEvent(ServiceProvider));

        
        // Lucky events
        InitializeEvent(new GiveRandomItemEvent(ServiceProvider));
        InitializeEvent(new PlayerHealEvent(ServiceProvider));
        InitializeEvent(new PlayerRespawnAllEvent(ServiceProvider));
        InitializeEvent(new PlayerRespawnEvent(ServiceProvider));

        // Misc events
        InitializeEvent(new ChickenSpawnEvent(ServiceProvider));
        InitializeEvent(new NothingEvent(ServiceProvider));
        InitializeEvent(new PlayerWishingEvent(ServiceProvider));
        InitializeEvent(new ScreenShakeEvent(ServiceProvider));

        isEventsInitialized = true;
    }

    private void InitializeEvent(OmikujiEventBase omikujiEvent)
    {
        events[omikujiEvent.OmikujiType].Add(omikujiEvent);
        omikujiEvent.Initialize();
        Plugin.RegisterFakeConVars(omikujiEvent.GetType(), omikujiEvent);
        DebugLogger.LogDebug($"[Omikuji {omikujiEvent.EventName}] initialized");
    }

    private void InitializeEventsList()
    {
        foreach (OmikujiType type in Enum.GetValues(typeof(OmikujiType)))
        {
            events[type] = new List<OmikujiEventBase>();
        }
    }
}