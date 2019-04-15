namespace BillionSongs.Pages {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using BillionSongs.Data;

    using JetBrains.Annotations;

    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class SongModel : PageModel
    {
        readonly ISongDatabase songDatabase;

        public Song Song { get; private set; }

        public async Task OnGetAsync(uint id, CancellationToken cancellation) {
            this.Song = await this.songDatabase.GetSong(id, cancellation).ConfigureAwait(false);
        }

        public SongModel([NotNull] ISongDatabase songDatabase) {
            this.songDatabase = songDatabase ?? throw new ArgumentNullException(nameof(songDatabase));
        }
    }
}
