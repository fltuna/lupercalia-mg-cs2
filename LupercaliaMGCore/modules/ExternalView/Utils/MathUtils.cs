using CounterStrikeSharp.API.Modules.Utils;
using System.Numerics;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace LupercaliaMGCore.modules.ExternalView.Utils
{
    public class MathUtils
    {
        public static float ToRad(float value)
        {
            return value * (float)Math.PI / 180.0f;
        }


        public static Vector3 ToVector3(Vector? v)
        {
            if (v == null)
            {
                return Vector3.Zero;
            }

            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector3 ToVector3(QAngle? v)
        {
            if (v == null)
            {
                return Vector3.Zero;
            }

            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector? ToVectorOrNull(Vector3? v)
        {
            if (v == null)
            {
                return null;
            }

            return ToVector(v.Value);
        }

        public static Vector ToVector(Vector3 v)
        {
            return new Vector(v.X, v.Y, v.Z);
        }

        public static QAngle? ToQAngleOrNull(Vector3? v)
        {
            if (v == null)
            {
                return null;
            }

            return ToQAngle(v.Value);
        }

        public static QAngle ToQAngle(Vector3 v)
        {
            return new QAngle(v.X, v.Y, v.Z);
        }

        public static Vector3 CalculateThirdPersonOffset(Vector3 viewAngle, float distance)
        {
            var headingRad = ToRad(viewAngle.Y);
            var pitchRad = ToRad(viewAngle.X);

            var xFactor = (float)Math.Cos(headingRad);
            var yFactor = (float)Math.Sin(headingRad);

            var horzFactor = (float)Math.Cos(pitchRad);
            var vertFactor = (float)Math.Sin(pitchRad);

            var forwardDir = new Vector3(xFactor, yFactor, 0);

            var dir = -forwardDir * horzFactor + Vector3.UnitZ * vertFactor;

            return dir * distance;
        }
    }
}
