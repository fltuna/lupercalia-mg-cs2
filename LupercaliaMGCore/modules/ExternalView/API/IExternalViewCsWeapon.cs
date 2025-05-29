namespace LupercaliaMGCore.modules.ExternalView.API
{
    /// <summary>
    /// A wrapper interface for CBasePlayerWeapon class.
    /// </summary>
    public interface IExternalViewCsWeapon : IEquatable<IExternalViewCsWeapon>
    {
        bool IsValid { get; }

        bool CanAttack { set; }
    }
}
