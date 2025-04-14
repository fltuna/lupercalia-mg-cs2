using CounterStrikeSharp.API.Core;
using TNCSSPluginFoundation.Models.Plugin;

namespace LupercaliaMGCore.modules.AntiCamp.Models;

public class AntiCampGlowingService(IServiceProvider provider, CCSPlayerController controller): PluginBasicFeatureBase(provider)
{
    private AntiCampGlowingEntity GlowingEntity { get; } = new AntiCampGlowingEntity(provider, controller);

    public bool StartGlow()
    {
        return GlowingEntity.CreateEntity();
    }

    public void StopGlow()
    {
        GlowingEntity.RemoveEntity();
    }

    public bool IsGlowing()
    {
        return GlowingEntity.GlowingEntity != null && GlowingEntity.RelayEntity != null;
    }
}