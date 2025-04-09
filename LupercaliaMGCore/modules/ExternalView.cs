using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore.modules;

enum ExternalViewMode
{
    Default,
    OffsetTP,
    TargetView,
}

public delegate bool UpdateExternalViewDelegate();

public delegate void CleanupExternalViewDelegate();

record ExternalViewInfo(
    ExternalViewMode mode,
    string arg,
    CCSPlayerController player,
    CDynamicProp entCamera,
    UpdateExternalViewDelegate onUpdate,
    CleanupExternalViewDelegate? onCleanup = null
);

/**
 * Third person and custom view camera modes.
 *
 * Thirdperson feature is heavily based on "ThirdPerson-WIP" plugin by BoinK & UgurhanK.
 * Big thanks for sharing the awesome plugin!
 *
 * Check https://github.com/grrhn/ThirdPerson-WIP for more detail.
 */
public class ExternalView(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "External View";

    public override string ModuleChatPrefix => "[External View]";

    private Dictionary<ulong, ExternalViewInfo> m_externalViewInfoMap = new();

    
    public readonly FakeConVar<bool> IsModuleEnabled =
        new("lp_mg_external_view_enabled", "External view feature is enabled", false);
    
    protected override void OnInitialize()
    {
        TrackConVar(IsModuleEnabled);
        
        Plugin.RegisterListener<Listeners.OnTick>(OnTick);
        Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd, HookMode.Post);

        Plugin.AddCommand("css_tp", "Toggles third person camera mode.", CommandTp);
        Plugin.AddCommand("css_tpp", "Toggles third person offset camera mode.", CommandTpp);
        Plugin.AddCommand("css_watch", "Starts to watch other player. (CAUTION: You are still moving!)", CommandWatch);
        Plugin.AddCommand("css_g", "Starts to watch other player. (CAUTION: You are still moving!)", CommandWatch);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveListener<Listeners.OnTick>(OnTick);
        Plugin.DeregisterEventHandler<EventRoundEnd>(OnRoundEnd, HookMode.Post);

        Plugin.RemoveCommand("css_tp", CommandTp);
        Plugin.RemoveCommand("css_tpp", CommandTpp);
        Plugin.RemoveCommand("css_watch", CommandWatch);
        Plugin.RemoveCommand("css_g", CommandWatch);
    }

    private bool IsEnabled => IsModuleEnabled.Value;

    private void OnTick()
    {
        var instancesToRemove = new List<ulong>();
        foreach (var p in m_externalViewInfoMap)
        {
            if (!IsEnabled || !p.Value.player.IsValid)
            {
                instancesToRemove.Add(p.Key);
                continue;
            }

            try
            {
                var shouldContinue = p.Value.onUpdate();
                if (!shouldContinue)
                {
                    instancesToRemove.Add(p.Key);
                }
            }
            catch
            {
                // Got an exception. Something went wrong.
                instancesToRemove.Add(p.Key);
            }
        }

        foreach (var id in instancesToRemove)
        {
            var info = m_externalViewInfoMap[id];

            if (!info.player.IsValid)
            {
                // Just remove the instance and done
                m_externalViewInfoMap.Remove(id);
                continue;
            }

            DestroyExternalView(info.player);

            if (info.onCleanup != null)
            {
                info.onCleanup();
            }
        }
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        m_externalViewInfoMap.Clear();
        return HookResult.Continue;
    }

    private void DestroyExternalView(CCSPlayerController player)
    {
        var info = m_externalViewInfoMap[player.SteamID];

        ExternalViewUtils.DestroyExternalCamera(player, info.entCamera);
        m_externalViewInfoMap.Remove(player.SteamID);

        player.PrintToChat(LocalizeWithPluginPrefix("ExternalView.Command.Notification.RestoreDefaultView"));
    }

    private void CommandTp(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive)
        {
            return;
        }

        if (!IsEnabled)
        {
            player.PrintToChat(LocalizeWithPluginPrefix("ExternalView.Command.Notification.NotAvailable"));
            return;
        }

        var (dist, distStr) = ExternalViewUtils.ParseDistance(command, 1);

        CDynamicProp? entCamera;
        if (m_externalViewInfoMap.TryGetValue(player.SteamID, out var info))
        {
            if (info.onCleanup != null)
            {
                info.onCleanup();
            }

            if (info.mode == ExternalViewMode.Default && info.arg == distStr)
            {
                DestroyExternalView(player);
                return;
            }

            entCamera = info.entCamera;
        }
        else
        {
            // Create the external camera
            entCamera = ExternalViewUtils.CreateExternalCamera(player);
        }

        m_externalViewInfoMap[player.SteamID] = new ExternalViewInfo(
            ExternalViewMode.Default,
            distStr,
            player,
            entCamera,
            delegate()
            {
                ExternalViewUtils.UpdateThirdPersonCamera(player, entCamera, dist);
                return true;
            }
        );
        ExternalViewUtils.UpdateThirdPersonCamera(player, entCamera, dist);

        player.PrintToChat(LocalizeWithPluginPrefix("ExternalView.Command.Notification.StartThirdPerson"));
    }

    private void CommandTpp(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive)
        {
            return;
        }

        if (!IsEnabled)
        {
            player.PrintToChat(LocalizeWithPluginPrefix("ExternalView.Command.Notification.NotAvailable"));
            return;
        }

        var (dist, distStr) = ExternalViewUtils.ParseDistance(command, 1);

        CDynamicProp? entCamera;
        if (m_externalViewInfoMap.TryGetValue(player.SteamID, out var info))
        {
            if (info.onCleanup != null)
            {
                info.onCleanup();
            }

            if (info.mode == ExternalViewMode.OffsetTP && info.arg == distStr)
            {
                DestroyExternalView(player);

                return;
            }

            entCamera = info.entCamera;
        }
        else
        {
            // Create the external camera
            entCamera = ExternalViewUtils.CreateExternalCamera(player);
        }

        const float YAW_OFFSET = 15;
        m_externalViewInfoMap[player.SteamID] = new ExternalViewInfo(
            ExternalViewMode.OffsetTP,
            distStr,
            player,
            entCamera,
            delegate()
            {
                ExternalViewUtils.UpdateThirdPersonCamera(player, entCamera, dist, YAW_OFFSET);
                return true;
            }
        );
        ExternalViewUtils.UpdateThirdPersonCamera(player, entCamera, dist, YAW_OFFSET);

        player.PrintToChat(LocalizeWithPluginPrefix("ExternalView.Command.Notification.StartThirdPersonOffset"));
    }

    private void CommandWatch(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive)
        {
            return;
        }

        if (!IsEnabled)
        {
            player.PrintToChat(LocalizeWithPluginPrefix("ExternalView.Command.Notification.NotAvailable"));
            return;
        }

        var shouldDisableWatch = false;
        CCSPlayerController? targetPlayer = null;

        if (command.ArgCount < 2)
        {
            player.PrintToChat(LocalizeWithPluginPrefix("ExternalView.Command.Notification.WatchTargetNameIsMissing"));
            shouldDisableWatch = true;
        }
        else
        {
            var targetStr = command.GetArg(1);

            if (targetStr.Length <= 1)
            {
                player.PrintToChat(LocalizeWithPluginPrefix("ExternalView.Command.Notification.WatchTargetNameIsTooShort"));
                shouldDisableWatch = true;
            }
            else
            {
                foreach (CCSPlayerController client in Utilities.GetPlayers())
                {
                    if (client == player)
                    {
                        continue;
                    }

                    if (client.PlayerName.ToLower().Contains(targetStr.ToLower()))
                    {
                        if (client.Team == player.Team)
                        {
                            targetPlayer = client;
                            break;
                        }
                    }
                }

                if (targetPlayer == null)
                {
                    player.PrintToChat(LocalizeWithPluginPrefix("ExternalView.Command.Notification.WatchTargetNotFound"));
                    shouldDisableWatch = true;
                }
            }
        }

        var (dist, distStr) = ExternalViewUtils.ParseDistance(command, 2);
        var arg = $"{targetPlayer?.PlayerName}, {distStr}";

        CDynamicProp? entCamera;
        if (m_externalViewInfoMap.TryGetValue(player.SteamID, out var info))
        {
            if (info.onCleanup != null)
            {
                info.onCleanup();
            }

            if (info.mode == ExternalViewMode.TargetView && info.arg == arg || shouldDisableWatch)
            {
                DestroyExternalView(player);
                return;
            }

            entCamera = info.entCamera;
        }
        else
        {
            if (shouldDisableWatch)
            {
                // Do nothing
                return;
            }

            // Create the external camera
            entCamera = ExternalViewUtils.CreateExternalCamera(player);
        }

        if (targetPlayer == null)
        {
            return;
        }

        // Lock weapons
        var weapons = new Dictionary<string, int>();
        var weaponServices = player.PlayerPawn.Value!.WeaponServices!;
        weaponServices.PreventWeaponPickup = true;
        foreach (var weapon in weaponServices.MyWeapons)
        {
            var key = weapon.Value!.DesignerName!;
            if (!weapons.TryAdd(key, 1))
            {
                weapons[key]++;
            }
        }

        player.RemoveWeapons();

        void RestoreWeapons()
        {
            weaponServices.PreventWeaponPickup = false;
            foreach (var weapon in weapons)
            {
                for (int i = 0; i < weapon.Value; i++)
                {
                    player.GiveNamedItem(weapon.Key);
                }
            }
        }

        m_externalViewInfoMap[player.SteamID] = new ExternalViewInfo(
            ExternalViewMode.TargetView,
            arg,
            player,
            entCamera,
            delegate()
            {
                ExternalViewUtils.UpdateTargetCamera(player, targetPlayer, entCamera, dist);
                if (!targetPlayer.PawnIsAlive || targetPlayer.Team != player.Team)
                {
                    player.PrintToChat(LocalizeWithPluginPrefix("ExternalView.Command.Notification.WatchTargetIsDead"));
                    return false;
                }

                return true;
            },
            RestoreWeapons
        );
        ExternalViewUtils.UpdateTargetCamera(player, targetPlayer, entCamera, dist);

        PrintLocalizedChatToAll("ExternalView.Command.Notification.WatchTarget", player.PlayerName, targetPlayer.PlayerName);
    }
}

public static class ExternalViewUtils
{
    public static (float, string) ParseDistance(CommandInfo command, int at)
    {
        var distance = 80.0f;
        if (command.ArgCount > at)
        {
            var distI = -1;
            int.TryParse(command.GetArg(at), out distI);
            if (distI != -1)
            {
                distance = Math.Clamp(distI, 50, 250);
            }
        }

        return (distance, distance.ToString());
    }

    public static CDynamicProp CreateExternalCamera(CCSPlayerController player)
    {
        CDynamicProp? entCamera = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");

        if (entCamera == null)
        {
            throw new Exception("[ExternalView] Failed to spawn an entity for external view.");
        }

        entCamera.DispatchSpawn();
        SetColor(entCamera, Color.FromArgb(0, 255, 255, 255));

        // You can attach the camera to the player pawn, but it may emphasize
        // the oscillation of the camera when you turn your character.
        // We want to stabilize the camera view rather than the character model for MG
        // so we dare not to attach it to the player.
        //entCamera.AcceptInput("SetParent", player.PlayerPawn.Value, null, "!activator");

        SetCameraEntity(player, entCamera);

        return entCamera;
    }

    public static void DestroyExternalCamera(CCSPlayerController player, CDynamicProp entCamera)
    {
        SetCameraEntity(player, null);

        if (entCamera.IsValid)
        {
            entCamera.Remove();
        }
    }

    private static void SetColor(CDynamicProp? ent, Color color)
    {
        if (ent == null || !ent.IsValid)
        {
            return;
        }

        ent.Render = color;
        Utilities.SetStateChanged(ent, "CBaseModelEntity", "m_clrRender");
    }

    private static void SetCameraEntity(CCSPlayerController player, CDynamicProp? camera)
    {
        var cameraHandle = camera?.EntityHandle.Raw ?? uint.MaxValue;
        player.PlayerPawn.Value!.CameraServices!.ViewEntity.Raw = cameraHandle;
        Utilities.SetStateChanged(player.PlayerPawn.Value, "CBasePlayerPawn", "m_pCameraServices");
    }

    private static readonly Vector EyeOffset = new Vector(0, 0, 64);

    private static Vector CalculateThirdPersonOffset(QAngle angle, float distance)
    {
        float yawRad = MathUtil.ToRad(angle.Y);
        float pitchRad = MathUtil.ToRad(angle.X);

        var x = (float)Math.Cos(yawRad);
        var y = (float)Math.Sin(yawRad);

        var horzFactor = (float)Math.Cos(pitchRad);
        var vertFactor = (float)Math.Sin(pitchRad);

        var yawDir = new Vector(-x, -y, 0);
        var pitchDir = new Vector(0, 0, 1);

        var dir = yawDir * horzFactor + pitchDir * vertFactor;

        return dir * distance;
    }

    public static void UpdateThirdPersonCamera(CCSPlayerController player, CDynamicProp entCamera, float distance, float yawOffset = 0)
    {
        var pawn = player.PlayerPawn.Value!;
        var viewAngle = pawn!.V_angle;
        var cameraAngle = new QAngle(viewAngle.X, viewAngle.Y + yawOffset, viewAngle.Z);

        var offset = CalculateThirdPersonOffset(cameraAngle, distance);

        entCamera.Teleport(
            pawn.AbsOrigin! + EyeOffset + offset,
            viewAngle,
            Vector.Zero
        );
    }

    public static void UpdateTargetCamera(CCSPlayerController player, CCSPlayerController target, CDynamicProp entCamera, float distance)
    {
        var pawn = player.PlayerPawn.Value!;
        var angle = pawn!.V_angle;
        var offset = CalculateThirdPersonOffset(angle, distance);

        entCamera.Teleport(
            target.PlayerPawn.Value!.AbsOrigin! + EyeOffset + offset,
            angle,
            Vector.Zero
        );
    }
}