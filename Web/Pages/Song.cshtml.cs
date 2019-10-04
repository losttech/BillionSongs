namespace BillionSongs.Pages {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using BillionSongs.Data;

    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [ResponseCache(VaryByHeader = "User-Agent", Duration = 30*24*60*60)]
    public class SongModel : PageModel
    {
        readonly ISongDatabase songDatabase;
        readonly IRandomSongProvider randomSongProvider;

        public Song Song { get; private set; }

        public async Task<IActionResult> OnGetAsync(uint id, string fallback = null, CancellationToken cancellation = default) {
            if (!this.ModelState.IsValid)
                return this.BadRequest();
            try {
                this.Song = await this.songDatabase.GetSong(id, cancellation).ConfigureAwait(false);
                if (this.Song.GeneratorError != null) {
                    switch (fallback) {
                    case "random":
                        uint randomID = await this.randomSongProvider.GetRandomSongID(cancellation).ConfigureAwait(false);
                        return this.RedirectToPage("Song", routeValues: new {
                            id = randomID, fallback = "random",
                        });
                    default:
                        return this.NotFound();
                    }
                }

                if (fallback != null)
                    return this.RedirectToPagePermanent("Song", routeValues: new { id = id });
                return new PageResult();
            }
            catch (LyricsGeneratorException) {
                switch (fallback) {
                case "random":
                    uint randomID = await this.randomSongProvider.GetRandomSongID(cancellation).ConfigureAwait(false);
                    return this.RedirectToPage("Song", routeValues: new {
                        id = randomID, fallback = "random",
                    });
                default:
                    return this.NotFound();
                }
            }
        }

        public SongModel([NotNull] ISongDatabase songDatabase,
                         [NotNull] IRandomSongProvider randomSongProvider) {
            this.songDatabase = songDatabase ?? throw new ArgumentNullException(nameof(songDatabase));
            this.randomSongProvider = randomSongProvider ?? throw new ArgumentNullException(nameof(randomSongProvider));
        }
    }
}
