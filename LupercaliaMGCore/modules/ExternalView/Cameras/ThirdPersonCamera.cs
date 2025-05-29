using LupercaliaMGCore.modules.ExternalView.API;

namespace LupercaliaMGCore.modules.ExternalView.Cameras
{
    public class ThirdPersonCamera : BaseChaseCamera
    {
        public ThirdPersonCamera(ICameraContext ctx)
            : base(ctx)
        {
        }

        protected override IExternalViewCsPlayer? Target => Ctx.Player;

        protected override float Distance => Ctx.Config.ThirdPersonCameraDistance;

        protected override float YawOffset => Ctx.Config.ThirdPersonCameraYawOffset;
    }
}
