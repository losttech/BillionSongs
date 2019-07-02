namespace BillionSongs.MidiTrainer {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Gradient;
    using Gradient.Samples.GPT2;
    using LostTech.WhichPython;
    using ManyConsole.CommandLineUtils;
    using numpy;

    class TrainCommand : ConsoleCommand {
        public override int Run(string[] remainingArguments) {
            if (this.CondaEnvironment != null){
                var env = PythonEnvironment.EnumerateCondaEnvironments()
                    .SingleOrDefault(e => Path.GetFileName(e.Home) == this.CondaEnvironment);
                if (env == null){
                    Console.Error.WriteLine($"conda environment '{this.CondaEnvironment}' was not found");
                    return (int)ErrorCodes.CondaEnvironmentNotFound;
                }
                GradientSetup.UsePythonEnvironment(env);
            }

            // force Gradient initialization
            tensorflow.tf.no_op();

            string checkpoint = Gpt2Checkpoints.ProcessCheckpointConfig(
                gpt2Root: Environment.CurrentDirectory,
                checkpoint: this.Checkpoint,
                modelName: this.ModelName,
                runName: this.RunName);

            var encoder = Gpt2Encoder.LoadEncoder(this.ModelName);

            var hParams = Gpt2Model.LoadHParams(this.ModelName);
            var random = this.Seed == null ? new Random() : new Random(this.Seed.Value);
            var stop = new CancellationTokenSource();
            Console.CancelKeyPress += delegate { stop.Cancel(); };
            var dataset = new List<ndarray>();
            new Gpt2Trainer(dataset, encoder, hParams, this.BatchSize, sampleLength: 1024, random)
                .Train(checkpoint, this.RunName, counter: null, stop.Token);

            return 0;
        }

        static int Main(string[] args) {
            GradientSetup.OptInToUsageDataCollection();

            return ConsoleCommandDispatcher.DispatchCommand(
                ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(TrainCommand)),
                args, Console.Out);
        }

        public string ModelName { get; set; } = "117M";
        public int? Seed { get; set; }
        public int BatchSize { get; set; } = 1;
        public int SampleLength { get; set; } = 1024;
        public int SampleNum { get; set; } = 1;
        public int SampleEvery { get; set; } = 100;
        public int SaveEvery { get; set; } = 1000;
        public string RunName { get; set; } = DateTime.Now.ToString("s").Replace(':', '-');
        public string Checkpoint { get; set; } = "latest";
        public string Include { get; set; }
        public string CondaEnvironment { get; set; }

        public TrainCommand() {
            this.IsCommand("train");
            this.HasAdditionalArguments(1, "<dataset>");
            this.HasOption("m|model=", "Which model to use", name => this.ModelName = name);
            this.HasOption("s|seed=",
                "Explicitly set seed for random generators to get reproducible results",
                (int s) => this.Seed = s);
            this.HasOption("i|include=", "Pattern of files to include in training",
                pattern => this.Include = pattern);
            this.HasOption("n|sample-num=", "",
                (int count) => this.SampleNum = count);
            this.HasOption("b|batch-size=", "Size of the batch, must divide sample-count",
                (int size) => this.BatchSize = size);
            this.HasOption("l|sample-length=", "",
                (int len) => this.SampleLength = len);
            this.HasOption("sample-every=", "Print a sample every N epochs",
                (int n) => this.SampleEvery = n);
            this.HasOption("save-every=", "How often to save a model, in epochs",
                (int n) => this.SaveEvery = n);
            this.HasOption("r|run=", "Name of the run (to be able to resume)",
                run => this.RunName = run);
            this.HasOption("c|checkpoint=", "Use specific checkpoint to start. Available values: 'latest' (default), 'fresh', or path to a checkpoint file",
                checkpoint => this.Checkpoint = checkpoint);
            this.HasOption("e|conda-env=", "Use specific conda environment",
                env => this.CondaEnvironment = env);
        }

        enum ErrorCodes {
            CondaEnvironmentNotFound = 1,
        }
    }
}
