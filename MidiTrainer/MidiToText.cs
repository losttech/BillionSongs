namespace BillionSongs.MidiTrainer {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using ManyConsole.CommandLineUtils;
    class MidiToText : ConsoleCommand {
        public override int Run(string[] remainingArguments) {
            if (remainingArguments.Length == 0) {
                Console.Error.WriteLine("must specify files to convert");
                return -1;
            }
            foreach(string filePath in remainingArguments) {
                string target = Path.ChangeExtension(filePath, Path.GetExtension(filePath) + ".txt");
                this.Convert(filePath, target);
            }
            return 0;
        }

        void Convert(string filePath, string target) {
            Console.WriteLine($"{filePath} -> {target}");
        }

        public MidiToText() {
            this.IsCommand("2text", oneLineDescription: "converts MIDI file(s) to text, suitable for GPT-2-like model");
            this.AllowsAnyAdditionalArguments("MIDI files to convert to text");
        }
    }
}
