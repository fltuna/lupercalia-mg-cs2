using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.util;

namespace LupercaliaMGCore.modules.AntiCamp.Models;

public class PlayerPositionHistory(int maxHistories)
{
    private readonly FixedSizeQueue<TimedPosition> positions = new(maxHistories);

    public void Update(Vector currentPosition)
    {
        double currentTime = Server.EngineTime;
        positions.Enqueue(new TimedPosition(currentTime, currentPosition));
    }

    public TimedPosition? GetPositionAt(double secondsAgo)
    {
        if (secondsAgo < 0 || positions.Count == 0)
        {
            return null;
        }

        return GetNearestTimedPosition(secondsAgo, positions);
    }

    public TimedPosition GetOldestPosition()
    {
        return positions.Peek();
    }

    public override string ToString()
    {
        return string.Join(", ", positions);
    }

    private static TimedPosition GetNearestTimedPosition(double secondsAgo,
        FixedSizeQueue<TimedPosition> timedPositionsQueue)
    {
        List<TimedPosition> timedPositions = timedPositionsQueue.ToArray().ToList();
        double nearestValue = timedPositions[0].Time;
        double minDifference = Math.Abs(secondsAgo - nearestValue);

        double currentDifference;
        int foundIndex = 0;
        for (int i = 1; i < timedPositions.Count; i++)
        {
            currentDifference = Math.Abs(secondsAgo - timedPositions[i].Time);
            if (currentDifference < minDifference)
            {
                minDifference = currentDifference;
                nearestValue = timedPositions[i].Time;
                foundIndex = i;
            }
            else if (currentDifference == minDifference)
            {
                nearestValue = Math.Min(nearestValue, timedPositions[i].Time);
                foundIndex = i;
            }
        }

        return timedPositions[foundIndex];
    }
}