namespace LupercaliaMGCore.modules.ExternalView.Cameras
{
    public abstract class BaseCamera
    {
        protected readonly ICameraContext Ctx;

        protected BaseCamera(ICameraContext ctx)
        {
            Ctx = ctx;
        }

        /// <summary>
        /// Updates the camera entity via Teleport.
        /// 
        /// Also validates the camera to see if the camera is no longer available.
        /// </summary>
        /// <returns>Returns false if the camera should be revoked.</returns>
        public abstract bool Update();
    }
}
