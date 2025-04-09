using LupercaliaMGCore.model;
using Microsoft.Extensions.DependencyInjection;

namespace LupercaliaMGCore.interfaces;

public interface ITunaPluginBase
{
    public ConVarConfigurationService ConVarConfigurationService { get; }
    
    public AbstractDebugLogger? DebugLogger { get; }
    
    public string ConVarConfigPath { get; }
    
    public string LocalizeStringWithPluginPrefix(string languageKey, params object[] args);
    
    public string LocalizeString(string languageKey, params object[] args);
}