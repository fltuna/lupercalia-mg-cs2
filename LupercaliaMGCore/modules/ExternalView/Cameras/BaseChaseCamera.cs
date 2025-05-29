using LupercaliaMGCore.modules.ExternalView.API;
using LupercaliaMGCore.modules.ExternalView.Utils;
using System.Numerics;

namespace LupercaliaMGCore.modules.ExternalView.Cameras
{
    /// <summary>
    /// The base class for chase (third person) cameras.
    /// </summary>
    public abstract class BaseChaseCamera : BaseCamera
    {
        public BaseChaseCamera(ICameraContext ctx) : base(ctx)
        {
        }

        abstract protected IExternalViewCsPlayer? Target { get; }
        abstract protected float Distance { get; }
        abstract protected float YawOffset { get; }

        public override bool Update()
        {
            if (Target == null)
                return false;

            var targetPosition = Target.Origin + Consts.EyeOffset;
            var targetVelocity = Target.Velocity;
            var viewAngle = Ctx.Player.ViewAngle;

            //
            // Add yaw offset to calculate third-person offset
            // - positive angle = offsets the camera to the right side
            // - negative angle = offsets the camera to the left side
            //
            var angle = viewAngle + new Vector3(0, YawOffset, 0);

            var offset = MathUtils.CalculateThirdPersonOffset(angle, Distance);
            var cameraPosition = targetPosition + offset;

            Ctx.CameraEntity?.Teleport(cameraPosition, viewAngle, targetVelocity);

            return true;
        }
    }
}
