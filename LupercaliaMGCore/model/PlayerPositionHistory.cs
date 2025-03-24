using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace LupercaliaMGCore {
    public class PlayerPositionHistory
    {
        public readonly FixedSizeQueue<TimedPosition> positions;

        public PlayerPositionHistory(int maxHistories) {
            positions = new FixedSizeQueue<TimedPosition>(maxHistories);
        }

        public void Update(Vector currentPosition) {
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

        public TimedPosition GetOldestPosition() {
            return positions.Peek();
        }

        public override string ToString()
        {
            return string.Join(", ", positions);
        }

        private static TimedPosition GetNearestTimedPosition(double secondsAgo, FixedSizeQueue<TimedPosition> timedPositionsQueue)
        {
            List<TimedPosition> timedPositions = timedPositionsQueue.ToArray().ToList();
            double nearestValue = timedPositions[0].time;
            double minDifference = Math.Abs(secondsAgo - nearestValue);

            double currentDifference;
            int foundIndex = 0;
            for (int i = 1; i < timedPositions.Count; i++)
            {
                currentDifference = Math.Abs(secondsAgo - timedPositions[i].time);
                if (currentDifference < minDifference)
                {
                    minDifference = currentDifference;
                    nearestValue = timedPositions[i].time;
                    foundIndex = i;
                }
                else if (currentDifference == minDifference)
                {
                    nearestValue = Math.Min(nearestValue, timedPositions[i].time);
                    foundIndex = i;
                }
            }
            
            return timedPositions[foundIndex];
        }
    }
}