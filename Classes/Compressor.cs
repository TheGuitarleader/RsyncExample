using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RsyncExample.Classes
{
    internal class Compressor
    {
        internal static async Task ZipGZip(string sourceFileName, string destFileName)
        {            
            using (FileStream originalFileStream = File.Open(sourceFileName, FileMode.Open))
            using (FileStream compressedFileStream = File.Create(destFileName))
            using (var compressor = new GZipStream(compressedFileStream, CompressionLevel.SmallestSize))
            {
                await originalFileStream.CopyToAsync(compressor);
            }
        }
    }
}
