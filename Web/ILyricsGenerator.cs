namespace BillionSongs {
    using System.Threading;
    using System.Threading.Tasks;

    public interface ILyricsGenerator {
        Task<string> GenerateLyrics(uint song, CancellationToken cancellation);
    }
}