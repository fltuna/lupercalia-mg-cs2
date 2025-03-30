using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore;

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
public class ExternalView : IPluginModule
{
    private LupercaliaMGCore m_CSSPlugin;

    public string PluginModuleName => "External View";

    private Dictionary<ulong, ExternalViewInfo> m_externalViewInfoMap = new();

    public ExternalView(LupercaliaMGCore plugin)
    {
        m_CSSPlugin = plugin;

        m_CSSPlugin.RegisterListener<Listeners.OnTick>(OnTick);
        m_CSSPlugin.RegisterEventHandler<EventRoundEnd>(onRoundEnd, HookMode.Post);

        m_CSSPlugin.AddCommand("css_tp", "Toggles third person camera mode.", CommandTp);
        m_CSSPlugin.AddCommand("css_tpp", "Toggles third person offset camera mode.", CommandTpp);
        m_CSSPlugin.AddCommand("css_watch", "Starts to watch other player. (CAUTION: You are still moving!)",
            CommandWatch);
        m_CSSPlugin.AddCommand("css_g", "Starts to watch other player. (CAUTION: You are still moving!)",
            CommandWatch);
    }

    public void AllPluginsLoaded()
    {
    }

    public void UnloadModule()
    {
        m_CSSPlugin.RemoveListener<Listeners.OnTick>(OnTick);
        m_CSSPlugin.DeregisterEventHandler<EventRoundEnd>(onRoundEnd, HookMode.Post);

        m_CSSPlugin.RemoveCommand("css_tp", CommandTp);
        m_CSSPlugin.RemoveCommand("css_tpp", CommandTpp);
        m_CSSPlugin.RemoveCommand("css_watch", CommandWatch);
        m_CSSPlugin.RemoveCommand("css_g", CommandWatch);
    }

    private bool isEnabled
    {
        get => PluginSettings.GetInstance.m_CVExternalViewEnabled.Value;
    }

    private void OnTick()
    {
        var instancesToRemove = new List<ulong>();
        foreach (var p in m_externalViewInfoMap)
        {
            if (!isEnabled || !p.Value.player.IsValid)
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

    private HookResult onRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        m_externalViewInfoMap.Clear();
        return HookResult.Continue;
    }

    private void DestroyExternalView(CCSPlayerController player)
    {
        var info = m_externalViewInfoMap[player.SteamID];

        ExternalViewUtils.DestroyExternalCamera(player, info.entCamera);
        m_externalViewInfoMap.Remove(player.SteamID);

        player.PrintToChat(m_CSSPlugin.Localizer["ExternalView.Command.Notification.RestoreDefaultView"]);
    }

    private void CommandTp(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive)
        {
            return;
        }

        if (!isEnabled)
        {
            player.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("ExternalView.Command.Notification.NotAvailable"));
            return;
        }

        var (dist, distStr) = ExternalViewUtils.ParseDistance(command, 1);

        CDynamicProp? entCamera;
        if (m_externalViewInfoMap.ContainsKey(player.SteamID))
        {
            var info = m_externalViewInfoMap[player.SteamID];
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

        player.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("ExternalView.Command.Notification.StartThirdPerson"));
    }

    private void CommandTpp(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive)
        {
            return;
        }

        if (!isEnabled)
        {
            player.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("ExternalView.Command.Notification.NotAvailable"));
            return;
        }

        var (dist, distStr) = ExternalViewUtils.ParseDistance(command, 1);

        CDynamicProp? entCamera;
        if (m_externalViewInfoMap.ContainsKey(player.SteamID))
        {
            var info = m_externalViewInfoMap[player.SteamID];
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

        float YAW_OFFSET = 15;
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

        player.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("ExternalView.Command.Notification.StartThirdPersonOffset"));
    }

    private void CommandWatch(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive)
        {
            return;
        }

        if (!isEnabled)
        {
            player.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("ExternalView.Command.Notification.NotAvailable"));
            return;
        }

        var shouldDisableWatch = false;
        CCSPlayerController? targetPlayer = null;

        if (command.ArgCount < 2)
        {
            player.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("ExternalView.Command.Notification.WatchTargetNameIsMissing"));
            shouldDisableWatch = true;
        }
        else
        {
            var targetStr = command.GetArg(1);

            if (targetStr.Length <= 1)
            {
                player.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("ExternalView.Command.Notification.WatchTargetNameIsTooShort"));
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
                    player.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("ExternalView.Command.Notification.WatchTargetNotFound"));
                    shouldDisableWatch = true;
                }
            }
        }

        var (dist, distStr) = ExternalViewUtils.ParseDistance(command, 2);
        var arg = $"{targetPlayer?.PlayerName}, {distStr}";

        CDynamicProp? entCamera;
        if (m_externalViewInfoMap.ContainsKey(player.SteamID))
        {
            var info = m_externalViewInfoMap[player.SteamID];
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
            if (weapons.ContainsKey(key))
            {
                weapons[key]++;
            }
            else
            {
                weapons.Add(key, 1);
            }
        }

        player.RemoveWeapons();

        var restoreWeapons = delegate()
        {
            weaponServices.PreventWeaponPickup = false;
            foreach (var weapon in weapons)
            {
                for (int i = 0; i < weapon.Value; i++)
                {
                    player.GiveNamedItem(weapon.Key);
                }
            }
        };

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
                    player.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("ExternalView.Command.Notification.WatchTargetIsDead"));
                    return false;
                }

                return true;
            },
            delegate() { restoreWeapons(); }
        );
        ExternalViewUtils.UpdateTargetCamera(player, targetPlayer, entCamera, dist);

        Server.PrintToChatAll(
            m_CSSPlugin.LocalizeStringWithPrefix("ExternalView.Command.Notification.WatchTarget", player.PlayerName, targetPlayer.PlayerName));
    }
}

public static class ExternalViewUtils
{
    static public (float, string) ParseDistance(CommandInfo command, int at)
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

    static public CDynamicProp CreateExternalCamera(CCSPlayerController player)
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

    static public void DestroyExternalCamera(CCSPlayerController player, CDynamicProp entCamera)
    {
        SetCameraEntity(player, null);

        if (entCamera.IsValid)
        {
            entCamera.Remove();
        }
    }

    static private void SetColor(CDynamicProp? ent, Color color)
    {
        if (ent == null || !ent.IsValid)
        {
            return;
        }

        ent.Render = color;
        Utilities.SetStateChanged(ent, "CBaseModelEntity", "m_clrRender");
    }

    static private void SetCameraEntity(CCSPlayerController player, CDynamicProp? camera)
    {
        var cameraHandle = camera?.EntityHandle.Raw ?? uint.MaxValue;
        player.PlayerPawn.Value!.CameraServices!.ViewEntity.Raw = cameraHandle;
        Utilities.SetStateChanged(player.PlayerPawn.Value, "CBasePlayerPawn", "m_pCameraServices");
    }

    static private Vector eyeOffset = new Vector(0, 0, 64);

    static private Vector CalculateThirdPersonOffset(QAngle angle, float distance)
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

    static public void UpdateThirdPersonCamera(CCSPlayerController player, CDynamicProp entCamera, float distance,
        float yawOffset = 0)
    {
        var pawn = player.PlayerPawn.Value!;
        var viewAngle = pawn!.V_angle;
        var cameraAngle = new QAngle(viewAngle.X, viewAngle.Y + yawOffset, viewAngle.Z);

        var offset = CalculateThirdPersonOffset(cameraAngle, distance);

        entCamera.Teleport(
            pawn.AbsOrigin! + eyeOffset + offset,
            viewAngle,
            Vector.Zero
        );
    }

    static public void UpdateTargetCamera(CCSPlayerController player, CCSPlayerController target,
        CDynamicProp entCamera, float distance)
    {
        var pawn = player.PlayerPawn.Value!;
        var angle = pawn!.V_angle;
        var offset = CalculateThirdPersonOffset(angle, distance);

        entCamera.Teleport(
            target.PlayerPawn.Value!.AbsOrigin! + eyeOffset + offset,
            angle,
            Vector.Zero
        );
    }
}