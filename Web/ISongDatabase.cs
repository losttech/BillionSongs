namespace BillionSongs {
    using System.Threading;
    using System.Threading.Tasks;
    using BillionSongs.Data;

    public interface ISongDatabase {
        Task<Song> GetSong(uint song, CancellationToken cancellation);
    }
}
