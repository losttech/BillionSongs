namespace BillionSongs {
using System;
using System.Threading;
using System.Threading.Tasks;

public class TrulyRandomSongProvider: IRandomSongProvider {
    readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random());
    public Task<uint> GetRandomSongID(CancellationToken cancellation) {
        uint id = unchecked((uint)this.random.Value.Next());
        id += unchecked((uint)this.random.Value.Next());
        return Task.FromResult(id);
    }
}
}
