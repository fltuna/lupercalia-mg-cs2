using CounterStrikeSharp.API.Core;
using LupercaliaMGCore.model;
using LupercaliaMGCore.modules;
using Microsoft.Extensions.DependencyInjection;

namespace LupercaliaMGCore;

public abstract class OmikujiEventBase(IServiceProvider serviceProvider) : PluginBasicFeatureBase(serviceProvider), IOmikujiEvent
{
    protected readonly Omikuji Omikuji = serviceProvider.GetRequiredService<Omikuji>();
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

    protected string LocalizeOmikujiResult(CCSPlayerController drawer, OmikujiType type, string languageKey, params object[] args)
    {
        return $"{Omikuji.ModuleChatPrefix} {Omikuji.GetOmikujiLuckMessage(type, drawer)} {LocalizeString(languageKey, args)}";
    }

    protected string LocalizeWithPrefix(string languageKey, params object[] args)
    {
        return $"{Omikuji.ModuleChatPrefix} {LocalizeString(languageKey, args)}";
    }
}