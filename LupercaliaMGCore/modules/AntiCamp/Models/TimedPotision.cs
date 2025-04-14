using CounterStrikeSharp.API.Modules.Utils;

namespace LupercaliaMGCore.modules.AntiCamp.Models
{
    public class TimedPosition(double time, Vector vector)
    {
        public readonly double Time = time;
        public readonly Vector Vector = vector;
    }
}