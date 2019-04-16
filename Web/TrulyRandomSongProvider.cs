namespace BillionSongs {
using System;
using System.Threading;
using System.Threading.Tasks;

public class TrulyRandomSongProvider: IRandomSongProvider {
    readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random());
    public Task<uint> GetRandomSongID(CancellationToken cancellation) {
        uint id = this.GetRandomSongID();
        return Task.FromResult(id);
    }

    public uint GetRandomSongID() {
        uint id = unchecked((uint)this.random.Value.Next());
        id += unchecked((uint)this.random.Value.Next());
        return id;
    }
}
}
