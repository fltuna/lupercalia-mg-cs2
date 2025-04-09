using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using LupercaliaMGCore.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LupercaliaMGCore.model;

/// <summary>
/// This is a base class for all plugin modules.
/// </summary>
/// <param name="plugin">Main plugin instance</param>
public abstract class PluginModuleBase(IServiceProvider serviceProvider) : PluginBasicFeatureBase(serviceProvider), IPluginModule
{
    /// <summary>
    /// Module Name
    /// </summary>
    public abstract string PluginModuleName { get; }
    
    public abstract string ModuleChatPrefix { get; } 

    /// <summary>
    /// ConVarManager
    /// </summary>
    private ConVarConfigurationService ConVarConfigurationService => Plugin.ConVarConfigurationService;
    
    public virtual void RegisterServices(IServiceCollection services) {}

    public virtual void UpdateServices(IServiceProvider services)
    {
        ServiceProvider = services;
    }

    /// <summary>
    /// Module initialization method (Internal)
    /// </summary>
    public void Initialize()
    {
        OnInitialize();
    }
    
    /// <summary>
    /// Module initialization method
    /// </summary>
    protected virtual void OnInitialize(){}


    /// <summary>
    /// Called when all plugins loaded (Internal)
    /// </summary>
    public void AllPluginsLoaded()
    {
        OnAllPluginsLoaded();
    }
    
    /// <summary>
    /// Called when all plugins loaded
    /// </summary>
    protected virtual void OnAllPluginsLoaded()
    {
        
    }

    /// <summary>
    /// Called when unloading module (Internal)
    /// </summary>
    public void UnloadModule()
    {
        OnUnloadModule();
        ConVarConfigurationService.UntrackModule(PluginModuleName);
    }

    
    /// <summary>
    /// Called when unloading module
    /// </summary>
    protected virtual void OnUnloadModule(){}
    
    
    /// <summary>
    /// Helper method for sending localized text to all players.
    /// </summary>
    /// <param name="localizationKey">Language localization key</param>
    /// <param name="args">Any args that can be use ToString()</param>
    protected void PrintLocalizedChatToAll(string localizationKey, params object[] args)
    {
        Server.PrintToChatAll(LocalizeWithPluginPrefix(localizationKey, args));
    }

    /// <summary>
    /// Helper method for obtain the localized text.
    /// </summary>
    /// <param name="localizationKey">Language localization key</param>
    /// <param name="args">Any args that can be use ToString()</param>
    /// <returns></returns>
    protected string LocalizeWithPluginPrefix(string localizationKey, params object[] args)
    {
        return Plugin.LocalizeStringWithPluginPrefix(localizationKey, args);
    }

    /// <summary>
    /// Helper method for obtain the localized text.
    /// </summary>
    /// <param name="localizationKey">Language localization key</param>
    /// <param name="args">Any args that can be use ToString()</param>
    /// <returns></returns>
    protected string LocalizeWithModulePrefix(string localizationKey, params object[] args)
    {
        return $"{ModuleChatPrefix} {LocalizeString(localizationKey, args)}";
    }


    /// <summary>
    /// Add ConVar to tracking list. if you want to generate config automatically, then call this method with ConVar that you wanted to track.
    /// </summary>
    /// <param name="conVar">Any FakeConVar</param>
    /// <typeparam name="T">FakeConVarType</typeparam>
    protected void TrackConVar<T>(FakeConVar<T> conVar) where T : IComparable<T>
    {
        ConVarConfigurationService.TrackConVar(PluginModuleName ,conVar);
    }


    /// <summary>
    /// Removes all module ConVar from tracking list.
    /// </summary>
    protected void UnTrackConVar()
    {
        ConVarConfigurationService.UntrackModule(PluginModuleName);
    }
}