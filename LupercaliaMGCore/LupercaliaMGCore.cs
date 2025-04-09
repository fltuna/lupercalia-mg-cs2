using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.interfaces;
using LupercaliaMGCore.model;
using LupercaliaMGCore.modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NativeVoteAPI.API;

namespace LupercaliaMGCore;

public sealed class LupercaliaMGCore : AbstractTunaPluginBase
{
    protected override string PluginPrefix =>
        $" {ChatColors.DarkRed}[{ChatColors.Blue}LPŘ MG{ChatColors.DarkRed}]{ChatColors.Default}";
    
    public override string ModuleName => "Lupercalia MG Core";

    public override string ModuleVersion => "1.5.0";

    public override string ModuleAuthor => "faketuna, Spitice";

    public override string ModuleDescription => "Provides core MG feature in CS2 with CounterStrikeSharp";

    public override string BaseCfgDirectoryPath => Path.Combine(Server.GameDirectory, "csgo/cfg/mgcore/");
    
    public override string ConVarConfigPath => Path.Combine(BaseCfgDirectoryPath, "mgcore.cfg");

    protected override void TunaOnPluginLoad(bool hotReload)
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
        RegisterModule<AntiCamp>(hotReload);
        RegisterModule<Omikuji>();
        RegisterModule<Debugging>();
        RegisterModule<MiscCommands>();
        RegisterModule<JoinTeamFix>();
        RegisterModule<HideLegs>();
        RegisterModule<ExternalView>();
        RegisterModule<CourseWeapons>();
        RegisterModule<VelocityDisplay>();
        RegisterModule<Rocket>();
        RebuildServiceProvider();

    }

    protected override void TunaOnPluginUnload(bool hotReload)
    {
    }

    protected override void RegisterRequiredPluginServices()
    {
        var debugLogger = new SimpleDebugLogger(ServiceProvider);
        RegisterFakeConVars(debugLogger.GetType(), debugLogger);
        ServiceCollection.AddSingleton<IDebugLogger>(debugLogger);
    }

    protected override void LateRegisterPluginServices()
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
            ServiceCollection.AddSingleton<INativeVoteApi>(nativeVoteApi);
        }
    }
}