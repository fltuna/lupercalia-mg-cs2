namespace LupercaliaMGCore.modules.ExternalView.Utils
{
    public record ObserverTarget(
        int Slot,
        string Name
    );

    public class ObserverTargetUtils
    {
        public static Random _Rand = new Random();

        public static ObserverTarget? Find(IEnumerable<ObserverTarget> candidates, string target)
        {
            var count = candidates.Count();
            if (count == 0)
                return null;

            target = target.ToLower();

            candidates = candidates
                .Where(candidate => candidate.Name.ToLower().IndexOf(target) >= 0)
                .OrderBy(candidate => candidate.Name.ToLower().IndexOf(target));

            if (candidates.Count() == 0)
                return null;

            return candidates.First();
        }

        public static ObserverTarget? Next(IEnumerable<ObserverTarget> candidates, int slot)
        {
            var list = candidates.ToList();
            if (list.Count == 0)
                return null;

            var idx = list.FindIndex(candidate => candidate.Slot == slot);
            if (idx < 0)
                return null;

            if (idx == list.Count - 1)
                return list[0];
            return list[idx + 1];
        }

        public static ObserverTarget? Prev(IEnumerable<ObserverTarget> candidates, int slot)
        {
            var list = candidates.ToList();
            if (list.Count == 0)
                return null;

            var idx = list.FindIndex(candidate => candidate.Slot == slot);
            if (idx < 0)
                return null;

            if (idx == 0)
                return list[list.Count - 1];
            return list[idx - 1];
        }

        public static ObserverTarget? SelectRandom(IEnumerable<ObserverTarget> candidates)
        {
            var count = candidates.Count();
            if (count == 0)
                return null;

            var idx = _Rand.Next(count);
            return candidates.ElementAt(idx);
        }
    }
}
