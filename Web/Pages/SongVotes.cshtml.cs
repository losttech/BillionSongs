namespace BillionSongs.Pages {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using BillionSongs.Data;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;

    public class SongVotesModel : PageModel {
        public uint ID{ get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public bool? OwnVote { get; set; }

        readonly ApplicationDbContext db;
        readonly SongVoteCache voteCache;

        public async Task OnGetAsync(uint id, CancellationToken cancellation = default) {
            this.ID = id;
            string userID = this.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            this.OwnVote = (await this.db.Votes.FindAsync(id, userID).ConfigureAwait(false))?.Upvote;
            this.Upvotes = await this.db.Votes.CountAsync(vote => vote.SongID == id && vote.Upvote, cancellation).ConfigureAwait(false);
            this.Downvotes = await this.db.Votes.CountAsync(vote => vote.SongID == id && !vote.Upvote, cancellation).ConfigureAwait(false);
        }

        public SongVotesModel(ApplicationDbContext db, SongVoteCache voteCache) {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.voteCache = voteCache ?? throw new ArgumentNullException(nameof(voteCache));
        }

        public Task<IActionResult> OnPostUpvote(uint id, CancellationToken cancellation = default)
            => this.OnPostSetVote(id, upvote: true, cancellation);
        public Task<IActionResult> OnPostDownvote(uint id, CancellationToken cancellation = default)
            => this.OnPostSetVote(id, upvote: false, cancellation);
        async Task<IActionResult> OnPostSetVote(uint id, bool upvote, CancellationToken cancellation){
            if (!this.ModelState.IsValid)
                return this.BadRequest();

            if (this.User?.Identity.IsAuthenticated != true)
                return this.RedirectToPage("/Account/Login", new { area = "Identity",
                    ReturnUrl = this.Url.Page("Song", new { id = id }) });

            return await this.SetVote(id, upvote, cancellation).ConfigureAwait(false);
        }
        async Task<IActionResult> SetVote(uint id, bool upvote, CancellationToken cancellation) {
            string userID = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existing = await this.db.Votes.FindAsync(id, userID).ConfigureAwait(false);

            int deltaUpvote = 0;
            int deltaDownvote = 0;

            if (existing == null) {
                this.db.Votes.Add(new SongVote {
                    SongID = id,
                    UserID = this.User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Upvote = upvote,
                });
                if (upvote)
                    deltaUpvote++;
                else
                    deltaDownvote++;
            } else {
                if (existing.Upvote == upvote)
                    return this.RedirectToPage("Song", routeValues: new { id = id });

                int delta = upvote ? 1 : -1;
                deltaUpvote += delta;
                deltaDownvote -= delta;

                existing.Upvote = upvote;
                this.db.Votes.Update(existing);
            }

            try {
                await this.db.SaveChangesAsync(cancellation).ConfigureAwait(false);
                if (deltaUpvote != 0)
                    this.voteCache.AddUpvotes(id, deltaUpvote);
                if (deltaDownvote != 0)
                    this.voteCache.AddDownvotes(id, deltaDownvote);
            } catch(DbUpdateConcurrencyException) { }

            return this.RedirectToPage("Song", routeValues: new { id = id });
        }
    }
}