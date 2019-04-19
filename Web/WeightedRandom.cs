namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    public class WeightedRandom<T> {
        readonly (int, T)[] providers;
        int Sum => this.providers[this.providers.Length - 1].Item1;

        public T GetRandom(Random random) {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            int point = random.Next(this.Sum);
            int index = Array.BinarySearch(this.providers, (point + 1, default(T)));
            if (index >= 0)
                return this.providers[index].Item2;

            return this.providers[~index].Item2;
        }

        public WeightedRandom(IReadOnlyDictionary<T, int> weights) {
            if (weights == null)
                throw new ArgumentNullException(nameof(weights));

            int sum = 0;

            var providers = new List<(int, T)>();
            foreach (var entry in weights) {
                if (entry.Value < 0)
                    throw new ArgumentOutOfRangeException(nameof(weights), message: "All weights must be >=0");

                if (entry.Value == 0)
                    continue;

                try {
                    sum = checked(sum + entry.Value);
                } catch (OverflowException overflow) {
                    throw new NotSupportedException("Sum of weigths is too large", overflow);
                }

                providers.Add((this.Sum, entry.Key));
            }

            if (providers.Count == 0)
                throw new ArgumentException(message: "At least one non-zero weight must be provided");

            this.providers = providers.ToArray();
        }
    }
}
