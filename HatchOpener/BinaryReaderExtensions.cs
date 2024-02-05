using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HatchOpener
{
    public static class BinaryReaderExtensions
    {

        /// <summary>
        /// Reads a fixed-length ASCII string from a stream
        /// </summary>
        /// <param name="length">Amount of bytes to read</param>
        /// <returns></returns>
        public static string ReadString(this BinaryReader reader, int length)
        {
            return Encoding.ASCII.GetString(reader.ReadBytes(length));
        }

        /// <summary>
        /// Reads bytes from a stream into a byte array
        /// </summary>
        /// <param name="count">Amount of bytes to read</param>
        /// <returns>A byte array containing the read data</returns>
        public static byte[] ReadBytes(this Stream stream, int count)
        {
            var bytes = new byte[count];
            stream.Read(bytes, 0, count);
            return bytes;
        }

    }
}
