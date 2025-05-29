namespace LupercaliaMGCore.modules.ExternalView.Cameras
{
    public class FreeCamera : BaseFlyCamera
    {
        public FreeCamera(ICameraContext ctx) : base(ctx)
        {
        }

        protected override float CameraSpeed => Ctx.Api.ConVars.FreeCameraSpeed;

        protected override float AltCameraSpeed => Ctx.Api.ConVars.FreeCameraAltSpeed;

        public override bool Update()
        {
            var cameraEntity = Ctx.CameraEntity;
            if (cameraEntity == null)
                return false;

            var velocity = CalculateVelocity();
            var origin = cameraEntity.Origin;
            cameraEntity.Teleport(origin + velocity * Ctx.Api.DeltaTime, Ctx.Player.ViewAngle, velocity);

            return true;
        }
    }
}
