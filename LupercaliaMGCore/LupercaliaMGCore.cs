using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;
using Microsoft.Extensions.Logging;
using NativeVoteAPI.API;

namespace LupercaliaMGCore;

public class LupercaliaMGCore : BasePlugin
{
    public static readonly string PLUGIN_PREFIX =
        $" {ChatColors.DarkRed}[{ChatColors.Blue}LPŘ MG{ChatColors.DarkRed}]{ChatColors.Default}";

    public static string MessageWithPrefix(string message)
    {
        return $"{PLUGIN_PREFIX} {message}";
    }

    private static LupercaliaMGCore? instance;

    public static LupercaliaMGCore getInstance()
    {
        return instance!;
    }

    private static INativeVoteApi? NativeVoteApi = null;

    public INativeVoteApi? GetNativeVoteApi()
    {
        return NativeVoteApi;
    }

    public override string ModuleName => "Lupercalia MG Core";

    public override string ModuleVersion => "1.4.0";

    public override string ModuleAuthor => "faketuna, Spitice";

    public override string ModuleDescription => "Provides core MG feature in CS2 with CounterStrikeSharp";

    private readonly HashSet<IPluginModule> loadedModules = new();


    public override void Load(bool hotReload)
    {
        instance = this;
        new PluginSettings(this);
        Logger.LogInformation("Plugin settings initialized");

        loadedModules.Add(new TeamBasedBodyColor(this));
        Logger.LogInformation("TeamBasedBodyColor initialized");

        loadedModules.Add(new DuckFix(this, hotReload));
        Logger.LogInformation("DuckFix initialized");

        loadedModules.Add(new TeamScramble(this));
        Logger.LogInformation("TeamScramble initialized");

        loadedModules.Add(new VoteMapRestart(this));
        Logger.LogInformation("VoteMapRestart initialized");

        loadedModules.Add(new VoteRoundRestart(this));
        Logger.LogInformation("VoteRoundRestart initialized");

        loadedModules.Add(new RoundEndDamageImmunity(this));
        Logger.LogInformation("RoundEndDamageImmunity initialized");

        loadedModules.Add(new RoundEndWeaponStrip(this));
        Logger.LogInformation("RoundEndWeaponStrip initialized");

        loadedModules.Add(new RoundEndDeathMatch(this));
        Logger.LogInformation("RoundEndDeathMatch initialized");

        loadedModules.Add(new ScheduledShutdown(this));
        Logger.LogInformation("ScheduledShutdown initialized");

        loadedModules.Add(new Respawn(this));
        Logger.LogInformation("Respawn initialized");

        loadedModules.Add(new MapConfig(this));
        Logger.LogInformation("MapConfig initialized");

        loadedModules.Add(new AntiCamp(this, hotReload));
        Logger.LogInformation("Anti Camp initialized");

        loadedModules.Add(new Omikuji(this));
        Logger.LogInformation("Omikuji initialized");

        loadedModules.Add(new Debugging(this));
        Logger.LogInformation("Debugging feature is initialized");

        loadedModules.Add(new MiscCommands(this));
        Logger.LogInformation("misc commands initialized");

        loadedModules.Add(new JoinTeamFix(this));
        Logger.LogInformation("Join team fix initialized");

        loadedModules.Add(new HideLegs(this));
        Logger.LogInformation("Hide legs has been initialized");

        loadedModules.Add(new ExternalView(this));
        Logger.LogInformation("External view has been initialized");
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        void OnNativeVoteApiNotFound(Exception? e = null)
        {
            foreach (IPluginModule loadedModule in loadedModules)
            {
                loadedModule.UnloadModule();
                Logger.LogInformation($"{loadedModule.PluginModuleName} has been unloaded.");
            }
            throw new Exception(
                "Native Vote API is not found in current server. Please make sure you have NativeVoteAPI installed.",
                e);
        }
        
        try
        {
            NativeVoteApi = INativeVoteApi.Capability.Get();
        }
        catch (Exception e)
        {
            OnNativeVoteApiNotFound(e);
        }

        if (NativeVoteApi == null)
        {
            OnNativeVoteApiNotFound();
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
            Logger.LogInformation($"{loadedModule.PluginModuleName} has been unloaded.");
        }
    }
}