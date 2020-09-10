namespace BillionSongs.MidiTrainer {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Text;
    using ManyConsole.CommandLineUtils;
    using MidiSharp;
    using MidiSharp.Events.Meta.Text;

    class MidiToText : ConsoleCommand {
        public override int Run(string[] remainingArguments) {
            if (remainingArguments.Length == 0) {
                Console.Error.WriteLine("must specify files to convert");
                return -1;
            }
            var files = remainingArguments.SelectMany(EnumerateFilesFromPathSpec);
            foreach(string filePath in files) {
                string target = Path.ChangeExtension(filePath, Path.GetExtension(filePath) + ".txt");
                this.Convert(filePath, target);
            }
            return 0;
        }

        static readonly char[] fileMaskCharacters = new[] { '*', '?' };
        static IEnumerable<string> EnumerateFilesFromPathSpec(string pathSpec) {
            if (pathSpec.IndexOfAny(fileMaskCharacters) == 0) {
                yield return pathSpec;
                yield break;
            }

            string nameMask = Path.GetFileName(pathSpec);
            string directory = Path.GetDirectoryName(pathSpec) ?? ".";
            if (directory.IndexOfAny(fileMaskCharacters) >= 0)
                throw new ArgumentException("The following characters can only be present in file name spec: "
                    + string.Join("", fileMaskCharacters));
            foreach (string entry in Directory.EnumerateFiles(directory, searchPattern: nameMask))
                yield return entry;
        }

        void Convert(string filePath, string target) {
            Console.WriteLine($"{filePath} -> {target}");
            try {
                using (var input = File.OpenRead(filePath)) {
                    var midi = MidiSequence.Open(input);
                    string[] lyrics = midi.SelectMany(track => track.Events)
                        .OfType<BaseTextMetaMidiEvent>()
                        .GroupBy(evt => evt.GetType())
                        .Select(e => e.Key.Name + "\n" + string.Join(" ",
                            e.Select(evt => $"{evt.Text.Trim(' ', '\t')}@{evt.DeltaTime}")))
                        .ToArray();

                    if (lyrics.Length == 0)
                        Console.WriteLine("(no lyrics found)");
                    else
                        foreach (var kind in lyrics)
                            Console.WriteLine(kind);

                    Console.WriteLine();
                    var list = midi.Tracks.SelectMany(track => track.Events).Select(evt => evt.GetType()).Distinct();
                    foreach (var entry in list) {
                        Console.WriteLine(entry.ToString());
                    }
                }
            } catch (MidiParser.Mid) {

            }
        }

        public MidiToText() {
            this.IsCommand("2text", oneLineDescription: "converts MIDI file(s) to text, suitable for GPT-2-like model");
            this.AllowsAnyAdditionalArguments("MIDI files to convert to text");
        }
    }
}
