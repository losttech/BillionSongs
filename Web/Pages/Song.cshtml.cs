namespace BillionSongs.Pages {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class SongModel : PageModel
    {
        readonly ILyricsGenerator lyricsGenerator;

        public string Lyrics { get; private set; }

        public async Task OnGetAsync(uint id, CancellationToken cancellation) {
            this.Lyrics = await this.lyricsGenerator.GenerateLyrics(id, cancellation).ConfigureAwait(false);
        }

        public SongModel([NotNull] ILyricsGenerator lyricsGenerator) {
            this.lyricsGenerator = lyricsGenerator ?? throw new ArgumentNullException(nameof(lyricsGenerator));
        }
    }
}
