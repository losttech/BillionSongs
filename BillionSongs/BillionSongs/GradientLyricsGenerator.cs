namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    class GradientLyricsGenerator : ILyricsGenerator {
        public Task<string> GenerateLyrics(int song) => Task.FromResult(song.ToString());
    }
}
