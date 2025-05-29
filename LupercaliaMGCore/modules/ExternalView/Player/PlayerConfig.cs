namespace LupercaliaMGCore.modules.ExternalView.Player
{
    /// <summary>
    /// Player preferences for the external view.
    /// 
    /// Preferences will be kept during the server is running.
    /// So players can restore their preferences on reconnection.
    /// 
    /// TODO: Use database to persist the preferences.
    /// </summary>
    public class PlayerConfig
    {
        /// <summary>
        /// The primary camera mode.
        /// </summary>
        public PrimaryCameraModes PrimaryCameraMode = PrimaryCameraModes.FirstPerson;

        /// <summary>
        /// The third-person camera distance from the eye position.
        /// </summary>
        public float ThirdPersonCameraDistance = Consts.DefaultCameraDistance;

        /// <summary>
        /// The third-person camera yaw offset angle in degrees.
        /// </summary>
        public float ThirdPersonCameraYawOffset = 0;
    }
}
