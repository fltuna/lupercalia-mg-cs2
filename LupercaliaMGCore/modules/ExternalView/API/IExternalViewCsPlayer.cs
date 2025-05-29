using LupercaliaMGCore.modules.ExternalView.Utils;
using System.Numerics;

namespace LupercaliaMGCore.modules.ExternalView.API
{
    /// <summary>
    /// A wrapper interface for PlayerController and PlayerPawn class.
    /// </summary>
    public interface IExternalViewCsPlayer : IExternalViewCsEntity
    {
        int Slot { get; }

        string Name { get; }

        /// <summary>
        /// Returns if the player has admin privileges.
        /// </summary>
        bool IsAdmin { get; }

        /// <summary>
        /// Returns if the player is alive.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Returns if the player is in spectators.
        /// </summary>
        bool IsSpectator { get; }

        bool IsInWater { get; }

        /// <summary>
        /// Returns the time elapsed from last death.
        /// 
        /// Returns 0 if the player is alive.
        /// </summary>
        float TimeElapsedFromLastDeath { get; }

        Vector3 ViewAngle { get; }

        ButtonState Buttons { get; }

        IExternalViewCsEntity? ViewEntity { get; set; }

        bool IsMovable { set; }
        bool IsWeaponPickupEnabled { set; }
        IExternalViewCsWeapon? ActiveWeapon { get; }

        /// <summary>
        /// Call this function once per tick to update button state.
        /// </summary>
        void UpdateButtonState();

        /// <summary>
        /// Print the message to the player's chat.
        /// 
        /// The message can be a localizatoin key to show localized text.
        /// </summary>
        void PrintToChat(string message, params object[] args);
    }
}
