namespace BillionSongs.Pages {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using BillionSongs.Data;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;

    [ResponseCache(VaryByHeader = "User-Agent", Duration = 60, Location = ResponseCacheLocation.Client)]
    [Authorize]
    public class MyVotesModel : PageModel {
        readonly ApplicationDbContext db;

        public List<SongSummary> Songs = new List<SongSummary>();

        public async Task OnGetAsync(CancellationToken cancellation = default) {
            string userID = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var votes = await this.db.Votes.Where(vote => vote.UserID == userID)
                .Select(vote => new { vote.Song, vote.Upvote })
                .ToArrayAsync(cancellation).ConfigureAwait(false);

            foreach (var vote in votes)
                this.Songs.Add(new SongSummary {
                    Song = vote.Song,
                    Upvotes = vote.Upvote ? 1 : 0,
                    Downvotes = vote.Upvote ? 0 : 1,
                });
        }

        public MyVotesModel([NotNull] ApplicationDbContext db) {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
        }
    }
}
