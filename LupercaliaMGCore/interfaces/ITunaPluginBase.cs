using Microsoft.Extensions.DependencyInjection;

namespace LupercaliaMGCore.interfaces;

public interface ITunaPluginBase
{
    public ConVarManager ConVarManager { get; }
    
    public string ConVarConfigPath { get; }
    
    public string LocalizeStringWithPluginPrefix(string languageKey, params object[] args);
    
    public string LocalizeString(string languageKey, params object[] args);
}