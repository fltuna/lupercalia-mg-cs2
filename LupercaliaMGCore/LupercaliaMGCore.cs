using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.modules;
using LupercaliaMGCore.modules.AntiCamp;
using LupercaliaMGCore.modules.ExternalView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NativeVoteAPI.API;
using TNCSSPluginFoundation;

namespace LupercaliaMGCore;

[MinimumApiVersion(334)]
public sealed class LupercaliaMGCore : TncssPluginBase
{
    public override string PluginPrefix =>
        $" {ChatColors.DarkRed}[{ChatColors.Blue}LPŘ MG{ChatColors.DarkRed}]{ChatColors.Default}";

    public override bool UseTranslationKeyInPluginPrefix => false;

    public override string ModuleName => "Lupercalia MG Core";

    public override string ModuleVersion => "1.7.0";

    public override string ModuleAuthor => "faketuna, Spitice, uru, Zeisen";

    public override string ModuleDescription => "Provides core MG feature in CS2 with CounterStrikeSharp";

    public override string BaseCfgDirectoryPath => Path.Combine(Server.GameDirectory, "csgo/cfg/mgcore/");
    
    public override string ConVarConfigPath => Path.Combine(BaseCfgDirectoryPath, "mgcore.cfg");

    protected override void TncssOnPluginLoad(bool hotReload)
    {
        RegisterModule<TeamBasedBodyColor>();
        RegisterModule<DuckFix>();
        RegisterModule<TeamScramble>();
        RegisterModule<VoteMapRestart>();
        RegisterModule<VoteRoundRestart>();
        RegisterModule<RoundEndDamageImmunity>();
        RegisterModule<RoundEndWeaponStrip>();
        RegisterModule<RoundEndDeathMatch>();
        RegisterModule<ScheduledShutdown>();
        RegisterModule<Respawn>();
        RegisterModule<MapConfig>();
        RegisterModule<AntiCampModule>(hotReload);
        RegisterModule<Omikuji>();
        RegisterModule<Debugging>();
        RegisterModule<MiscCommands>();
        RegisterModule<JoinTeamFix>();
        RegisterModule<HideLegs>();
        RegisterModule<ExternalView>();
        RegisterModule<CourseWeapons>();
        RegisterModule<VelocityDisplay>();
        RegisterModule<Rocket>();
        RegisterModule<EntityOutputHook>();
        RegisterModule<SpawnPointDuplicator>();
        RegisterModule<GrenadePickupFix>();
    }

    protected override void RegisterRequiredPluginServices(IServiceCollection collection, IServiceProvider services)
    {
        DebugLogger = new SimpleDebugLogger(services);
    }

    protected override void LateRegisterPluginServices(IServiceCollection serviceCollection, IServiceProvider provider)
    {
        INativeVoteApi? nativeVoteApi = null;
        try
        {
            nativeVoteApi = INativeVoteApi.Capability.Get();
        }
        catch (Exception)
        {
            Logger.LogError("Native vote API not found! some modules may not work properly!!!!");
        }

        if (nativeVoteApi != null)
        {
            serviceCollection.AddSingleton<INativeVoteApi>(nativeVoteApi);
        }
    }
}