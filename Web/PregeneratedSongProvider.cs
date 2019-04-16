namespace BillionSongs {
using System;
using System.Collections.Concurrent;
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
    readonly int reuseLimit = 10;
    readonly TrulyRandomSongProvider randomProvider = new TrulyRandomSongProvider();
    readonly TimeSpan emptyDelayInterval = TimeSpan.FromSeconds(1);
    readonly TimeSpan fullDelayInterval = TimeSpan.FromMinutes(1);
    readonly ISongDatabase songDatabase;
    readonly CancellationToken stopToken;
    readonly ILogger<PregeneratedSongProvider> logger;
    readonly DbSet<Song> prebuiltSongs;

    public async Task<uint> GetRandomSongID(CancellationToken cancellation) {
        while (true) {
            if (!this.pregenerated.TryDequeue(out PregeneratedSong song)) {
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

    async void Generator(CancellationToken cancellation) {
        await this.prebuiltSongs.Take(this.desiredPoolSize)
            .ForEachAsync(song => {
                if (song.GeneratorError == null)
                    this.pregenerated.Enqueue(new PregeneratedSong {
                        id = song.ID,
                        usesLeft = this.reuseLimit,
                    });
            }, cancellation).ConfigureAwait(false);
        
        this.logger.LogInformation($"loaded {this.pregenerated.Count} pregenerated songs");
        
        while (!cancellation.IsCancellationRequested) {
            if (this.pregenerated.Count >= this.desiredPoolSize) {
                this.logger.LogDebug("pregen queue full");
                await Task.Delay(this.fullDelayInterval, cancellation).ConfigureAwait(false);
                continue;
            }

            uint id = this.randomProvider.GetRandomSongID();
            try {
                Song song = await this.songDatabase.GetSong(id, cancellation).ConfigureAwait(false);
                if (song.GeneratorError == null) {
                    this.pregenerated.Enqueue(new PregeneratedSong {
                        id = id,
                        usesLeft = this.reuseLimit,
                    });

                    int tenPercent = this.desiredPoolSize / 10;
                    if (this.pregenerated.Count % tenPercent == 0)
                        this.logger.LogDebug($"pregen queue: {this.pregenerated.Count * 100 / tenPercent}%");
                }
            }
            catch (LyricsGeneratorException) { }
            catch (OperationCanceledException) { }
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
