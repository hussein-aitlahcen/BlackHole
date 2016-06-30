using System.IO;

namespace BlackHole.Common.Extentions
{
    public static class ByteArrayExtensions
    {
        public static byte[] CompressLz4(this byte[] array)
        {
            using (var output = new MemoryStream())
            {
                using (var lzStream = new LZ4s.LZ4Stream(output, LZ4s.CompressionMode.Compress, array.Length))
                {
                    lzStream.Write(array, 0, array.Length);
                    lzStream.Flush();
                    return output.ToArray();
                }
            }
        }

        public static byte[] DecompressLz4(this byte[] array)
        {
            using (var output = new MemoryStream())
            {
                using (var lzStream = new LZ4s.LZ4Stream(new MemoryStream(array), LZ4s.CompressionMode.Decompress, array.Length))
                {
                    lzStream.CopyTo(output);
                    return output.ToArray();
                }
            }
        }
    }
}
