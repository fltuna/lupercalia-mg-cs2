using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace LupercaliaMGCore.modules.AntiCamp.Models;

public class AntiCampGlowingEntity(IServiceProvider provider, CCSPlayerController controller): PluginBasicFeatureBase(provider)
{
    public CBaseModelEntity? GlowingEntity { get; private set; }
    public CBaseModelEntity? RelayEntity { get; private set; }

    public bool CreateEntity()
    {
        DebugLogger.LogDebug($"[Anti Camp] [Player {controller.PlayerName}] Start player glow");
        CCSPlayerPawn playerPawn = controller.PlayerPawn.Value!;

        DebugLogger.LogDebug($"[Anti Camp] [Player {controller.PlayerName}] Creating overlay entity.");
        CBaseModelEntity? modelGlow = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");
        CBaseModelEntity? modelRelay = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");

        if (modelGlow == null || modelRelay == null)
        {
            DebugLogger.LogDebug($"[Anti Camp] [Player {controller.PlayerName}] Failed to create glowing entity!");
            return false;
        }


        string playerModel = PlayerUtil.GetPlayerModel(controller);

        DebugLogger.LogTrace($"[Anti Camp] [Player {controller.PlayerName}] player model: {playerModel}");
        if (playerModel == string.Empty)
            return false;

        // Code from Joakim in CounterStrikeSharp Discord
        // https://discord.com/channels/1160907911501991946/1235212931394834432/1245928951449387009
        DebugLogger.LogTrace($"[Anti Camp] [Player {controller.PlayerName}] Setting player model to overlay entity.");
        modelRelay.SetModel(playerModel);
        modelRelay.Spawnflags = 256u;
        modelRelay.RenderMode = RenderMode_t.kRenderNone;
        modelRelay.DispatchSpawn();

        modelGlow.SetModel(playerModel);
        modelGlow.Spawnflags = 256u;
        modelGlow.RenderMode = RenderMode_t.kRenderTransColor;
        modelGlow.Render = Color.FromArgb(1, 255, 255, 255);
        modelGlow.DispatchSpawn();

        DebugLogger.LogTrace($"[Anti Camp] [Player {controller.PlayerName}] Changing overlay entity's render mode.");
        modelGlow.Glow.GlowColorOverride = controller.Team == CsTeam.Terrorist ? Color.Red : Color.Blue;

        modelGlow.Glow.GlowRange = 5000;
        modelGlow.Glow.GlowTeam = -1;
        modelGlow.Glow.GlowType = 3;
        modelGlow.Glow.GlowRangeMin = 0;

        modelRelay.AcceptInput("FollowEntity", playerPawn, modelRelay, "!activator");
        modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");

        GlowingEntity = modelGlow;
        RelayEntity = modelRelay;
        return true;
    }

    public void RemoveEntity()
    {
        if (GlowingEntity != null)
        {
            if (GlowingEntity.IsValid)
            {
                GlowingEntity.Remove();
            }
            GlowingEntity = null;
        }

        if (RelayEntity != null)
        {
            if (RelayEntity.IsValid)
            {
                RelayEntity.Remove();
            }
            RelayEntity = null;
        }
    }
}