using FastRsync.Core;
using FastRsync.Delta;
using FastRsync.Signature;
using RsyncExample.Classes;
using System.CommandLine;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace RsyncExample.Commands
{
    internal class ZipCommand : Command
    {
        private static string SignatureFolder = string.Empty;
        private static string RemoteFolder = string.Empty;
        private static string DeltaFolder = string.Empty;
        private static string TempFolder = string.Empty;

        private Argument<string> sourceArg = new("source", "The source path.");
        private Argument<string> destArg = new("destination", "The destination path.");

        public ZipCommand() : base("zip", "Moves the raw files without compression")
        {            
            this.AddArgument(sourceArg);
            this.AddArgument(destArg);
            this.SetHandler(async (source, destination) =>
            {
                await GenerateTempDirectories(destination);
                if (File.Exists(source))
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    await TransferFile(source);

                    Console.WriteLine($"\nCompleted file transfer in {sw.Elapsed}");
                }
                else if (Directory.Exists(source))
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    Console.WriteLine($"Scanning for files in {source}...");
                    string[] files = Directory.GetFiles(source, $"*", SearchOption.TopDirectoryOnly);
                    foreach (string file in files)
                    {
                        await TransferFile(file);
                    }

                    Console.WriteLine($"\nCompleted {files.Length.ToString("N0")} file transfers in {sw.Elapsed}");
                }
                else
                {
                    Console.WriteLine("The provided path is not valid!");
                }
            }, sourceArg, destArg);
        }

        private Task GenerateTempDirectories(string remoteDir)
        {
            TempFolder = Path.Combine(Path.GetTempPath(), $"parallel_1742178189903");
            RemoteFolder = Path.Combine(remoteDir, "files");
            SignatureFolder = Path.Combine(remoteDir, "signatures");
            DeltaFolder = Path.Combine(remoteDir, "deltas");

            if (!Directory.Exists(RemoteFolder)) Directory.CreateDirectory(RemoteFolder);
            if (!Directory.Exists(SignatureFolder)) Directory.CreateDirectory(SignatureFolder);
            if (!Directory.Exists(DeltaFolder)) Directory.CreateDirectory(DeltaFolder);
            return Task.CompletedTask;
        }

        private async Task TransferFile(string localFilePath)
        {
            //string remoteFilePath = Path.Combine(RemoteFolder, localFilePath.Replace(":", string.Empty));
            string remoteFilePath = Path.Combine(RemoteFolder, Path.GetFileName(localFilePath) + ".gz");
            string remoteParentDir = Path.GetDirectoryName(remoteFilePath);
            if (!Directory.Exists(remoteParentDir)) Directory.CreateDirectory(remoteParentDir);

            string hash = Convert.ToHexString(SHA1.HashData(Encoding.ASCII.GetBytes(localFilePath))).ToLower();
            string signatureFilePath = Path.Combine(SignatureFolder, hash);
            string deltaFilePath = Path.Combine(DeltaFolder, hash);

            string zipFilePath = Path.Combine(TempFolder, Path.GetFileName(localFilePath) + ".gz");
            await Compressor.ZipGZip(localFilePath, zipFilePath);

            await Rsync.CreateSignatureAsync(remoteFilePath, signatureFilePath);
            await Rsync.CreateDeltaAsync(zipFilePath, signatureFilePath, deltaFilePath);
            await Rsync.ApplyDeltaAsync(zipFilePath, remoteFilePath, deltaFilePath);

            //if (File.Exists(signatureFilePath)) File.Delete(signatureFilePath);
            //if (File.Exists(deltaFilePath)) File.Delete(deltaFilePath);
        }
    }
}
