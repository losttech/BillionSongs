namespace BillionSongs {
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BillionSongs.Data;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class PregeneratedSongProvider: IRandomSongProvider {
    readonly ConcurrentQueue<PregeneratedSong> pregenerated = new ConcurrentQueue<PregeneratedSong>();
    readonly int desiredPoolSize = 50000;
    readonly int reuseLimit = 3;
    readonly TimeSpan emptyDelayInterval = TimeSpan.FromSeconds(1);
    readonly TimeSpan fullDelayInterval = TimeSpan.FromMinutes(1);
    readonly ISongDatabase songDatabase;
    readonly CancellationToken stopToken;
    readonly ILogger<PregeneratedSongProvider> logger;
    readonly DbSet<Song> prebuiltSongs;

    public async Task<uint> GetRandomSongID(CancellationToken cancellation) {
        while (true) {
            if (!this.pregenerated.TryDequeue(out PregeneratedSong song)) {
                this.logger.LogWarning("pregenerated song queue is dry");
                await Task.Delay(this.emptyDelayInterval, cancellation).ConfigureAwait(false);
            } else {
                int usesLeft = Interlocked.Decrement(ref song.usesLeft);
                if (usesLeft > 0) {
                    this.pregenerated.Enqueue(song);
                    return song.id;
                }
            }

            cancellation.ThrowIfCancellationRequested();
            this.stopToken.ThrowIfCancellationRequested();
        }
    }

    const int UseOldPercentage = 75;
    async void Generator(CancellationToken cancellation) {
        var fromDatabase = new List<PregeneratedSong>();
        await this.prebuiltSongs.AsNoTracking().Take(this.desiredPoolSize * UseOldPercentage / 100)
            .ForEachAsync(song => {
                if (song.GeneratorError == null)
                    fromDatabase.Add(new PregeneratedSong {
                        id = song.ID,
                        usesLeft = this.reuseLimit,
                    });
            }, cancellation).ConfigureAwait(false);

        Shuffle(fromDatabase);
        foreach (PregeneratedSong song in fromDatabase)
            this.pregenerated.Enqueue(song);

        this.logger.LogInformation($"loaded {this.pregenerated.Count} pregenerated songs");

        while (!cancellation.IsCancellationRequested) {
            if (this.pregenerated.Count >= this.desiredPoolSize) {
                this.logger.LogDebug("pregen queue full");
                await Task.Delay(this.fullDelayInterval, cancellation).ConfigureAwait(false);
                continue;
            }

            fromDatabase = fromDatabase.Select(song => new PregeneratedSong {
                id = song.id,
                usesLeft = this.reuseLimit,
            }).ToList();
            Shuffle(fromDatabase);
            foreach (PregeneratedSong song in fromDatabase)
                this.pregenerated.Enqueue(song);
        }
    }

    static void Shuffle<T>(IList<T> array, Random rng = null) {
        rng = rng ?? new Random();
        int n = array.Count;
        while (n > 1) 
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    public PregeneratedSongProvider([NotNull] ISongDatabase songDatabase,
        [NotNull] DbSet<Song> prebuiltSongs,
        [NotNull] ILogger<PregeneratedSongProvider> logger,
        CancellationToken stopToken) {
        this.songDatabase = songDatabase ?? throw new ArgumentNullException(nameof(songDatabase));
        this.stopToken = stopToken;
        this.prebuiltSongs = prebuiltSongs ?? throw new ArgumentNullException(nameof(prebuiltSongs));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.Generator(stopToken);
    }

    class PregeneratedSong {
        internal uint id;
        internal int usesLeft;
    }
}
}
