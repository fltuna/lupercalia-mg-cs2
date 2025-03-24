using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore {
    public class RoundEndDeathMatch: IPluginModule {
        private LupercaliaMGCore m_CSSPlugin;
        
        public string PluginModuleName => "RoundEndDeathMatch";

        private ConVar? mp_teammates_are_enemies = null;

        public RoundEndDeathMatch(LupercaliaMGCore plugin) {
            m_CSSPlugin = plugin;

            trySetValue(false);

            m_CSSPlugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
            m_CSSPlugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        }

        public void AllPluginsLoaded()
        {
        }

        public void UnloadModule()
        {
            m_CSSPlugin.DeregisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
            m_CSSPlugin.DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);
        }

        private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info) {
            SimpleLogging.LogDebug("[Round End Death Match] Called RoundPreStart.");
            if(!PluginSettings.getInstance.m_CVIsRoundEndDeathMatchEnabled.Value) {
                SimpleLogging.LogDebug("[Round End Death Match] REDM is disabled and does nothing.");
                return HookResult.Continue;
            }
            trySetValue(false);
            SimpleLogging.LogDebug("[Round End Death Match] Ended.");
            return HookResult.Continue;
        }

        private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
            SimpleLogging.LogDebug("[Round End Death Match] Called RoundEnd");
            if(!PluginSettings.getInstance.m_CVIsRoundEndDeathMatchEnabled.Value) {
                SimpleLogging.LogDebug("[Round End Death Match] REDM is disabled and does nothing.");
                return HookResult.Continue;
            }
            trySetValue(true);
            SimpleLogging.LogDebug("[Round End Death Match] Started.");
            return HookResult.Continue;
        }

        private void trySetValue(bool value) {
            if(mp_teammates_are_enemies == null) {
                mp_teammates_are_enemies = ConVar.Find("mp_teammates_are_enemies");
            }

            mp_teammates_are_enemies?.SetValue(value);

            try {
                if(mp_teammates_are_enemies == null || mp_teammates_are_enemies.GetPrimitiveValue<bool>() != value) {
                    SimpleLogging.LogDebug($"ConVar mp_teammates_are_enemies is failed to set! Current map: {Server.MapName}");
                }
            } catch(Exception e) {
                SimpleLogging.LogDebug($"ConVar mp_teammates_are_enemies is failed to set due to exception. Trace: {e.StackTrace}");
            }
        }
    }
}