using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using LupercaliaMGCore.modules.ExternalView.API;

namespace LupercaliaMGCore.modules.ExternalView.CSSharp
{
    internal class ExternalViewCsWeapon : IExternalViewCsWeapon
    {
        private readonly CBasePlayerWeapon _Weapon;

        public ExternalViewCsWeapon(CBasePlayerWeapon weapon)
        {
            _Weapon = weapon;
        }

        public bool IsValid => _Weapon.IsValid;

        public bool CanAttack
        {
            set
            {
                // int.MaxValue means never able to shoot the weapon.
                var nextAttackTick = value ? Server.TickCount : int.MaxValue;

                _Weapon.NextPrimaryAttackTick = nextAttackTick;
                _Weapon.NextSecondaryAttackTick = nextAttackTick;

                Utilities.SetStateChanged(_Weapon, "CBasePlayerWeapon", "m_nNextPrimaryAttackTick");
                Utilities.SetStateChanged(_Weapon, "CBasePlayerWeapon", "m_nNextSecondaryAttackTick");
            }
        }

        public bool Equals(IExternalViewCsWeapon? other)
        {
            var otherWeapon = other as ExternalViewCsWeapon;

            if (otherWeapon == null)
                return false;

            return _Weapon.Handle == otherWeapon._Weapon.Handle;
        }
    }
}
