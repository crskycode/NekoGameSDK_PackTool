using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoGameSDK_PackTool
{
    static class ZipLib
    {
        public static byte[] Inflate(byte[] buffer, int capacity)
        {
            var block = new byte[256];
            var outputStream = new MemoryStream(capacity);

            var inflater = new Inflater();
            using (var memoryStream = new MemoryStream(buffer))
            using (var inflaterInputStream = new InflaterInputStream(memoryStream, inflater))
            {
                while (true)
                {
                    int numBytes = inflaterInputStream.Read(block, 0, block.Length);
                    if (numBytes < 1)
                        break;
                    outputStream.Write(block, 0, numBytes);
                }
            }

            return outputStream.ToArray();
        }

        public static byte[] Deflate(byte[] buffer, int capacity, bool appendOriginalLength)
        {
            var deflater = new Deflater(Deflater.BEST_COMPRESSION);
            using (var memoryStream = new MemoryStream(capacity))
            using (var deflaterOutputStream = new DeflaterOutputStream(memoryStream, deflater))
            {
                deflaterOutputStream.Write(buffer, 0, buffer.Length);
                deflaterOutputStream.Flush();
                deflaterOutputStream.Finish();

                if (appendOriginalLength)
                {
                    memoryStream.Write(BitConverter.GetBytes(buffer.Length));
                }

                return memoryStream.ToArray();
            }
        }
    }
}
