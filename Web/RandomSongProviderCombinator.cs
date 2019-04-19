namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    public class RandomSongProviderCombinator : IRandomSongProvider {
        readonly WeightedRandom<IRandomSongProvider> providers;

        public Task<uint> GetRandomSongID(CancellationToken cancellation)
            => this.providers.GetRandom(ThreadSafeRandom.Instance).GetRandomSongID(cancellation);

        public RandomSongProviderCombinator(WeightedRandom<IRandomSongProvider> providers) {
            this.providers = providers ?? throw new ArgumentNullException(nameof(providers));
        }
    }
}
