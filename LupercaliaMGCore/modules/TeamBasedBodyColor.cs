using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using TNCSSPluginFoundation.Models.Plugin;

namespace LupercaliaMGCore.modules;

public sealed class TeamBasedBodyColor(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "TeamBasedBodyColor";
    
    public override string ModuleChatPrefix => "[TeamBasedBodyColor]";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    public readonly FakeConVar<bool> IsModuleEnabled =
        new("lp_mg_teamcolor_enabled", "Should apply team color after respawn", true);

    public readonly FakeConVar<string> ColorCt =
        new("lp_mg_teamcolor_ct", "Counter Terrorist's Body color. R, G, B", "0, 0, 255");

    public readonly FakeConVar<string> ColorT =
        new("lp_mg_teamcolor_t", "Terrorist's Body color. R, G, B", "255, 0, 0");
    
    
    protected override void OnInitialize()
    {
        TrackConVar(IsModuleEnabled);
        TrackConVar(ColorCt);
        TrackConVar(ColorT);
        Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
    }

    protected override void OnUnloadModule()
    {
        Plugin.DeregisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null || !player.IsValid)
            return HookResult.Continue;

        if (player.Team == CsTeam.None || player.Team == CsTeam.Spectator)
            return HookResult.Continue;

        DebugLogger.LogDebug($"[Team Based Body Color] [Player {player.PlayerName}] spawned");

        //
        // Default player color = A255 R255 G255 B255.
        //
        // We will compare `newColor` and `playerPawn.Render` to check if we need to send the model state change to clients.
        //
        // `playerPawn.Render` always returns an actual ARGB value rather than the color name (e.g., "white").
        // So keep your mind that if you assign `Color.White` to `newColor`, `playerPawn.Render != newColor` becomes
        // always negative since `Color.White` is not equal to `Color.FromArgb(255, 255, 255, 255)`.
        //
        Color newColor = Color.FromArgb(255, 255, 255, 255);
        RenderMode_t renderMode = RenderMode_t.kRenderNormal;

        if (IsModuleEnabled.Value)
        {
            // Use team color
            List<int> rgb = new List<int>();
            if (player.Team == CsTeam.CounterTerrorist)
            {
                rgb = ColorCt.Value.Split(',').Select(s => int.Parse(s.Trim()))
                    .ToList();
            }
            else if (player.Team == CsTeam.Terrorist)
            {
                rgb = ColorT.Value.Split(',').Select(s => int.Parse(s.Trim()))
                    .ToList();
            }

            newColor = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
            DebugLogger.LogTrace(
                $"[Team Based Body Color] Player {player.PlayerName}'s team: {player.Team}, color: {newColor.R} {newColor.G} {newColor.B}");

            renderMode = RenderMode_t.kRenderTransColor;
        }

        CBasePlayerPawn? playerPawn = player.PlayerPawn.Value;

        if (playerPawn == null)
            return HookResult.Continue;

        if (playerPawn.Render != newColor || playerPawn.RenderMode != renderMode)
        {
            DebugLogger.LogTrace(
                $"[Team Based Body Color] [Player {player.PlayerName}] render mode changed from {playerPawn.RenderMode} to {renderMode}");
            DebugLogger.LogTrace(
                $"[Team Based Body Color] [Player {player.PlayerName}] color changed from {playerPawn.Render} to {newColor}");
            playerPawn.RenderMode = renderMode;
            playerPawn.Render = newColor;

            DebugLogger.LogTrace($"[Team Based Body Color] [Player {player.PlayerName}] Sending state change.");
            Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
            DebugLogger.LogDebug($"[Team Based Body Color] [Player {player.PlayerName}] Done.");
        }
        else
        {
            DebugLogger.LogTrace(
                $"[Team Based Body Color] [Player {player.PlayerName}] Nothing has been changed.");
        }

        return HookResult.Continue;
    }
}