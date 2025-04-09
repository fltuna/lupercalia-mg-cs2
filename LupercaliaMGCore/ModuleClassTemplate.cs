using LupercaliaMGCore.model;
using Microsoft.Extensions.DependencyInjection;

namespace LupercaliaMGCore;

public class ModuleClassTemplate(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "ClassTemplate";

    public override string ModuleChatPrefix => "[ClassTemplate]";

    // public FakeConVar<float> VariableName = new(
    //     "convar_name",
    //     "Description",
    //     DefaultValue,
    //     ConVarFlags.FCVAR_NONE,
    //     new RangeValidator<float>(0.0F, float.MaxValue));
    //
    // You can register ConVar in Module.
    //
    // If you want to add this ConVar to ConVar config file automatic generation
    // You need to call TrackConVar() method in OnInitialize()
    //
    //
    
    
    
    /// <summary>
    /// This method will call while registering module, and module registration is called in plugins Load method.
    /// Also, we can get ConVarConfigurationService and AbstractTunaPluginBase from DI container from this method.
    /// </summary>
    /// <param name="services">ServiceCollection</param>
    public override void RegisterServices(IServiceCollection services)
    {
    }

    
    
    /// <summary>
    /// This method will call while BasePlugin's OnAllPluginsLoaded.
    /// This serviceProvider should contain latest and all module dependency.
    /// </summary>
    /// <param name="services">Latest DI container</param>
    public override void UpdateServices(IServiceProvider services)
    {
    }


    
    /// <summary>
    /// This method will call while registering module, and module registration is called from plugin's Load method.
    /// Also, this time you can call TrackConVar() method to add specific ConVar to ConVar config file automatic generation.
    /// </summary>
    protected override void OnInitialize()
    {
        // For instance
        // TrackConVar(ConVarVariableName);
    }
    
    // This method will call in end of PluginModuleBase::AllPluginsLoaded()
    protected override void OnAllPluginsLoaded()
    {
    }

    
    // This method will call in end of PluginModuleBase::UnloadModule()
    protected override void OnUnloadModule()
    {
    }
}