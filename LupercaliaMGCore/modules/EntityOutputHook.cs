using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using TNCSSPluginFoundation.Models.Plugin;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace LupercaliaMGCore.modules;

public class EntityOutputHook(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "EntityOutputHook";

    public override string ModuleChatPrefix => $" {ChatColors.Gold}[EntityOutputHook]{ChatColors.Default}";

    public readonly FakeConVar<bool> IsModuleEnabled = new("lp_mg_entity_output_hook", "Is EntityOutputHook enabled?", false);

    // it seems that these entities are mainly used when sending commands from the map.
    public readonly FakeConVar<string> ValidEntites = new("lp_mg_entity_output_hook_valid_entites", "Definition of the entity to be hooked. Comma separated.", "logic_auto,point_servercommand,func_button");

    public readonly FakeConVar<string> MapCommands = new("lp_mg_entity_output_hook_map_commands", "Definition of the map command to be hooked. Comma separated.", "");

    protected override void OnInitialize()
    {
        TrackConVar(IsModuleEnabled);
        TrackConVar(ValidEntites);
        TrackConVar(MapCommands);

        Plugin.HookEntityOutput("*", "*", Hook, HookMode.Post);
    }

    protected override void OnUnloadModule()
    {
        Plugin.UnhookEntityOutput("*", "*", Hook, HookMode.Post);
    }

    public HookResult Hook(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
    {
        if (caller?.DesignerName == null || IsModuleEnabled.Value || ValidEntites.Value.Length == 0 ||  MapCommands.Value.Length == 0) return HookResult.Continue;

        var validEntityNames = ValidEntites.Value.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        if (!validEntityNames.Contains(caller.DesignerName)) return HookResult.Continue;

        var cvarList = new Dictionary<(ConVar, string), float>();
        var ent = output.Connections;

        while (ent != null)
        {
            if (ent.TargetInput == "Command")
            {
                var raw = ent.ValueOverride.Replace("\"", "");
                var mapCommands = raw.Split(' ', 2); // command should be like a "say bluh" so

                if (mapCommands.Length > 0) // Check if there's at least a command
                {
                    var commandName = mapCommands[0];
                    var commandArgs = mapCommands.Length > 1 ? mapCommands[1] : ""; // Handle commands without arguments

                    // Check if the command is in the allowed list from MapCommands ConVar
                    var allowedMapCommands = MapCommands.Value.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                    if (allowedMapCommands.Any() && !allowedMapCommands.Contains(commandName))
                    {
                        // If MapCommands is defined and the current command is not in the list, skip it.
                        continue;
                    }

                    var cvar = ConVar.Find(commandName);

                    // Only process if it's a ConVar and requires cheat flag
                    if (cvar != null && cvar.Flags.HasFlag(ConVarFlags.FCVAR_CHEAT))
                    {
                        cvarList[(cvar, commandArgs)] = delay;
                    }
                }
            }
            ent = ent.Next;
        }

        if (cvarList.Count > 0)
        {
            SetCvar(cvarList);
        }

        return HookResult.Continue;
    }

    public void SetCvar(Dictionary<(ConVar, string), float> cvarList)
    {
        foreach (((ConVar, string) pair, float delay) in cvarList)
        {
            Action setCvarAction = () =>
            {
                DebugLogger.LogDebug($"[EntityOutputHook] sv_cheats ON");
                var cheats = ConVar.Find("sv_cheats");
                cheats?.SetValue(true);

                ParseAndSet(pair);

                cheats?.SetValue(false);
                DebugLogger.LogDebug($"[EntityOutputHook] sv_cheats OFF");
            };

            if (delay == 0.0f)
            {
                Server.NextFrame(setCvarAction);
            }
            else
            {
                new Timer(delay, setCvarAction);
            }
        }
    }

    public void ParseAndSet((ConVar, string) pair)
    {
        try
        {
            switch (pair.Item1.Type)
            {
                case ConVarType.Bool:
                    // Parse to bool
                    if (TryParseBool(pair.Item2, out bool boolValue))
                    {
                        pair.Item1.SetValue(boolValue);
                        DebugLogger.LogDebug($"[EntityOutputHook] {pair.Item1.Name} = {boolValue}");
                        return;
                    }
                    break;
                case ConVarType.Float32:
                case ConVarType.Float64:
                    // Parse to float
                    if (TryParseFloat(pair.Item2, out float floatValue))
                    {
                        pair.Item1.SetValue(floatValue);
                        DebugLogger.LogDebug($"[EntityOutputHook] {pair.Item1.Name} = {floatValue}");
                        return;
                    }
                    break;
                case ConVarType.Int16:
                case ConVarType.Int32:
                case ConVarType.Int64:
                    // Parse to int
                    if (TryParseInt(pair.Item2, out int intValue))
                    {
                        pair.Item1.SetValue(intValue);
                        DebugLogger.LogDebug($"[EntityOutputHook] {pair.Item1.Name} = {intValue}");
                        return;
                    }
                    break;
                case ConVarType.UInt16:
                case ConVarType.UInt32:
                case ConVarType.UInt64:
                    // Parse to uint
                    if (TryParseUint(pair.Item2, out uint uintValue))
                    {
                        pair.Item1.SetValue(uintValue);
                        DebugLogger.LogDebug($"[EntityOutputHook] {pair.Item1.Name} = {uintValue}");
                        return;
                    }
                    break;
                default:
                    // Default to string if all parsing fails
                    pair.Item1.StringValue = pair.Item2;
                    DebugLogger.LogDebug($"[EntityOutputHook] {pair.Item1.Name} = {pair.Item2}");
                    break;
            }
        }
        catch (Exception ex) { DebugLogger.LogDebug($"[EntityOutputHook] parse error. {pair.Item1.Name} = {pair.Item2}\n[EntityOutputHook] Raw error: {ex}"); }
    }

    // Utility methods for parsing
    public bool TryParseBool(string value, out bool result)
    {
        if (value == "0" || value == "1")
        {
            result = value == "1";
            return true;
        }
        result = false;
        return false;
    }

    public bool TryParseInt(string value, out int result)
    {
        return int.TryParse(value, out result);
    }

    public bool TryParseUint(string value, out uint result)
    {
        return uint.TryParse(value, out result);
    }

    public bool TryParseFloat(string value, out float result)
    {
        return float.TryParse(value, out result);
    }
}
