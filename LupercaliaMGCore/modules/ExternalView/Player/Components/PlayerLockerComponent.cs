using LupercaliaMGCore.modules.ExternalView.API;

namespace LupercaliaMGCore.modules.ExternalView.Player.Components
{
    /// <summary>
    /// Locks player movement and weapons.
    /// 
    /// Locked players can still be moved by other things (e.g., trigger_push, moving platforms, and teleport).
    /// </summary>
    public class PlayerLockerComponent : BasePlayerComponent
    {
        private IExternalViewCsWeapon? _LastWeapon;

        public PlayerLockerComponent(IExternalViewCsPlayer player) : base(player)
        {
        }

        public override void OnEnable()
        {
            Player.IsMovable = false;
            Player.IsWeaponPickupEnabled = false;

            var weapon = Player.ActiveWeapon;
            if (weapon != null)
            {
                weapon.CanAttack = false;
                _LastWeapon = weapon;
            }
        }

        public override void OnDisable()
        {
            Player.IsMovable = true;
            Player.IsWeaponPickupEnabled = true;

            if (_LastWeapon != null)
            {
                _LastWeapon.CanAttack = true;
                _LastWeapon = null;
            }
        }

        public override void OnUpdate()
        {
            var weapon = Player.ActiveWeapon;
            
            if (weapon == null && _LastWeapon == null)
                return;

            if (weapon != null && _LastWeapon != null)
                if (weapon.Equals(_LastWeapon))
                    return;

            // Weapon has been changed
            if (weapon != null)
            {
                weapon.CanAttack = false;
            }

            if (_LastWeapon != null)
            {
                _LastWeapon.CanAttack = true;
            }

            _LastWeapon = weapon;
        }
    }
}
