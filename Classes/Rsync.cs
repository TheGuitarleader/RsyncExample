using FastRsync.Core;
using FastRsync.Delta;
using FastRsync.Signature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RsyncExample.Classes
{
    public static class Rsync
    {
        private static ProgressLogging Logging = new ProgressLogging();
        private static int DeltaSize = 206;

        public static async Task CreateSignatureAsync(string remoteFilePath, string signatureFilePath)
        {
            SignatureBuilder signatureBuilder = new SignatureBuilder();
            using (var basisStream = new FileStream(remoteFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            using (var signatureStream = new FileStream(signatureFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await signatureBuilder.BuildAsync(basisStream, new SignatureWriter(signatureStream));
            }

            Console.WriteLine($"Created signature '{signatureFilePath}' ({new FileInfo(signatureFilePath).Length} bytes) for file: {remoteFilePath}");
        }

        public static async Task CreateDeltaAsync(string localFilePath, string signatureFilePath, string deltaFilePath)
        {
            DeltaBuilder delta = new DeltaBuilder();
            using (var newFileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var signatureStream = new FileStream(signatureFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var deltaStream = new FileStream(deltaFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await delta.BuildDeltaAsync(newFileStream, new SignatureReader(signatureStream, Logging), new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaStream)));
            }

            Console.WriteLine($"Created delta '{deltaFilePath}' ({new FileInfo(deltaFilePath).Length} bytes) for file: {localFilePath}");
        }

        public static async Task ApplyDeltaAsync(string localFilePath, string remoteFilePath, string deltaFilePath)
        {
            DeltaApplier delta = new DeltaApplier();
            using (var basisStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var deltaStream = new FileStream(deltaFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var newFileStream = new FileStream(remoteFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                await delta.ApplyAsync(basisStream, new BinaryDeltaReader(deltaStream, Logging), newFileStream);
            }

            Console.WriteLine($"Applied {new FileInfo(deltaFilePath).Length - DeltaSize} bytes to file: {remoteFilePath}");
        }
    }
}
