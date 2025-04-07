using CounterStrikeSharp.API.Core;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore;

public abstract class OmikujiEventBase(Omikuji omikuji, LupercaliaMGCore plugin) : PluginBasicFeatureBase(plugin), IOmikujiEvent
{
    protected readonly Omikuji Omikuji = omikuji;
    protected readonly Random Random = new();
    
    public abstract string EventName { get; }
    public abstract OmikujiType OmikujiType { get; }
    public abstract OmikujiCanInvokeWhen OmikujiCanInvokeWhen { get; }
    
    public virtual void Execute(CCSPlayerController client) {}

    public virtual void Initialize() {}

    public virtual double GetOmikujiWeight()
    {
        return 0.0D;
    }

    protected string GetLocalizedString(string key)
    {
        return Plugin.Localizer[key];
    }
}