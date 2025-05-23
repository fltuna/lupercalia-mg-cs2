using LupercaliaMGCore.modules.ExternalView.Utils;
using System.Numerics;

namespace LupercaliaMGCore.modules.ExternalView.Cameras
{
    public class ModelViewCamera : BaseFlyCamera
    {
        private Vector3? _LastRelPos = null;

        public ModelViewCamera(ICameraContext ctx) : base(ctx)
        {
        }

        protected override float CameraSpeed => Ctx.Api.ConVars.ModelViewCameraSpeed;

        protected override float AltCameraSpeed => Ctx.Api.ConVars.ModelViewCameraAltSpeed;

        public override bool Update()
        {
            var cameraEntity = Ctx.CameraEntity;
            if (cameraEntity == null)
                return false;

            if (Ctx.Player.Buttons.IsPressed(CounterStrikeSharp.API.PlayerButtons.Jump))
            {
                // Exit model view camera
                Ctx.Player.PrintToChat("ExternalView.ModelView.EndedByJump");
                return false;
            }

            if (_LastRelPos == null)
            {
                // Initialize the camera position
                _LastRelPos = MathUtils.CalculateThirdPersonOffset(Ctx.Player.ViewAngle, Consts.ModelViewInitialCameraDistance);
            }

            var velocity = CalculateVelocity();
            var relPos = velocity * Ctx.Api.DeltaTime + _LastRelPos!.Value;

            // Restrict the movement by the specified radius
            var distance = relPos.Length();
            if (distance > Ctx.Api.ConVars.ModelViewCameraRadius)
            {
                relPos *= Ctx.Api.ConVars.ModelViewCameraRadius / distance;
            }

            _LastRelPos = relPos;

            // Update the camera position
            var eyePos = Ctx.Player.Origin + Consts.EyeOffset;
            var absPos = eyePos + relPos;
            cameraEntity.Teleport(absPos, Ctx.Player.ViewAngle, null);

            return true;
        }
    }
}
