using LupercaliaMGCore.interfaces;
using LupercaliaMGCore.model;
using Microsoft.Extensions.DependencyInjection;

namespace LupercaliaMGCore;

public class ClassTemplate(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
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
    
    
    // This method will call while registering module.
    // Module registration is called in plugins Load method.
    public override void RegisterServices(IServiceCollection services)
    {
    }

    
    // This method will call while BasePlugin's OnAllPluginsLoaded.
    // This serviceProvider should contain latest and all module dependency.
    public override void UpdateServices(IServiceProvider services)
    {
    }


    // This method will call while registering module.
    // Module registration is called in plugins Load method.
    // Also, this time you can call TrackConVar() method to add specific ConVar to ConVar config file automatic generation
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