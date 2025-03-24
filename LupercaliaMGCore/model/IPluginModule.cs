namespace LupercaliaMGCore.model;

public interface IPluginModule
{
    string PluginModuleName { get; }

    public void AllPluginsLoaded();
    public void UnloadModule();
}