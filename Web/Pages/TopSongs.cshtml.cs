namespace BillionSongs.Pages {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BillionSongs.Data;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [ResponseCache(VaryByHeader = "User-Agent", Duration = 5*60)]
    public class TopSongsModel : PageModel {
        readonly ISongDatabase db;
        readonly SongVoteCache voteCache;

        public List<SongSummary> Top { get; } = new List<SongSummary>();

        public async Task OnGetAsync(CancellationToken cancellation = default) {
            var summaries = this.voteCache.AllSongs
                .OrderByDescending(summary => summary.VoteSum)
                .Take(10)
                .ToArray();

            foreach (var summary in summaries)
                this.Top.Add(new SongSummary {
                    Song = await this.db.GetSong(summary.Song.ID, cancellation).ConfigureAwait(false),
                    Upvotes = summary.Upvotes,
                    Downvotes = summary.Downvotes,
                });
        }

        public TopSongsModel([NotNull] ISongDatabase db, [NotNull] SongVoteCache voteCache) {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.voteCache = voteCache ?? throw new ArgumentNullException(nameof(voteCache));
        }
    }
}