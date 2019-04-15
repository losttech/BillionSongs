namespace BillionSongs.Pages {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using JetBrains.Annotations;

public class IndexModel : PageModel {
    readonly IRandomSongProvider randomSongProvider;
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellation) {
        uint songID = await this.randomSongProvider.GetRandomSongID(cancellation).ConfigureAwait(false);
        return this.RedirectToPage("Song", routeValues: new {id = songID, fallback = "random"});
    }

    public IndexModel([NotNull] IRandomSongProvider randomSongProvider) {
        this.randomSongProvider = randomSongProvider ?? throw new ArgumentNullException(nameof(randomSongProvider));
    }
}
}
