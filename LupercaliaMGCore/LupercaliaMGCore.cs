using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;
using Microsoft.Extensions.Logging;
using NativeVoteAPI.API;

namespace LupercaliaMGCore;

public class LupercaliaMGCore : BasePlugin
{
    private static readonly string PluginPrefix =
        $" {ChatColors.DarkRed}[{ChatColors.Blue}LPŘ MG{ChatColors.DarkRed}]{ChatColors.Default}";

    public string LocalizeStringWithPrefix(string languageKey, params object[] args)
    {
        return $"{PluginPrefix} {Localizer[languageKey, args]}";
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

    public override string ModuleVersion => "1.5.0";

    public override string ModuleAuthor => "faketuna, Spitice";

    public override string ModuleDescription => "Provides core MG feature in CS2 with CounterStrikeSharp";

    private readonly HashSet<PluginModuleBase> loadedModules = [];


    public override void Load(bool hotReload)
    {
        instance = this;
        new PluginSettings(this);
        Logger.LogInformation("Plugin settings initialized");

        InitializeModule(new TeamBasedBodyColor(this));
        InitializeModule(new DuckFix(this));
        InitializeModule(new TeamScramble(this));
        InitializeModule(new VoteMapRestart(this));
        InitializeModule(new VoteRoundRestart(this));
        InitializeModule(new RoundEndDamageImmunity(this));
        InitializeModule(new RoundEndWeaponStrip(this));
        InitializeModule(new RoundEndDeathMatch(this));
        InitializeModule(new ScheduledShutdown(this));
        InitializeModule(new Respawn(this));
        InitializeModule(new MapConfig(this));
        InitializeModule(new AntiCamp(this, hotReload));
        InitializeModule(new Omikuji(this));
        InitializeModule(new Debugging(this));
        InitializeModule(new MiscCommands(this));
        InitializeModule(new JoinTeamFix(this));
        InitializeModule(new HideLegs(this));
        InitializeModule(new ExternalView(this));
        InitializeModule(new CourseWeapons(this));
        InitializeModule(new VelocityDisplay(this));
        InitializeModule(new Rocket(this));
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
        UnloadAllModules();
    }
    
    private void InitializeModule(PluginModuleBase module)
    {
        loadedModules.Add(module);
        module.Initialize();
        Logger.LogInformation($"{module.PluginModuleName} has been initialized");
    }

    private void UnloadAllModules()
    {
        foreach (PluginModuleBase loadedModule in loadedModules)
        {
            loadedModule.UnloadModule();
            Logger.LogInformation($"{loadedModule.PluginModuleName} has been unloaded.");
        }
    }
}