using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using TNCSSPluginFoundation.Models.Plugin;

namespace LupercaliaMGCore.modules;

public sealed class RoundEndDeathMatch(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "RoundEndDeathMatch";
    
    public override string ModuleChatPrefix => "[RoundEndDeathMatch]";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private ConVar? mp_teammates_are_enemies = null;

    
    public readonly FakeConVar<bool> IsModuleEnabled =
        new("lp_mg_redm_enabled", "Should enable round end death match?", true);
    
    protected override void OnInitialize()
    {
        TrackConVar(IsModuleEnabled);
        
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
        DebugLogger.LogDebug("[Round End Death Match] Called RoundPreStart.");
        if (!IsModuleEnabled.Value)
        {
            DebugLogger.LogDebug("[Round End Death Match] REDM is disabled and does nothing.");
            return HookResult.Continue;
        }

        TrySetConVarValue(false);
        DebugLogger.LogDebug("[Round End Death Match] Ended.");
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        DebugLogger.LogDebug("[Round End Death Match] Called RoundEnd");
        if (!IsModuleEnabled.Value)
        {
            DebugLogger.LogDebug("[Round End Death Match] REDM is disabled and does nothing.");
            return HookResult.Continue;
        }

        TrySetConVarValue(true);
        DebugLogger.LogDebug("[Round End Death Match] Started.");
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
                DebugLogger.LogDebug(
                    $"ConVar mp_teammates_are_enemies is failed to set! Current map: {Server.MapName}");
            }
        }
        catch (Exception e)
        {
            DebugLogger.LogDebug(
                $"ConVar mp_teammates_are_enemies is failed to set due to exception. Trace: {e.StackTrace}");
        }
    }
}