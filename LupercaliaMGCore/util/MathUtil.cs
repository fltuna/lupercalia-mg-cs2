using CounterStrikeSharp.API.Modules.Utils;

namespace LupercaliaMGCore;

public static class MathUtil
{
    public static List<int> DecomposePowersOfTwo(int number)
    {
        List<int> powers = new List<int>();

        for (int i = 0; i < 32; i++)
        {
            int bitValue = 1 << i;
            if ((number & bitValue) != 0)
            {
                powers.Add(bitValue);
            }
        }

        return powers;
    }
}