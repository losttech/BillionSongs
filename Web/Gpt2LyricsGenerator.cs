namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;

    class Gpt2LyricsGenerator : ILyricsGenerator {
        const int MaxLength = 1024; // 2048 is more, than 80% of the songs are shorter
        readonly ILogger<Gpt2LyricsGenerator> logger;
        readonly string condaEnv;
        readonly string gpt2Root;
        readonly string modelName;
        readonly string checkpoint;
        static readonly string ExecutablePath = Assembly.GetExecutingAssembly().Location;

        public Task<string> GenerateLyrics(uint song, CancellationToken cancellation) {
            var startInfo = new ProcessStartInfo {
                Arguments = "", // using ArgumentList requires this set to ""
                WorkingDirectory = this.gpt2Root,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            if (Path.GetExtension(ExecutablePath)
                    .Equals(".dll", StringComparison.OrdinalIgnoreCase)) {
                startInfo.FileName = "dotnet";
                startInfo.ArgumentList.Add(ExecutablePath);
            } else
                startInfo.FileName = ExecutablePath;

            var args = startInfo.ArgumentList;
            args.Add("generate");

            args.Add("--model");
            args.Add(this.modelName);

            args.Add("--checkpoint");
            args.Add(this.checkpoint);

            args.Add("--max-length");
            args.Add(MaxLength.ToString(CultureInfo.InvariantCulture));

            args.Add("--seed");
            args.Add(song.ToString(CultureInfo.InvariantCulture));

            if (!string.IsNullOrEmpty(this.condaEnv)) {
                args.Add("--conda-env");
                args.Add(this.condaEnv);
            }

            var error = new StringBuilder(16*1024);
            var text = new StringBuilder(1024);

            return Task.Run(() => {
                using (var generator = new Process()) {
                    generator.StartInfo = startInfo;
                    generator.ErrorDataReceived += (_, eventArgs) => error.AppendLine(eventArgs.Data);
                    generator.OutputDataReceived += (_, eventArgs) => text.AppendLine(eventArgs.Data);

                    try {
                        if (!generator.Start())
                            throw new LyricsGeneratorException("Unable to start generator process");
                    } catch (System.ComponentModel.Win32Exception e) {
                        throw new LyricsGeneratorException("Unable to start generator process", e);
                    } catch (PlatformNotSupportedException e) {
                        throw new LyricsGeneratorException("Unable to start generator process", e);
                    }

                    try {
                        generator.BeginErrorReadLine();
                        generator.BeginOutputReadLine();

                        while (!generator.WaitForExit(milliseconds: 100)) {
                            if (cancellation.IsCancellationRequested) {
                                try {
                                    generator.Kill();
                                } catch (InvalidOperationException) {
                                } catch (Win32Exception cantTerminate) {
                                    this.logger.LogError(cantTerminate, "Could not terminate generator on cancellation");
                                }
                                cancellation.ThrowIfCancellationRequested();
                            }
                        }

                        generator.WaitForExit();

                        switch (generator.ExitCode) {
                        case 0:
                            return text.ToString();
                        case -2:
                            throw new LyricsGeneratorException("Text generated from this seed is longer than max-length");
                        default:
                            throw new LyricsGeneratorException(error.ToString());
                        }
                    } finally {
                        if (!generator.HasExited) {
                            try {
                                generator.Kill();
                            } catch (InvalidOperationException) {
                            } catch (Win32Exception cantTerminate) {
                                this.logger.LogError(cantTerminate, "Could not terminate generator after timeout");
                            }
                        }
                    }
                }
            }, cancellation);
        }

        public Gpt2LyricsGenerator(
            [NotNull] string gpt2Root,
            [NotNull] string modelName,
            [NotNull] string checkpoint,
            [NotNull] ILogger<Gpt2LyricsGenerator> logger,
            [CanBeNull] string condaEnv) {
            this.gpt2Root = gpt2Root ?? throw new ArgumentNullException(nameof(gpt2Root));
            this.modelName = modelName ?? throw new ArgumentNullException(nameof(modelName));
            this.checkpoint = checkpoint ?? throw new ArgumentNullException(nameof(checkpoint));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.condaEnv = condaEnv;
        }
    }
}
