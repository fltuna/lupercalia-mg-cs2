using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace LupercaliaMGCore.modules.omikuji.events;

public abstract class OmikujiEventBase(IServiceProvider serviceProvider) : PluginBasicFeatureBase(serviceProvider), IOmikujiEvent
{
    private readonly Omikuji Omikuji = serviceProvider.GetRequiredService<Omikuji>();
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
        return $"{Omikuji.ModuleChatPrefix} {Omikuji.GetOmikujiLuckMessage(type, drawer)} {LocalizeString(drawer, languageKey, args)}";
    }

    protected string LocalizeWithPrefix(CCSPlayerController? translationTarget, string languageKey, params object[] args)
    {
        return $"{Omikuji.ModuleChatPrefix} {LocalizeString(translationTarget, languageKey, args)}";
    }
    
    /// <summary>
    /// Add ConVar to tracking list. if you want to generate config automatically, then call this method with ConVar that you wanted to track.
    /// </summary>
    /// <param name="conVar">Any FakeConVar</param>
    /// <typeparam name="T">FakeConVarType</typeparam>
    protected void TrackConVar<T>(FakeConVar<T> conVar) where T : IComparable<T>
    {
        Plugin.ConVarConfigurationService.TrackConVar(EventName ,conVar);
    }
    
}