using LupercaliaMGCore.model;

namespace LupercaliaMGCore;

public class ClassTemplate: IPluginModule
{
    private LupercaliaMGCore m_CSSPlugin;

    public ClassTemplate(LupercaliaMGCore plugin)
    {
        m_CSSPlugin = plugin;
    }

    public string PluginModuleName => "TEMPLATE";
    
    public void AllPluginsLoaded()
    {
    }

    public void UnloadModule()
    {
    }
}