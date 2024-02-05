using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static HatchOpener.Encryption;

namespace HatchOpener
{
    public class DataPack
    {
        protected Stream _stream;
        protected MD5 _md5;
        public List<DataPackFileEntry> Files = new List<DataPackFileEntry>();

        public void Load(Stream stream, List<string> fileList)
        {
            // Reader
            var reader = new BinaryReader(_stream = stream);

            // Check signature
            if (reader.ReadString(5) != "HATCH")
                throw new Exception("Invalid signature found!");

            if (reader.ReadByte() != 1)
                throw new Exception("Invalid version found!");
            reader.ReadUInt16();
            int fileCount = reader.ReadUInt16();

            // Read Files
            for (int i = 0; i < fileCount; ++i)
                Files.Add(ReadFile(reader, fileList));
        }

        protected DataPackFileEntry ReadFile(BinaryReader reader, List<string> filenames)
        {
            uint hash         = reader.ReadUInt32();
            long dataPosition = reader.ReadInt64();
            long fileLength   = reader.ReadInt64();
            int  flags        = reader.ReadInt32();
            long dataLength   = reader.ReadInt64();
            bool known = filenames.Any(t => SG_HashString(t, 0xFFFFFFFF) == hash);
            string fileName = filenames.FirstOrDefault(t => SG_HashString(t, 0xFFFFFFFF) == hash) ?? ((flags & 2) != 0 ? $"EncryptedUnknown/{hash:X8}.bin" : $"Unknown/{hash:X8}.bin");
            SGKey keys = GenerateKeys(fileName, dataLength);

            // Create file entry
            return new DataPackFileEntry(fileName, hash, dataPosition, fileLength, flags, dataLength, keys, known);
        }

        public List<string> GetObjectPathsFromObjectList(List<string> objectList)
        {
            var paths = new List<string>();

            // Hash object names
            foreach (var filename in objectList)
            {
                paths.Add(string.Format("Objects/{0:X8}.ibc", SG_HashString(filename, 0xFFFFFFFF)));
            }

            return paths;
        }

        protected static string ByteArrayToString(byte[] bytes)
        {
            string s = "";
            for (int i = 0; i < bytes.Length; ++i)
                s += string.Format("{0:X2}", bytes[i]);
            return s;
        }

        public void ExtractAllFiles(string path)
        {
            foreach (var file in Files)
            {
                // Set location
                _stream.Position = file.DataPosition;
                // Create directories
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(path, file.FileName)));
                // Read data
                var data = _stream.ReadBytes((int)file.DataLength);
                // Decrypt
                if ((file.Flags & 2) != 0)
                {
                    Console.WriteLine("{0} is encrypted!", file.FileName);
                    DecryptData(data, file.Keys);
                }
                // Write data
                File.WriteAllBytes(Path.Combine(path, file.FileName), data);
            }
        }

        public void Dispose()
        {
            _md5.Dispose();
        }

        public class DataPackFileEntry
        {
            public string FileName = "";
            public uint Hash = 0;
            public long DataPosition = 0;
            public long FileLength = 0;
            public long DataLength = 0;
            public int Flags = 0;
            public bool Known = false;
            public SGKey Keys;

            // Constructor
            public DataPackFileEntry(string fileName, uint hash, long dataPosition, long fileLength, int flags, long dataLength, SGKey keys, bool known)
            {
                FileName = fileName;
                Hash = hash;
                DataPosition = dataPosition;
                FileLength = fileLength;
                Flags = flags;
                DataLength = dataLength;
                Keys = keys;
                Known = known;
            }
        }

    }
}
