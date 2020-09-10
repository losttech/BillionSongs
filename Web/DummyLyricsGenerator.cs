namespace BillionSongs {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    public class DummyLyrics : ILyricsGenerator {
        public async Task<string> GenerateLyrics(uint song, CancellationToken cancellation) {
            if ((song & 1) == 0) throw new LyricsGeneratorException("Dummy only generates odd songs");
            await Task.Delay(TimeSpan.FromSeconds(5), cancellation).ConfigureAwait(false);
            return song.ToString() + " by DummyLyrics.\n\nRemove 'Generator': 'dummy' from settings to switch to the real one.";
        }
    }
}
