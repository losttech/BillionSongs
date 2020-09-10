namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LostTech.Gradient;
    using ManyConsole.CommandLineUtils;

    public class GenerateTextCommand: ConsoleCommand {
        public string ModelName { get; private set; }
        public string Checkpoint { get; private set; }
        public int MaxLength { get; private set; } = 1024;
        public uint? Seed { get; set; }
        public string CondaEnv { get; set; }

        public override int Run(string[] remainingArguments) {
            Console.OutputEncoding = Encoding.UTF8;
            GradientLog.WarningWriter = GradientLog.OutputWriter = Console.Error;
            if (!string.IsNullOrEmpty(this.CondaEnv))
                GradientEngine.UseCondaEnvironment(this.CondaEnv);

            var generator = new Gpt2TextGenerator(
                modelName: this.ModelName,
                checkpoint: this.Checkpoint,
                sampleLength: this.MaxLength);
            uint seed = this.Seed ?? GetRandomSeed();
            string text = generator.GenerateSample(seed);
            while (text.StartsWith(generator.EndOfText))
                text = text.Substring(generator.EndOfText.Length);
            int end = text.IndexOf(generator.EndOfText, StringComparison.Ordinal);
            if (end < 0) {
                Console.Error.WriteLine("Text generated from this seed is longer than max-length.");
                Console.WriteLine(text);
                return -2;
            }

            Console.Write(text.Substring(0, end));

            return 0;
        }

        static uint GetRandomSeed() {
            var random = new Random();
            uint word1 = (uint)random.Next(ushort.MaxValue + 1);
            uint word2 = (uint)random.Next(ushort.MaxValue + 1);
            return word1 + (word2 << 16);
        }

        public GenerateTextCommand() {
            this.IsCommand("generate");
            this.HasRequiredOption("m|model=", "Name of the model to use",
                model => this.ModelName = model);
            this.HasRequiredOption("c|checkpoint=", "Specified which model checkpoint to load",
                pathOrName => this.Checkpoint = pathOrName);
            this.HasOption("l|max-length=", "Set the length limit on the generated text",
                (int length) => this.MaxLength = length);
            this.HasOption("s|seed=", "Set the seed for the generator to ensure reproducibility",
                (uint seed) => this.Seed = seed);
            this.HasOption("e|conda-env=", "Name of the conda environment to use", env => this.CondaEnv = env);
            this.SkipsCommandSummaryBeforeRunning();
        }
    }
}
