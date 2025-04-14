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
}