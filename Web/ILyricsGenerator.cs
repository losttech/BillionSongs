namespace BillionSongs {
    using System.Threading.Tasks;

    public interface ILyricsGenerator {
        Task<string> GenerateLyrics(uint song);
    }
}