using CounterStrikeSharp.API;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore.model;

/// <summary>
/// This is a base class for all plugin modules.
/// </summary>
/// <param name="plugin">Main plugin instance</param>
public abstract class PluginModuleBase(LupercaliaMGCore plugin) : PluginBasicFeatureBase(plugin), IPluginModule
{
    /// <summary>
    /// Module Name
    /// </summary>
    public abstract string PluginModuleName { get; }

    
    
    /// <summary>
    /// Module initialization method
    /// </summary>
    public virtual void Initialize(){}
    
    /// <summary>
    /// Called when all plugins loaded
    /// </summary>
    public virtual void AllPluginsLoaded(){}
    
    /// <summary>
    /// Called when unloading module
    /// </summary>
    public virtual void UnloadModule(){}

    
    /// <summary>
    /// Helper method for sending localized text to all players.
    /// </summary>
    /// <param name="localizationKey">Language localization key</param>
    /// <param name="args">Any args that can be use ToString()</param>
    protected void PrintLocalizedChatToAll(string localizationKey, params object[] args)
    {
        Server.PrintToChatAll(LocalizeWithPrefix(localizationKey, args));
    }

    /// <summary>
    /// Helper method for obtain the localized text.
    /// </summary>
    /// <param name="localizationKey">Language localization key</param>
    /// <param name="args">Any args that can be use ToString()</param>
    /// <returns></returns>
    protected string LocalizeWithPrefix(string localizationKey, params object[] args)
    {
        return Plugin.LocalizeStringWithPrefix(localizationKey, args);
    }
}