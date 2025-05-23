using CounterStrikeSharp.API;
using LupercaliaMGCore.modules.ExternalView.Utils;
using System.Numerics;

namespace LupercaliaMGCore.modules.ExternalView.Cameras
{
    public abstract class BaseFlyCamera : BaseCamera
    {
        private Vector3 _LastVelocity = Vector3.Zero;

        public BaseFlyCamera(ICameraContext ctx) : base(ctx)
        {
        }

        /// <summary>
        /// The camera speed.
        /// </summary>
        protected abstract float CameraSpeed { get; }

        /// <summary>
        /// The camera speed when the speed (walk) key is down.
        /// </summary>
        protected abstract float AltCameraSpeed { get; }

        protected Vector3 CalculateVelocity()
        {
            var moveX = MoveX;
            var moveY = MoveY;

            var isMoving = Math.Abs(moveX) + Math.Abs(moveY) > 0.001;
            var isSpeedKeyDown = Ctx.Player.Buttons.IsDown(PlayerButtons.Speed);

            // Calculate the moving direction
            var wishDir = Vector3.Zero;
            if (isMoving)
            {
                var viewAngle = Ctx.Player.ViewAngle;
                var quatYaw = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathUtils.ToRad(viewAngle.Y));
                var quatPitch = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathUtils.ToRad(viewAngle.X));
                var viewQuat = Quaternion.Multiply(quatYaw, quatPitch);

                var rightDir = Vector3.Transform(-Vector3.UnitY, viewQuat);
                var forwardDir = Vector3.Transform(Vector3.UnitX, viewQuat);

                var wishDirNonNormalized = rightDir * moveX + forwardDir * moveY;
                wishDir = Vector3.Normalize(wishDirNonNormalized);
            }

            // Moving speed
            var wishSpeed = 0.0f;
            if (isMoving)
            {
                wishSpeed = CameraSpeed;
                if (isSpeedKeyDown)
                {
                    wishSpeed = AltCameraSpeed;
                }
            }

            // Accelerate
            var currentSpeedInWishDir = Vector3.Dot(_LastVelocity, wishDir);
            var addSpeed = wishSpeed - currentSpeedInWishDir;
            var accelerationScale = Math.Max(250, wishSpeed);
            var accelSpeed = Consts.FlyCameraAccelerate * Ctx.Api.DeltaTime * accelerationScale;
            addSpeed = Math.Min(accelSpeed, addSpeed);
            var velocity = _LastVelocity + addSpeed * wishDir;

            // Decelerate
            var speed = velocity.Length();
            if (speed < 1.0f)
            {
                // Stop
                velocity = Vector3.Zero;
            }
            else
            {
                var control = Math.Max(speed, CameraSpeed * 0.25f);
                var drop = control * Consts.FlyCameraFriction * Ctx.Api.DeltaTime;
                var deceleratedSpeed = Math.Max(speed - drop, 0);
                var speedScale = deceleratedSpeed / speed;
                velocity *= speedScale;
            }

            _LastVelocity = velocity;
            return velocity;
        }

        private float MoveX
        {
            get
            {
                var buttons = Ctx.Player.Buttons;
                var x = 0.0f;

                if (buttons.IsDown(PlayerButtons.Moveright))
                {
                    x += 1.0f;
                }
                if (buttons.IsDown(PlayerButtons.Moveleft))
                {
                    x -= 1.0f;
                }

                return x;
            }
        }

        private float MoveY
        {
            get
            {
                var buttons = Ctx.Player.Buttons;
                var y = 0.0f;

                if (buttons.IsDown(PlayerButtons.Forward))
                {
                    y += 1.0f;
                }
                if (buttons.IsDown(PlayerButtons.Back))
                {
                    y -= 1.0f;
                }

                return y;
            }
        }
    }
}
