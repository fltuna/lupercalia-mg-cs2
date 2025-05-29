using System.Numerics;

namespace LupercaliaMGCore.modules.ExternalView
{
    public enum PrimaryCameraModes
    {
        FirstPerson,
        ThirdPerson,
    }

    internal class Consts
    {
        /// <summary>
        /// The eye height standing.
        /// </summary>
        public static readonly float EyeHeight = 62.0f;

        /// <summary>
        /// The eye offset position from player's origin.
        /// </summary>
        public static readonly Vector3 EyeOffset = new Vector3(0, 0, EyeHeight);

        /// <summary>
        /// The default yaw offset for shoulder view.
        /// </summary>
        public static readonly float ThirdPersonYawOffsetAngle = 15.0f;

        /// <summary>
        /// The default value of the third-person camera distance.
        /// </summary>
        public static readonly float DefaultCameraDistance = 80.0f;

        /// <summary>
        /// The initial camera distance when model view is activated.
        /// </summary>
        public static readonly float ModelViewInitialCameraDistance = 80.0f;

        /// <summary>
        /// The interval to check player's model to update the preview model in seconds.
        /// </summary>
        public static readonly float ModelViewPreviewModelCheckInterval = 1.0f;

        /// <summary>
        /// The acceleration factor for fly camera mode. Equivalent of `sv_specaccelerate` and `sv_noclipaccelerate`
        /// </summary>
        public static readonly float FlyCameraAccelerate = 5.0f;

        /// <summary>
        /// The friction factor for fly camera mode. Equivalent of `sv_friction`
        /// </summary>
        public static readonly float FlyCameraFriction = 5.2f;

        /// <summary>
        /// The time to wait after the observer target is dead.
        /// 
        /// Once the specified duration has elapsed, the observer target will be automatically changed.
        /// </summary>
        public static readonly float WatchCameraPostDeathWaitTime = 3.0f;
    }
}
