using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore.modules;

public class RoundEndDeathMatch(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "RoundEndDeathMatch";
    
    public override string ModuleChatPrefix => "[RoundEndDeathMatch]";

    private ConVar? mp_teammates_are_enemies = null;

    protected override void OnInitialize()
    {
        TrySetConVarValue(false);

        Plugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
        Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
    }

    protected override void OnUnloadModule()
    {
        Plugin.DeregisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
        Plugin.DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);
    }

    private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info)
    {
        SimpleLogging.LogDebug("[Round End Death Match] Called RoundPreStart.");
        if (!PluginSettings.GetInstance.m_CVIsRoundEndDeathMatchEnabled.Value)
        {
            SimpleLogging.LogDebug("[Round End Death Match] REDM is disabled and does nothing.");
            return HookResult.Continue;
        }

        TrySetConVarValue(false);
        SimpleLogging.LogDebug("[Round End Death Match] Ended.");
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        SimpleLogging.LogDebug("[Round End Death Match] Called RoundEnd");
        if (!PluginSettings.GetInstance.m_CVIsRoundEndDeathMatchEnabled.Value)
        {
            SimpleLogging.LogDebug("[Round End Death Match] REDM is disabled and does nothing.");
            return HookResult.Continue;
        }

        TrySetConVarValue(true);
        SimpleLogging.LogDebug("[Round End Death Match] Started.");
        return HookResult.Continue;
    }

    private void TrySetConVarValue(bool value)
    {
        mp_teammates_are_enemies ??= ConVar.Find("mp_teammates_are_enemies");

        mp_teammates_are_enemies?.SetValue(value);

        try
        {
            if (mp_teammates_are_enemies == null || mp_teammates_are_enemies.GetPrimitiveValue<bool>() != value)
            {
                SimpleLogging.LogDebug(
                    $"ConVar mp_teammates_are_enemies is failed to set! Current map: {Server.MapName}");
            }
        }
        catch (Exception e)
        {
            SimpleLogging.LogDebug(
                $"ConVar mp_teammates_are_enemies is failed to set due to exception. Trace: {e.StackTrace}");
        }
    }
}