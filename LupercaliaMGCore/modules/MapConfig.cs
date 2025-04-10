using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;

namespace LupercaliaMGCore.modules;

public sealed class MapConfig(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "MapConfig";

    public override string ModuleChatPrefix => "[Map Config]";

    // Config name and path
    private readonly List<MapConfigFile> configs = new();

    private string configFolder = null!;

    
    public readonly FakeConVar<int> ConfigExecutionTiming = new("lp_mg_mapcfg_execution_timing",
        "When configs are executed? 0: Does nothing, 1: Execute on map start, 2: Execute on every round start, 3: Execute on map transition and every round start",
        1, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 3));

    public readonly FakeConVar<int> ConfigType = new("lp_mg_mapcfg_type",
        "Map configuration type. 0: disabled, 1: Exact match, 2: Partial Match", 1, ConVarFlags.FCVAR_NONE,
        new RangeValidator<int>(0, 2));
    
    protected override void OnInitialize()
    {
        TrackConVar(ConfigExecutionTiming);
        TrackConVar(ConfigType);
        
        configFolder = Path.GetFullPath(Path.Combine(Server.GameDirectory, Plugin.BaseCfgDirectoryPath, "map/"));
        
        if (!checkDirectoryExists())
            throw new InvalidOperationException("Map config directory is not exists and failed to create.");

        updateConfigsDictionary();

        Plugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart, HookMode.Post);
        Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    protected override void OnUnloadModule()
    {
        Plugin.DeregisterEventHandler<EventRoundPrestart>(OnRoundPreStart, HookMode.Post);
        Plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
    }

    private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info)
    {
        if (!MathUtil.DecomposePowersOfTwo(ConfigExecutionTiming.Value)
                .Contains(2))
            return HookResult.Continue;

        DebugLogger.LogDebug("[Map Config] Executing configs at round PreStart.");
        ExecuteConfigs();
        return HookResult.Continue;
    }

    private void OnMapStart(string mapName)
    {
        if (!MathUtil.DecomposePowersOfTwo(ConfigExecutionTiming.Value)
                .Contains(1))
            return;

        DebugLogger.LogDebug("[Map Config] Executing configs at map start.");
        ExecuteConfigs();
    }

    private void ExecuteConfigs()
    {
        DebugLogger.LogTrace("[Map Config] Updating the config dictionary");
        updateConfigsDictionary();

        int mapCfgType = ConfigType.Value;

        DebugLogger.LogTrace("[Map Config] Checking the Map Config type");
        if (mapCfgType == 0)
        {
            DebugLogger.LogTrace("[Map Config] mapCfgType is 0. cancelling the execution.");
            return;
        }

        string? mapName = Server.MapName;

        // mapName is nullable when server startup. This is required for plugin loading when server startup.
        if (mapName == null)
        {
            DebugLogger.LogTrace("[Map Config] mapName is null. cancelling the execution.");
            return;
        }


        DebugLogger.LogTrace("[Map Config] Iterating the config file");
        foreach (MapConfigFile conf in configs)
        {
            bool shouldExecute = false;

            if (MathUtil.DecomposePowersOfTwo(mapCfgType).Contains(2) && mapName.Contains(conf.name))
                shouldExecute = true;

            if (MathUtil.DecomposePowersOfTwo(mapCfgType).Contains(1) && mapName.Equals(conf.name))
                shouldExecute = true;

            if (!shouldExecute) 
                continue;
            
            DebugLogger.LogTrace($"[Map Config] Executing config {conf.name} located at {conf.path}");
            Server.ExecuteCommand($"exec {conf.path}");
        }
    }

    private void updateConfigsDictionary()
    {
        DebugLogger.LogTrace("[Map Config] Get files from directory");
        string[] files = Directory.GetFiles(configFolder, "", SearchOption.TopDirectoryOnly);

        DebugLogger.LogTrace("[Map Config] Clearing configs");
        configs.Clear();
        DebugLogger.LogTrace("[Map Config] Iterating files");
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            if (!fileName.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase))
                continue;

            string relativePath =
                Path.GetRelativePath(Path.GetFullPath(Path.Combine(Server.GameDirectory, "csgo/cfg/")), file);

            configs.Add(new MapConfigFile(fileName[..fileName.LastIndexOf(".", StringComparison.Ordinal)], relativePath));
            DebugLogger.LogTrace($"[Map Config] Adding config {configs.Last().name}, {configs.Last().path}");
        }
    }

    private bool checkDirectoryExists()
    {
        if (!Directory.Exists(configFolder))
        {
            try
            {
                Logger.LogWarning(
                    $"Map config folder {configFolder} is not exists. Trying to create...");

                Directory.CreateDirectory(configFolder);
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to create map config folder!");
                Logger.LogError(e.StackTrace);
                return false;
            }
        }

        return true;
    }
}