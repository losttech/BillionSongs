namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BillionSongs.Data;

    public class TopSongWeightedProvider : IRandomSongProvider {
        readonly SongVoteCache voteCache;

        public Task<uint> GetRandomSongID(CancellationToken cancellation) {
            var weights = this.voteCache.AllSongs.ToDictionary(
                keySelector: summary => summary,
                elementSelector: summary => Math.Max(summary.VoteSum, 0));

            if (weights.Count == 0)
                return Task.FromResult(0u);

            cancellation.ThrowIfCancellationRequested();

            var weightedRandom = new WeightedRandom<SongSummary>(weights);

            cancellation.ThrowIfCancellationRequested();

            return Task.FromResult(weightedRandom.GetRandom(ThreadSafeRandom.Instance).Song.ID);
        }

        public TopSongWeightedProvider(SongVoteCache voteCache) {
            this.voteCache = voteCache ?? throw new ArgumentNullException(nameof(voteCache));
        }
    }
}
