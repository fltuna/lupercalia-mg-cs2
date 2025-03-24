using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;
using Microsoft.Extensions.Logging;
using NativeVoteAPI.API;

namespace LupercaliaMGCore {
    public class LupercaliaMGCore: BasePlugin
    {
        public static readonly string PLUGIN_PREFIX  = $" {ChatColors.DarkRed}[{ChatColors.Blue}LPŘ MG{ChatColors.DarkRed}]{ChatColors.Default}";

        public static string MessageWithPrefix(string message) {
            return $"{PLUGIN_PREFIX} {message}";
        }

        private static LupercaliaMGCore? instance;

        public static LupercaliaMGCore getInstance() {
            return instance!;
        }

        private static INativeVoteApi? NativeVoteApi = null;

        public INativeVoteApi getNativeVoteApi()
        {
            if (NativeVoteApi == null)
                throw new InvalidOperationException("Native Vote API is not initialized");
            
            return NativeVoteApi;
        }
        
        public override string ModuleName => "Lupercalia MG Core";

        public override string ModuleVersion => "1.3.0";

        public override string ModuleAuthor => "faketuna, Spitice";

        public override string ModuleDescription => "Provides core MG feature in CS2 with CounterStrikeSharp";

        private readonly HashSet<IPluginModule> loadedModules = new();


        public override void Load(bool hotReload)
        {
            instance = this;
            new PluginSettings(this);
            Logger.LogInformation("Plugin settings initialized");

            new TeamBasedBodyColor(this);
            Logger.LogInformation("TBBC initialized");

            new DuckFix(this, hotReload);
            Logger.LogInformation("DFix initialized");

            new TeamScramble(this);
            Logger.LogInformation("TeamScramble initialized");

            loadedModules.Add(new VoteMapRestart(this));
            Logger.LogInformation("VoteMapRestart initialized");

            new VoteRoundRestart(this);
            Logger.LogInformation("VoteRoundRestart initialized");
            
            new RoundEndDamageImmunity(this);
            Logger.LogInformation("RoundEndDamageImmunity initialized");

            new RoundEndWeaponStrip(this);
            Logger.LogInformation("RoundEndWeaponStrip initialized");

            new RoundEndDeathMatch(this);
            Logger.LogInformation("RoundEndDeathMatch initialized");

            new ScheduledShutdown(this);
            Logger.LogInformation("ScheduledShutdown initialized");

            new Respawn(this);
            Logger.LogInformation("Respawn initialized");

            new MapConfig(this);
            Logger.LogInformation("MapConfig initialized");

            new AntiCamp(this, hotReload);
            Logger.LogInformation("Anti Camp initialized");

            new Omikuji(this);
            Logger.LogInformation("Omikuji initialized");

            new Debugging(this);
            Logger.LogInformation("Debugging feature is initialized");

            new MiscCommands(this);
            Logger.LogInformation("misc commands initialized");

            new JoinTeamFix(this);
            Logger.LogInformation("Join team fix initialized");

            new HideLegs(this);
            Logger.LogInformation("Hide legs has been initialized");

            new ExternalView(this);
            Logger.LogInformation("External view has been initialized");
        }

        public override void OnAllPluginsLoaded(bool hotReload)
        {
            try
            {
                NativeVoteApi = INativeVoteApi.Capability.Get();
            }
            catch (Exception e)
            {
                throw new Exception("Native Vote API is not found in current server. Please make sure you have NativeVoteAPI installed.", e);
            }
            
            if (NativeVoteApi == null)
            {
                throw new Exception("Native Vote API is not found in current server. Please make sure you have NativeVoteAPI installed.");
            }
            
            foreach (IPluginModule loadedModule in loadedModules)
            {
                loadedModule.AllPluginsLoaded();
            }
        }

        public override void Unload(bool hotReload)
        {
            foreach (IPluginModule loadedModule in loadedModules)
            {
                loadedModule.UnloadModule();
            }
        }
    }
}