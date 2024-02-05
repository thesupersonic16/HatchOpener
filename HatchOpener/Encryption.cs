using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HatchOpener
{
    public static class Encryption
    {

        public static SGKey GenerateKeys(string fileName, long size)
        {
            var key = new SGKey();

            // Key1
            key.Key1 = new byte[16];
            key.Key2 = new byte[16];
            byte[] key1 = BitConverter.GetBytes(SG_HashString(fileName, 0xFFFFFFFF));
            byte[] key2 = BitConverter.GetBytes(SG_HashArray(BitConverter.GetBytes(size), 0xFFFFFFFF));
            key1.CopyTo(key.Key1, 0);
            key2.CopyTo(key.Key2, 0);
            key1.CopyTo(key.Key1, 4);
            key2.CopyTo(key.Key2, 4);
            key1.CopyTo(key.Key1, 8);
            key2.CopyTo(key.Key2, 8);
            key1.CopyTo(key.Key1, 12);
            key2.CopyTo(key.Key2, 12);
            return key;
        }

        public static void DecryptData(byte[] data, SGKey keys)
        {
            // Init keys
            bool swapNibble = false;
            int key1Position = 0;
            int key2Position = 8;
            int keyPosition = (data.Length >> 2) & 0x7F;

            for (int i = 0; i < data.Length; ++i)
            {
                var value = data[i] ^ keyPosition ^ keys.Key2[key2Position++];
                if (swapNibble)
                    value = ((value & 0xF0) >> 4) | ((value & 0x0F) << 4);
                data[i] = (byte)(value ^ keys.Key1[key1Position++]);

                if (key1Position > 0x0F)
                {
                    if (key2Position > 0x8)
                    {
                        keyPosition += 2;
                        keyPosition &= 0x7F;
                        if (swapNibble)
                        {
                            key1Position = keyPosition % 7;
                            key2Position = (keyPosition % 0x0C) + 2;
                        }
                        else
                        {
                            key1Position = (keyPosition % 0x0C) + 3;
                            key2Position = keyPosition % 7;
                        }
                        swapNibble = !swapNibble;
                    }
                    else
                    {
                        key1Position = 0;
                        swapNibble = !swapNibble;
                    }
                }
                else if (key2Position > 0x0C)
                {
                    key2Position = 0;
                    swapNibble = !swapNibble;
                }
            }
        }

        public static uint SG_HashString(string input, uint mask)
        {
            int length = input.Length;
            long v4 = mask;
            for (int i = 0; length != 0; --length)
            {
                byte v6 = (byte)input[i++];
                long v7 = ((v6 ^ v4) >> 1) ^ -(((byte)v6 ^ (byte)v4) & 1) & 0xEDB88320;
                long v8 = (((v7 >> 1) ^ -(v7 & 1) & 0xEDB88320) >> 1) ^ -(((byte)(v7 >> 1) ^ -(v7 & 1) & 0x20) & 1) & 0xEDB88320;
                long v9 = (((v8 >> 1) ^ -(v8 & 1) & 0xEDB88320) >> 1) ^ -(((byte)(v8 >> 1) ^ -(v8 & 1) & 0x20) & 1) & 0xEDB88320;
                long v10 = (((v9 >> 1) ^ -(v9 & 1) & 0xEDB88320) >> 1) ^ -(((byte)(v9 >> 1) ^ -(v9 & 1) & 0x20) & 1) & 0xEDB88320;
                v4 = (v10 >> 1) ^ -(v10 & 1) & 0xEDB88320;
            }
            return (uint)(~v4);
        }

        public static uint SG_HashArray(byte[] input, uint mask)
        {
            int length = input.Length;
            long v4 = mask;
            for (int i = 0; length != 0; --length)
            {
                byte v6 = (byte)input[i++];
                long v7 = ((v6 ^ v4) >> 1) ^ -(((byte)v6 ^ (byte)v4) & 1) & 0xEDB88320;
                long v8 = (((v7 >> 1) ^ -(v7 & 1) & 0xEDB88320) >> 1) ^ -(((byte)(v7 >> 1) ^ -(v7 & 1) & 0x20) & 1) & 0xEDB88320;
                long v9 = (((v8 >> 1) ^ -(v8 & 1) & 0xEDB88320) >> 1) ^ -(((byte)(v8 >> 1) ^ -(v8 & 1) & 0x20) & 1) & 0xEDB88320;
                long v10 = (((v9 >> 1) ^ -(v9 & 1) & 0xEDB88320) >> 1) ^ -(((byte)(v9 >> 1) ^ -(v9 & 1) & 0x20) & 1) & 0xEDB88320;
                v4 = (v10 >> 1) ^ -(v10 & 1) & 0xEDB88320;
            }
            return (uint)(~v4);
        }


        public struct SGKey
        {
            public byte[] Key1;
            public byte[] Key2;
        }
    }
}
