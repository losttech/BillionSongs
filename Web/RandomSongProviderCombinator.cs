namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    public class RandomSongProviderCombinator : IRandomSongProvider {
        readonly WeightedRandom<IRandomSongProvider> providers;
        readonly ILogger<RandomSongProviderCombinator> logger;

        public Task<uint> GetRandomSongID(CancellationToken cancellation) {
            IRandomSongProvider provider = this.providers.GetRandom(ThreadSafeRandom.Instance);
            this.logger?.LogDebug($"random song from {provider}");
            return provider.GetRandomSongID(cancellation);
        }

        public RandomSongProviderCombinator(WeightedRandom<IRandomSongProvider> providers,
        ILogger<RandomSongProviderCombinator> logger) {
            this.providers = providers;
            this.logger = logger;
        }
    }
}
