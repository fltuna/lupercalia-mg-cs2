using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore.model;

public abstract class PluginBasicFeatureBase(LupercaliaMGCore plugin)
{
    protected readonly LupercaliaMGCore Plugin = plugin;
    protected readonly ILogger Logger = plugin.Logger;
    protected readonly PluginSettings PluginSettings = PluginSettings.GetInstance;
}