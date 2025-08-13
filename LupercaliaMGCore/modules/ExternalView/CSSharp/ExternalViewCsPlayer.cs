using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.modules.ExternalView.API;
using LupercaliaMGCore.modules.ExternalView.Utils;
using System.Numerics;

namespace LupercaliaMGCore.modules.ExternalView.CSSharp
{
    internal class ExternalViewCsPlayer
        : ExternalViewCsEntity
        , IExternalViewCsPlayer
    {
        private CCSPlayerController _Controller;
        private ButtonState _Buttons = new ButtonState();

        private readonly ILocalizer _localizer;

        private CBasePlayerPawn? _Pawn => _Controller.PlayerPawn.Value;

        public ExternalViewCsPlayer(CCSPlayerController controller, ILocalizer localizer)
            : base(() => controller.PlayerPawn.Value!)
        {
            _Controller = controller;
            _localizer = localizer;
        }

        public new bool IsValid => _Controller.IsValid;

        public int Slot => _Controller.Slot;

        public string Name => _Controller.PlayerName;

        public bool IsAdmin => AdminManager.PlayerHasPermissions(_Controller, "@css/root");

        public bool IsAlive => _Controller.PawnIsAlive;

        public bool IsSpectator => _Controller.Team != CsTeam.Terrorist && _Controller.Team != CsTeam.CounterTerrorist;

        public bool IsInWater => ((PlayerFlags)(_Pawn?.Flags ?? 0)).HasFlag(PlayerFlags.FL_INWATER);

        public float TimeElapsedFromLastDeath
        {
            get
            {
                if (_Controller.PawnIsAlive)
                {
                    return 0;
                }

                var pawn = _Controller.PlayerPawn.Value;
                if (pawn == null)
                {
                    return 0;
                }

                var lastDeathAt = pawn.DeathTime;
                return Server.CurrentTime - lastDeathAt;
            }
        }

        public Vector3 ViewAngle => MathUtils.ToVector3(_Controller.PlayerPawn.Value?.V_angle);

        public ButtonState Buttons => _Buttons;

        public IExternalViewCsEntity? ViewEntity
        {
            get
            {
                var viewEntityHandle = _Pawn?.CameraServices?.ViewEntity;
                if (viewEntityHandle == null || viewEntityHandle.Raw == uint.MaxValue)
                {
                    return null;
                }

                var viewEntity = viewEntityHandle.Value;
                if (viewEntity == null)
                {
                    return null;
                }

                return new ExternalViewCsEntity(viewEntity);
            }

            set
            {
                var nextViewEntityHandleRaw = uint.MaxValue;
                if (value != null)
                {
                    nextViewEntityHandleRaw = value.HandleRaw;
                }

                var pawn = _Pawn;
                if (pawn == null)
                    return;

                var viewEntityHandle = pawn.CameraServices?.ViewEntity;
                if (viewEntityHandle == null)
                    return;

                viewEntityHandle.Raw = nextViewEntityHandleRaw;
                Utilities.SetStateChanged(pawn, "CBasePlayerPawn", "m_pCameraServices");
            }
        }

        public bool IsMovable
        {
            set
            {
                var pawn = _Pawn;
                if (pawn == null)
                {
                    return;
                }

                if (value)
                {
                    pawn.Flags &= ~(uint)PlayerFlags.FL_ATCONTROLS;
                    pawn.MoveType = MoveType_t.MOVETYPE_WALK;
                }
                else
                {
                    //
                    // As of AnimGraph2 update, ATCONTROLS no longer works as expected;
                    // once you rotate the camera, the player character sometimes starts moving w/ player's WASD input.
                    // (might be a bug)
                    // To prevent this, use ATCONTROLS flag with MOVETYPE_OBSERVER.
                    //
                    // NOTE: I've tried the following things but none of them worked:
                    //
                    // - Using `FL_ONTRAIN` flag
                    // - Using `FL_FROZEN` flag (it completely stops the camera movement as well so we cannot use it)
                    // - Setting `MOVETYPE_NONE`, or `MOVETYPE_LADDER`
                    // - Setting `pawn.MovementServices.Maxspeed` as 0
                    //
                    pawn.Flags |= (uint)PlayerFlags.FL_ATCONTROLS;
                    pawn.MoveType = MoveType_t.MOVETYPE_OBSERVER;  // Somehow it worked, noice.
                }

                Utilities.SetStateChanged(pawn, "CBaseEntity", "m_fFlags");
                Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
            }
        }

        public bool IsWeaponPickupEnabled
        {
            set
            {
                var weaponService = _Pawn?.WeaponServices;
                if (weaponService == null)
                    return;

                weaponService.PreventWeaponPickup = !value;
            }
        }

        public IExternalViewCsWeapon? ActiveWeapon
        {
            get
            {
                var weapon = _Pawn?.WeaponServices?.ActiveWeapon.Value;
                if (weapon == null)
                    return null;

                return new ExternalViewCsWeapon(weapon);
            }
        }

        public void UpdateButtonState()
        {
            _Buttons.Update(_Controller.Buttons);
        }

        public void PrintToChat(string message, params object[] args)
        {
            var localizedText = _localizer.LocalizeForPlayer(_Controller, message, args);
            _Controller.PrintToChat(localizedText);
        }
    }
}
