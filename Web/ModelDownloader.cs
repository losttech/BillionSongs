namespace BillionSongs {
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using LostTech.Gradient.Samples.GPT2;
    using JetBrains.Annotations;

    public static class ModelDownloader
    {
        const string CheckpointRootUri = "https://github.com/losttech/BillionSongs/releases/download/v0.1/117M-Lyrics-v0.1.zip";
        [NotNull]
        public static string DownloadCheckpoint(string root, string modelName, string runName) {
            string targetDirectory = Path.Combine(root, "checkpoint", runName);
            Directory.CreateDirectory(targetDirectory);

            using (var zipStream = new WebClient().OpenRead(CheckpointRootUri))
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Read))
                zip.ExtractToDirectory(targetDirectory);

            string checkpoint = Gpt2Checkpoints.GetLatestCheckpoint(root, modelName, runName);
            if (checkpoint == null)
                throw new IOException("Can't find checkpoint file after downloading");
            return checkpoint;
        }

        const string Gpt2ModelsRoot = "https://storage.googleapis.com/gpt-2/models";
        public static void DownloadModelParameters(string gpt2Root, string modelName) {
            string modelRoot = Path.Combine(gpt2Root, "models", modelName);
            string[] files = {
                "encoder.json",
                "hparams.json",
                "vocab.bpe",
            };

            Directory.CreateDirectory(modelRoot);
            var webClient = new WebClient();
            foreach (string file in files)
                webClient.DownloadFile(
                    address: $"{Gpt2ModelsRoot}/{modelName}/{file}",
                    fileName: Path.Combine(modelRoot, file));
        }
    }
}