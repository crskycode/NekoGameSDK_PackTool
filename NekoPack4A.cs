using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoGameSDK_PackTool
{
    static class NekoPack4A
    {
        public static void Create(string filePath, string rootPath)
        {
            var entries = new List<TEntry>();

            foreach (var path in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories))
            {
                entries.Add(new TEntry
                {
                    LocalPath = path,
                    Path = Path.GetRelativePath(rootPath, path).Replace("/", "\\")
                });
            }

            using var writer = new BinaryWriter(File.Create(filePath));

            var signature = Encoding.ASCII.GetBytes("NEKOPACK4A");

            writer.Write(signature);

            writer.Write(0); // index size

            var indexPos = writer.BaseStream.Position;

            // Write index

            var nameEncoding = Encoding.GetEncoding("shift_jis");

            foreach (var entry in entries)
            {
                var nameBytes = nameEncoding.GetBytes(entry.Path);

                entry.Hash = ComputeHash(nameBytes);

                writer.Write(nameBytes.Length + 1);
                writer.Write(nameBytes);
                writer.Write((byte)0); // null-terminated

                entry.Position = writer.BaseStream.Position;

                writer.Write(0); // offset
                writer.Write(0); // length
            }

            writer.Write(0); // End of index

            var indexSize = Convert.ToUInt32(writer.BaseStream.Position - indexPos);

            // Write data

            foreach (var entry in entries)
            {
                Console.WriteLine($"Add \"{entry.Path}\"");

                var data = File.ReadAllBytes(entry.LocalPath);

                var comprData = ZipLib.Deflate(data, data.Length, true);

                EncryptData(comprData);

                entry.Offset = Convert.ToUInt32(writer.BaseStream.Position) ^ entry.Hash;
                entry.Length = Convert.ToUInt32(comprData.Length) ^ entry.Hash;

                writer.Write(comprData);
            }

            // Write index

            foreach (var entry in entries)
            {
                writer.BaseStream.Position = entry.Position;

                writer.Write(entry.Offset);
                writer.Write(entry.Length);
            }

            // Write index size

            writer.BaseStream.Position = 0xA;
            writer.Write(indexSize);

            // Done

            writer.Flush();
        }

        static uint ComputeHash(byte[] data)
        {
            int hash = 0;

            for (var i = 0; i < data.Length; i++)
            {
                hash += (sbyte)data[i];
            }

            return (uint)hash;
        }

        static void EncryptData(byte[] data)
        {
            byte key = (byte)((data.Length >> 3) + 0x22);

            for (var i = 0; i < 0x20; i++)
            {
                if (i >= data.Length)
                    break;

                data[i] ^= key;
                key *= 8;
            }
        }

        class TEntry
        {
            public string LocalPath;
            public string Path;
            public long Position;
            public uint Hash;
            public uint Offset;
            public uint Length;
        }
    }
}
