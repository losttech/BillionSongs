namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    public struct WeightedRandom<T> {
        readonly List<WeightedEntry<T>> providers;
        int Sum => this.providers[this.providers.Count - 1].Limit;

        public T GetRandom(Random random) {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            int point = random.Next(this.Sum);
            int index = this.providers.BinarySearch(new WeightedEntry<T> { Limit = point + 1 });
            if (index >= 0)
                return this.providers[index].Value;

            return this.providers[~index].Value;
        }

        public WeightedRandom(IReadOnlyDictionary<T, int> weights) {
            if (weights == null)
                throw new ArgumentNullException(nameof(weights));

            int sum = 0;

            var providers = new List<WeightedEntry<T>>();
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

                providers.Add(new WeightedEntry<T> {
                    Limit = sum,
                    Value = entry.Key
                });
            }

            if (providers.Count == 0)
                throw new ArgumentException(message: "At least one non-zero weight must be provided");

            this.providers = providers;
        }
    }

    struct WeightedEntry<T> : IComparable<WeightedEntry<T>> {
        public int Limit;
        public T Value;

        public int CompareTo(WeightedEntry<T> other) => this.Limit.CompareTo(other.Limit);
    }
}
