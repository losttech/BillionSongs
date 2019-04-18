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
    using Microsoft.EntityFrameworkCore;

    [ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
    public class TopSongsModel : PageModel {
        readonly ApplicationDbContext db;

        public List<SongSummary> Top { get; } = new List<SongSummary>();

        public async Task OnGetAsync(CancellationToken cancellation = default) {
            var summaries = await this.db.Songs.Select(song => new {
                Song = song,
                Upvotes = song.Votes.Count(vote => vote.Upvote),
                Downvotes = song.Votes.Count(vote => !vote.Upvote),
            }).OrderByDescending(summary => summary.Upvotes - summary.Downvotes)
              .Take(10).ToArrayAsync(cancellation).ConfigureAwait(false);

            foreach (var summary in summaries)
                this.Top.Add(new SongSummary {
                    Song = summary.Song,
                    Upvotes = summary.Upvotes,
                    Downvotes = summary.Downvotes,
                });
        }

        public TopSongsModel([NotNull] ApplicationDbContext db) {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
        }
    }
}