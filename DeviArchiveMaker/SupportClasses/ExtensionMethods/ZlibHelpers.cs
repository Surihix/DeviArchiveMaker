using Ionic.Zlib;
using System.IO;

namespace DeviArchiveMaker.SupportClasses
{
    internal static class ZlibHelpers
    {
        public static byte[] ZlibDecompressBuffer(this byte[] dataToDcmp)
        {
            return ZlibStream.UncompressBuffer(dataToDcmp);
        }

        public static byte[] ZlibCompressWithLvl(this byte[] dataToCmp, CompressionLevel compressionLevel)
        {
            var compressedDataBuffer = new byte[] { };

            using (var cmpData = new MemoryStream())
            {
                using (var compressor = new ZlibStream(cmpData, CompressionMode.Compress, compressionLevel, true))
                {
                    compressor.Write(dataToCmp, 0, dataToCmp.Length);
                }

                cmpData.Seek(0, SeekOrigin.Begin);
                compressedDataBuffer = cmpData.ToArray();
            }

            return compressedDataBuffer;
        }
    }
}