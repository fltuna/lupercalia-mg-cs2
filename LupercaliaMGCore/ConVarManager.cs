using CounterStrikeSharp.API.Modules.Cvars;

namespace LupercaliaMGCore;

public class ConVarManager(string conVarConfigPath)
{
    // 設定出力のためだけにConVarを追跡
    private readonly Dictionary<string, List<object>> _moduleConVars = new();
    
    // ConVarを追跡リストに追加（管理はしない）
    public void TrackConVar<T>(string moduleName, FakeConVar<T> conVar) where T : IComparable<T>
    {
        if (!_moduleConVars.TryGetValue(moduleName, out var list))
        {
            list = new List<object>();
            _moduleConVars[moduleName] = list;
        }
        
        list.Add(conVar);
    }
    
    // モジュールの設定をファイルに保存
    public void SaveModuleConfigToFile(string moduleName)
    {
        if (!_moduleConVars.TryGetValue(moduleName, out var list))
            return;

        using (StreamWriter writer = new StreamWriter(conVarConfigPath))
        {
            foreach (var conVarObj in list)
            {
                dynamic conVar = conVarObj;
                writer.WriteLine($"// {conVar.Description}");
                
                // boolの場合は0/1に変換
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
    
    // 全モジュールの設定をファイルに保存
    public void SaveAllConfigToFile()
    {
        using (StreamWriter writer = new StreamWriter(conVarConfigPath))
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
    
    // モジュールの追跡を解除
    public void UntrackModule(string moduleName)
    {
        _moduleConVars.Remove(moduleName);
    }
}