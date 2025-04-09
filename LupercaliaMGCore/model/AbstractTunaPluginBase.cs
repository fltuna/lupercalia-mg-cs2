using CounterStrikeSharp.API.Core;
using LupercaliaMGCore.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore.model;

public abstract class AbstractTunaPluginBase: BasePlugin, ITunaPluginBase
{
    public ConVarConfigurationService ConVarConfigurationService { get; private set; } = null!;

    public abstract string ConVarConfigPath { get; }

    protected ServiceCollection ServiceCollection { get; } = new();
    
    public ServiceProvider ServiceProvider = null!;

    protected abstract string PluginPrefix { get; }


    protected virtual void RegisterRequiredPluginServices(){}
    
    protected virtual void LateRegisterPluginServices(){}

    private void UpdateServices()
    {
        foreach (PluginModuleBase module in loadedModules)
        {
            module.UpdateServices(ServiceProvider);
        }
    }

    public sealed override void Load(bool hotReload)
    {
        ConVarConfigurationService = new(ConVarConfigPath);
        // Add self and core service to DI Container
        ServiceCollection.AddSingleton(this);
        RegisterRequiredPluginServices();
        
        // Build first ServiceProvider, because we need a plugin instance to initialize modules
        RebuildServiceProvider();
        
        // Call customizable OnLoad method
        TunaOnPluginLoad(hotReload);
    }

    protected virtual void TunaOnPluginLoad(bool hotReload){}
    
    
    public sealed override void OnAllPluginsLoaded(bool hotReload)
    {
        LateRegisterPluginServices();
        RebuildServiceProvider();
        UpdateServices();
        
        TunaAllPluginsLoaded(hotReload);
        CallModulesAllPluginsLoaded();
    }
    
    protected virtual void TunaAllPluginsLoaded(bool hotReload){}

    
    public sealed override void Unload(bool hotReload)
    {
        TunaOnPluginUnload(hotReload);
        UnloadAllModules();
    }
    
    protected virtual void TunaOnPluginUnload(bool hotReload){}


    protected void RebuildServiceProvider()
    {
        ServiceProvider = ServiceCollection.BuildServiceProvider();
    }
    
    public string LocalizeStringWithPluginPrefix(string languageKey, params object[] args)
    {
        return $"{PluginPrefix} {LocalizeString(languageKey, args)}";
    }

    public string LocalizeString(string languageKey, params object[] args)
    {
        return Localizer[languageKey, args];
    }
    

    private readonly HashSet<PluginModuleBase> loadedModules = [];

    protected void RegisterModule<T>() where T : PluginModuleBase
    {
        var module = (T)Activator.CreateInstance(typeof(T), ServiceProvider)!;
        loadedModules.Add(module);
        module.RegisterServices(ServiceCollection);
        module.Initialize();
        RegisterFakeConVars(module.GetType(), module);
        Logger.LogInformation($"{module.PluginModuleName} has been initialized");
    }

    protected void RegisterModule<T>(bool hotReload) where T : PluginModuleBase
    {
        var module = (T)Activator.CreateInstance(typeof(T), ServiceProvider, hotReload)!;
        loadedModules.Add(module);
        module.RegisterServices(ServiceCollection);
        module.Initialize();
        Logger.LogInformation($"{module.PluginModuleName} has been initialized");
    }

    protected void CallModulesAllPluginsLoaded()
    {
        foreach (IPluginModule loadedModule in loadedModules)
        {
            loadedModule.AllPluginsLoaded();
        }
    }
    

    protected void UnloadAllModules()
    {
        foreach (PluginModuleBase loadedModule in loadedModules)
        {
            loadedModule.UnloadModule();
            Logger.LogInformation($"{loadedModule.PluginModuleName} has been unloaded.");
        }
    }
}