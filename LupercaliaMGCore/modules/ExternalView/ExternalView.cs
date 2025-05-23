using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using LupercaliaMGCore.modules.ExternalView.API;
using LupercaliaMGCore.modules.ExternalView.CSSharp;
using TNCSSPluginFoundation.Models.Plugin;

/**
 * Third person and custom view camera modes.
 * 
 * Including the following camera modes:
 * 
 * - Third person
 * - Third person (offseted to right/left)
 * - Model view
 * - Free camera (a.k.a., noclip)
 * - Watch other players
 * 
 * Requires ExternalViewHelper metamod plugin to fix shoot/+use location on third peron camera.
 * Check https://github.com/spitice/cs2-external-view for more detail.
 *
 * This module is inspired by "ThirdPerson-WIP" plugin by BoinK & UgurhanK.
 * Big thanks for sharing the awesome plugin!
 * https://github.com/grrhn/ThirdPerson-WIP
 */
namespace LupercaliaMGCore.modules.ExternalView
{
    public sealed class ExternalView(IServiceProvider serviceProvider)
        : PluginModuleBase(serviceProvider)
        , IExternalViewConVars
        , ILocalizer
    {
        public override string PluginModuleName => "External View";
        public override string ModuleChatPrefix => "[External View]";
        protected override bool UseTranslationKeyInModuleChatPrefix => false;

        private ExternalViewSystem? _System;
        private AttackAndUsePositionFixer? _PositionFixer;

        // ConVars
        public readonly FakeConVar<bool> IsModuleEnabled =
            new("lp_mg_external_view_enabled", "External view feature is enabled", true);

        public readonly FakeConVar<float> ConVar_ThirdPersonMinDistance =
            new("lp_mg_external_view_thirdperson_min_distance", "The minimum camera distance for third-person camera.", 50);
        public readonly FakeConVar<float> ConVar_ThirdPersonMaxDistance =
            new("lp_mg_external_view_thirdperson_max_distance", "The maximum camera distance for third-person camera.", 200);
        public readonly FakeConVar<float> ConVar_ModelViewCameraSpeed =
            new("lp_mg_external_view_modelview_speed", "The speed of model view camera movement", 160);
        public readonly FakeConVar<float> ConVar_ModelViewCameraAltSpeed =
            new("lp_mg_external_view_modelview_alt_speed", "The speed of model view camera movement while walk button pressed", 40);
        public readonly FakeConVar<float> ConVar_ModelViewCameraRadius =
            new("lp_mg_external_view_modelview_radius", "The radius from the player the model view camera can fly around", 120);
        public readonly FakeConVar<float> ConVar_FreeCameraSpeed =
            new("lp_mg_external_view_freecam_speed", "The speed of free camera movement", 800);
        public readonly FakeConVar<float> ConVar_FreeCameraAltSpeed =
            new("lp_mg_external_view_freecam_alt_speed", "The speed of free camera movement while walk button pressed", 2400);
        public readonly FakeConVar<bool> ConVar_IsObserverViewEnabled =
            new("lp_mg_external_view_observer_enabled", "True if observer views (i.e., freecam and watch) are enabled for non-admin players.", true);
        public readonly FakeConVar<bool> ConVar_IsAdminPrevilegesEnabled =
            new("lp_mg_external_view_admin_previleges_enabled", "True if admins can use all features regardless of the flags (e.g., IsObserverViewEnabled)", true);

        float IExternalViewConVars.ThirdPersonMinDistance => ConVar_ThirdPersonMinDistance.Value;
        float IExternalViewConVars.ThirdPersonMaxDistance => ConVar_ThirdPersonMaxDistance.Value;
        float IExternalViewConVars.ModelViewCameraSpeed => ConVar_ModelViewCameraSpeed.Value;
        float IExternalViewConVars.ModelViewCameraAltSpeed => ConVar_ModelViewCameraAltSpeed.Value;
        float IExternalViewConVars.ModelViewCameraRadius => ConVar_ModelViewCameraRadius.Value;
        float IExternalViewConVars.FreeCameraSpeed => ConVar_FreeCameraSpeed.Value;
        float IExternalViewConVars.FreeCameraAltSpeed => ConVar_FreeCameraAltSpeed.Value;
        bool IExternalViewConVars.IsObserverViewEnabled => ConVar_IsObserverViewEnabled.Value;
        bool IExternalViewConVars.IsAdminPrevilegesEnabled => ConVar_IsAdminPrevilegesEnabled.Value;

        string ILocalizer.LocalizeForPlayer(CCSPlayerController controller, string message, params object[] args)
        {
            return LocalizeWithModulePrefixForPlayer(controller, message, args);
        }

        protected override void OnInitialize()
        {
            Plugin.RegisterListener<Listeners.OnTick>(OnTick);
            Plugin.RegisterEventHandler<EventRoundPrestart>(OnRoundStart, HookMode.Post);
            Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd, HookMode.Post);
            Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, HookMode.Post);
            Plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath, HookMode.Post);

            TrackConVar(IsModuleEnabled);
            TrackConVar(ConVar_ThirdPersonMinDistance);
            TrackConVar(ConVar_ThirdPersonMaxDistance);
            TrackConVar(ConVar_ModelViewCameraSpeed);
            TrackConVar(ConVar_ModelViewCameraRadius);
            TrackConVar(ConVar_FreeCameraSpeed);
            TrackConVar(ConVar_IsObserverViewEnabled);

            Plugin.AddCommand("css_tp", "Toggles third person camera mode.", CommandThirdPerson);
            Plugin.AddCommand("css_tpp", "Toggles third person offset camera mode (right handed).", CommandThirdPersonOffsetRightHanded);
            Plugin.AddCommand("css_tpq", "Toggles third person offset camera mode (left handed).", CommandThirdPersonOffsetLeftHanded);
            Plugin.AddCommand("css_mv", "Toggles model view camera mode.", CommandModelView);
            Plugin.AddCommand("css_freecam", "Toggles free camera mode.", CommandFreeCam);
            Plugin.AddCommand("css_fc", "Toggles free camera mode.", CommandFreeCam);
            Plugin.AddCommand("css_watch", "Starts to watche other player.", CommandWatch);
            Plugin.AddCommand("css_g", "Starts to watche other player.", CommandWatch);
            Plugin.AddCommand("css_camdist", "Changes the third person camera distance.", CommandCamDist);

            _PositionFixer = new AttackAndUsePositionFixer(Logger, (interval, action) => Plugin.AddTimer(interval, action), GetPlayersForPositonFix);
        }

        protected override void OnUnloadModule()
        {
            _PositionFixer?.Unload();

            IsEnabled = false;

            Plugin.RemoveListener<Listeners.OnTick>(OnTick);
            Plugin.DeregisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, HookMode.Post);
            Plugin.DeregisterEventHandler<EventPlayerDeath>(OnPlayerDeath, HookMode.Post);

            Plugin.RemoveCommand("css_tp", CommandThirdPerson);
            Plugin.RemoveCommand("css_tpp", CommandThirdPersonOffsetRightHanded);
            Plugin.RemoveCommand("css_tpq", CommandThirdPersonOffsetLeftHanded);
            Plugin.RemoveCommand("css_mv", CommandModelView);
            Plugin.RemoveCommand("css_freecam", CommandFreeCam);
            Plugin.RemoveCommand("css_fc", CommandFreeCam);
            Plugin.RemoveCommand("css_watch", CommandWatch);
            Plugin.RemoveCommand("css_g", CommandWatch);
            Plugin.RemoveCommand("css_camdist", CommandCamDist);
        }

        IEnumerable<CCSPlayerController?> GetPlayersForPositonFix()
        {
            if (_System == null)
            {
                return Enumerable.Empty<CCSPlayerController?>();
            }

            return _System.Players.Select(player => Utilities.GetPlayerFromSlot(player.Player.Slot));
        }

        private bool IsEnabled
        {
            get => _System != null;
            set {
                if (value == IsEnabled)
                    return;

                if (_System == null)
                {
                    _System = new ExternalViewSystem(new ExternalViewCsApi(this, this));
                }
                else
                {
                    _System.Unload();
                    _System = null;
                }
            }
        }

        private void OnTick()
        {
            IsEnabled = IsModuleEnabled.Value;
            _System?.Update();
        }

        private HookResult OnRoundStart(EventRoundPrestart @event, GameEventInfo info)
        {
            _System?.Unload();
            return HookResult.Continue;
        }

        private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            _System?.Unload();
            return HookResult.Continue;
        }

        private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            TryToActivatePrimaryCamera(@event.Userid);
            return HookResult.Continue;
        }

        private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            TryToActivatePrimaryCamera(@event.Userid);
            return HookResult.Continue;
        }

        private void CommandThirdPerson(CCSPlayerController? controller, CommandInfo command)
        {
            TryToToggleThirdPerson(controller, command, 0);
        }

        private void CommandThirdPersonOffsetRightHanded(CCSPlayerController? controller, CommandInfo command)
        {
            TryToToggleThirdPerson(controller, command, Consts.ThirdPersonYawOffsetAngle);
        }

        private void CommandThirdPersonOffsetLeftHanded(CCSPlayerController? controller, CommandInfo command)
        {
            TryToToggleThirdPerson(controller, command, -Consts.ThirdPersonYawOffsetAngle);
        }

        private void CommandModelView(CCSPlayerController? controller, CommandInfo command)
        {
            if (controller == null)
                return;

            _System?.ToggleModelView(controller.SteamID);
        }

        private void CommandFreeCam(CCSPlayerController? controller, CommandInfo command)
        {
            TryToToggleFreeCam(controller);
        }

        private void CommandWatch(CCSPlayerController? controller, CommandInfo command)
        {
            if (controller == null)
                return;

            var target = ParseWatchTarget(command);
            _System?.ToggleWatch(controller.SteamID, target);
        }

        private void CommandCamDist(CCSPlayerController? controller, CommandInfo command)
        {
            if (controller == null)
                return;

            float? cameraDistance = ParseCameraDistance(command);
            _System?.SetThirdPersonCameraDistance(controller.SteamID, cameraDistance);
        }

        private void TryToActivatePrimaryCamera(CCSPlayerController? controller)
        {
            if (controller == null)
                return;

            if (controller.IsBot)
                return;

            _System?.ActivatePrimaryCamera(controller.SteamID);
        }

        private void TryToToggleThirdPerson(CCSPlayerController? controller, CommandInfo command, float yawOffset)
        {
            if (controller == null)
                return;

            float? cameraDistance = ParseCameraDistance(command);
            _System?.ToggleThirdPerson(controller.SteamID, cameraDistance, yawOffset);
        }

        private void TryToToggleFreeCam(CCSPlayerController? controller)
        {
            if (controller == null)
                return;

            _System?.ToggleFreeCam(controller.SteamID);
        }

        private float? ParseCameraDistance(CommandInfo command)
        {
            if (command.ArgCount < 2)
                return null;

            float cameraDist;
            if (!float.TryParse(command.GetArg(1), out cameraDist))
                return null;

            return cameraDist;
        }

        private string? ParseWatchTarget(CommandInfo command)
        {
            if (command.ArgCount < 2)
                return null;

            return command.GetArg(1);
        }
    }
}
