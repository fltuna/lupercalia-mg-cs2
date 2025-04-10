using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Configuration;
using TNCSSPluginFoundation.Models.Logger;

namespace LupercaliaMGCore;

public sealed class SimpleDebugLogger : AbstractDebugLoggerBase
{
    public readonly FakeConVar<int> DebugLogLevelConVar = new("lp_mg_debug_level",
        "0: Nothing, 1: Print info, warn, error message, 2: Print previous one and debug message, 3: Print previous one and trace message", 0, ConVarFlags.FCVAR_NONE,
        new RangeValidator<int>(0, 3));
    
    public readonly FakeConVar<bool> PrintToAdminClientsConsoleConVar = new("lp_mg_debug_show_console", "Debug message shown in client console?", false);
    
    public readonly FakeConVar<string> RequiredFlagForPrintToConsoleConVar = new ("lp_mg_debug_console_print_required_flag", "Required flag for print to client console", "css/generic");

    public override int DebugLogLevel => DebugLogLevelConVar.Value;
    
    public override bool PrintToAdminClientsConsole => PrintToAdminClientsConsoleConVar.Value;

    public override string RequiredFlagForPrintToConsole => RequiredFlagForPrintToConsoleConVar.Value;

    public override string LogPrefix => "[LPR MG]";

    private const string ModuleName = "DebugLogger";
    
    public SimpleDebugLogger(IServiceProvider serviceProvider)
    {
        var conVarService = serviceProvider.GetRequiredService<ConVarConfigurationService>();
        conVarService.TrackConVar(ModuleName, DebugLogLevelConVar);
        conVarService.TrackConVar(ModuleName, PrintToAdminClientsConsoleConVar);
        conVarService.TrackConVar(ModuleName, RequiredFlagForPrintToConsoleConVar);
    }
}