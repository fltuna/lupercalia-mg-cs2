using CounterStrikeSharp.API.Modules.Utils;

namespace LupercaliaMGCore;

public static class VectorExtension
{
    public static bool WithinAABox(this Vector target, Vector min, Vector max)
    {
        bool withinX = target.X >= Math.Min(min.X, max.X) && 
                       target.X <= Math.Max(min.X, max.X);
        
        bool withinY = target.Y >= Math.Min(min.Y, max.Y) && 
                       target.Y <= Math.Max(min.Y, max.Y);
        
        bool withinZ = target.Z >= Math.Min(min.Z, max.Z) && 
                       target.Z <= Math.Max(min.Z, max.Z);

        return withinX && withinY && withinZ;
    }
    
    public static double Distance3D(this Vector vec1, Vector vec2)
    {
        double deltaX = vec1.X - vec2.X;
        double deltaY = vec1.Y - vec2.Y;
        double deltaZ = vec1.Z - vec2.Z;

        double distanceSquared = Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2);
        return Math.Sqrt(distanceSquared);
    }
}