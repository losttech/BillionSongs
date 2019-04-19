namespace BillionSongs {
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using BillionSongs.Data;
    using Microsoft.EntityFrameworkCore;

    public class SongVoteCache {
        readonly ConcurrentDictionary<uint, SongSummary> voteCache = new ConcurrentDictionary<uint, SongSummary>();

        public void AddUpvotes(uint songID, int votes) {
            this.voteCache.AddOrUpdate(songID, new SongSummary{ Upvotes = votes, Song = new Song { ID = songID } },
                updateValueFactory: (_, oldSummary) => {
                    var newSummary = oldSummary.Clone();
                    newSummary.Upvotes += votes;
                    return newSummary;
                });
        }

        public void AddDownvotes(uint songID, int votes) {
            this.voteCache.AddOrUpdate(songID, new SongSummary{ Downvotes = votes, Song = new Song { ID = songID } },
                updateValueFactory: (_, oldSummary) => {
                    var newSummary = oldSummary.Clone();
                    newSummary.Downvotes += votes;
                    return newSummary;
                });
        }

        public IEnumerable<SongSummary> AllSongs => this.voteCache.Select(v => v.Value);

        public static SongVoteCache Load(DbSet<SongVote> votes) {
            var result = new SongVoteCache();
            foreach(var vote in votes.AsNoTracking()) {
                if (vote.Upvote)
                    result.AddUpvotes(vote.SongID, 1);
                else
                    result.AddDownvotes(vote.SongID, 1);
            }
            return result;
        }
    }
}
