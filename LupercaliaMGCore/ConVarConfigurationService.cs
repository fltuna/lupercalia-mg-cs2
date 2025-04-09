using CounterStrikeSharp.API.Modules.Cvars;
using LupercaliaMGCore.model;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class ConVarConfigurationService(AbstractTunaPluginBase plugin)
{
    private readonly Dictionary<string, List<object>> _moduleConVars = new();
    
    public void TrackConVar<T>(string moduleName, FakeConVar<T> conVar) where T : IComparable<T>
    {
        if (!_moduleConVars.TryGetValue(moduleName, out var list))
        {
            list = new List<object>();
            _moduleConVars[moduleName] = list;
        }
        
        list.Add(conVar);
    }
    
    public void SaveModuleConfigToFile(string moduleName)
    {
        string moduleConfigPath = Path.Combine(plugin.BaseCfgDirectoryPath, moduleName + ".cfg");
        
        if(IsFileExists(moduleConfigPath))
            return;
        
        
        if (!_moduleConVars.TryGetValue(moduleName, out var list))
            return;

        using (StreamWriter writer = new StreamWriter(moduleConfigPath))
        {
            foreach (var conVarObj in list)
            {
                dynamic conVar = conVarObj;
                writer.WriteLine($"// {conVar.Description}");
                
                // If value is boolean, then convert it to 0|1
                if (conVarObj.GetType().GenericTypeArguments[0] == typeof(bool))
                {
                    bool value = conVar.Value;
                    writer.WriteLine($"{conVar.Name} {Convert.ToInt32(value)}");
                }
                else
                {
                    writer.WriteLine($"{conVar.Name} {conVar.Value}");
                }
                
                writer.WriteLine();
            }
        }
    }
    
    public void SaveAllConfigToFile()
    {
        if(IsFileExists(plugin.ConVarConfigPath))
            return;

        using (StreamWriter writer = new StreamWriter(plugin.ConVarConfigPath))
        {
            foreach (var moduleName in _moduleConVars.Keys)
            {
                writer.WriteLine($"// ===== {moduleName} =====");
                writer.WriteLine();
                
                foreach (var conVarObj in _moduleConVars[moduleName])
                {
                    dynamic conVar = conVarObj;
                    writer.WriteLine($"// {conVar.Description}");
                    
                    if (conVarObj.GetType().GenericTypeArguments[0] == typeof(bool))
                    {
                        bool value = conVar.Value;
                        writer.WriteLine($"{conVar.Name} {Convert.ToInt32(value)}");
                    }
                    else
                    {
                        writer.WriteLine($"{conVar.Name} {conVar.Value}");
                    }
                    
                    writer.WriteLine();
                }
                
                writer.WriteLine();
            }
        }
    }

    private bool IsFileExists(string path)
    {
        string directory = Path.GetDirectoryName(path)!;
        if (!Directory.Exists(directory))
        {
            plugin.Logger.LogInformation($"Failed to find the config folder. Trying to generate...");
                
            Directory.CreateDirectory(directory);

            if (!Directory.Exists(directory))
            {
                plugin.Logger.LogError($"Failed to generate the Config folder! cancelling the config generation!");
                return false;
            }
        }

        return File.Exists(path);
    }
    
    
    public void UntrackModule(string moduleName)
    {
        _moduleConVars.Remove(moduleName);
    }
}