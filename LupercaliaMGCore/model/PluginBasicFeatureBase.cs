using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore.model;

public abstract class PluginBasicFeatureBase(IServiceProvider serviceProvider)
{
    protected readonly AbstractTunaPluginBase Plugin = serviceProvider.GetRequiredService<AbstractTunaPluginBase>();
    protected readonly ILogger Logger = serviceProvider.GetRequiredService<AbstractTunaPluginBase>().Logger;
    
    
    protected string LocalizeString(string localizationKey, params object[] args)
    {
        return Plugin.LocalizeString(localizationKey, args);
    }
}