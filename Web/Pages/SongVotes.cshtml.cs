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

        public async Task OnGetAsync(uint id, CancellationToken cancellation = default) {
            this.ID = id;
            string userID = this.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            this.OwnVote = (await this.db.Votes.FindAsync(id, userID).ConfigureAwait(false))?.Upvote;
            this.Upvotes = await this.db.Votes.CountAsync(vote => vote.SongID == id && vote.Upvote, cancellation).ConfigureAwait(false);
            this.Downvotes = await this.db.Votes.CountAsync(vote => vote.SongID == id && !vote.Upvote, cancellation).ConfigureAwait(false);
        }

        public SongVotesModel(ApplicationDbContext db) {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
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
            if (existing == null) {
                this.db.Votes.Add(new SongVote {
                    SongID = id,
                    UserID = this.User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Upvote = upvote,
                });
            } else {
                existing.Upvote = upvote;
                this.db.Votes.Update(existing);
            }

            try {
                await this.db.SaveChangesAsync(cancellation).ConfigureAwait(false);
            } catch(DbUpdateConcurrencyException) { }

            return this.RedirectToPage("Song", routeValues: new { id = id });
        }
    }
}