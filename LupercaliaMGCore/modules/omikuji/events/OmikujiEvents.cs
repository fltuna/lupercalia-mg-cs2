using LupercaliaMGCore.model;
using LupercaliaMGCore.modules;
using Microsoft.Extensions.DependencyInjection;

namespace LupercaliaMGCore;

public class OmikujiEvents(IServiceProvider serviceProvider)
{
    private readonly AbstractTunaPluginBase plugin = serviceProvider.GetRequiredService<AbstractTunaPluginBase>();

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
        InitializeEvent(new GravityChangeEvent(serviceProvider));
        InitializeEvent(new PlayerFreezeEvent(serviceProvider));
        InitializeEvent(new PlayerLocationSwapEvent(serviceProvider));
        InitializeEvent(new PlayerSlapEvent(serviceProvider));

        
        // Lucky events
        InitializeEvent(new GiveRandomItemEvent(serviceProvider));
        InitializeEvent(new PlayerHealEvent(serviceProvider));
        InitializeEvent(new PlayerRespawnAllEvent(serviceProvider));
        InitializeEvent(new PlayerRespawnEvent(serviceProvider));

        // Misc events
        InitializeEvent(new ChickenSpawnEvent(serviceProvider));
        InitializeEvent(new NothingEvent(serviceProvider));
        InitializeEvent(new PlayerWishingEvent(serviceProvider));
        InitializeEvent(new ScreenShakeEvent(serviceProvider));

        isEventsInitialized = true;
    }

    private void InitializeEvent(OmikujiEventBase omikujiEvent)
    {
        events[omikujiEvent.OmikujiType].Add(omikujiEvent);
        omikujiEvent.Initialize();
        plugin.RegisterFakeConVars(omikujiEvent.GetType(), omikujiEvent);
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