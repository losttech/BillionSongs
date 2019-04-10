namespace BillionSongs {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    public class DummyLyrics : ILyricsGenerator {
        public async Task<string> GenerateLyrics(uint song, CancellationToken cancellation) {
            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            return song.ToString();
        }
    }
}
