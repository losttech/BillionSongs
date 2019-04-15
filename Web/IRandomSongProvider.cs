namespace BillionSongs {
using System.Threading;
using System.Threading.Tasks;

public interface IRandomSongProvider {
    Task<uint> GetRandomSongID(CancellationToken cancellation);
}
}
