namespace LupercaliaMGCore.interfaces;

public interface IPluginModule
{
    string PluginModuleName { get; }

    public void Initialize();
    public void AllPluginsLoaded();
    public void UnloadModule();
}