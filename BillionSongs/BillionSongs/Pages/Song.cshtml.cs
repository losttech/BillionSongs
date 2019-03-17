namespace BillionSongs.Pages {
    using System;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class SongModel : PageModel
    {
        readonly ILyricsGenerator lyricsGenerator;

        public string Lyrics { get; private set; }

        public async Task OnGetAsync(int id) {
            this.Lyrics = await this.lyricsGenerator.GenerateLyrics(id);
        }

        public SongModel([NotNull] ILyricsGenerator lyricsGenerator) {
            this.lyricsGenerator = lyricsGenerator ?? throw new ArgumentNullException(nameof(lyricsGenerator));
        }
    }
}
