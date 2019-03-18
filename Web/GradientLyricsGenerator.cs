namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    class GradientLyricsGenerator : ILyricsGenerator {
        readonly GradientTextGenerator textGenerator;

        public Task<string> GenerateLyrics(uint song)
            => Task.Run(() => this.textGenerator.GenerateSample(song));

        public GradientLyricsGenerator(GradientTextGenerator textGenerator) {
            this.textGenerator = textGenerator;
        }
    }
}
