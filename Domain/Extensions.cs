using System;
using System.Linq;
using System.Security.Cryptography;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace Domain
{
    public static class Extensions
    {
        public static string ToHex(this byte[] inputBytes)
        {
            return string.Join(null, inputBytes.Select(octet => Convert.ToString(octet, 16).PadLeft(2, '0'))); //hex = base16
        }
        public static byte[] HexToBytes(this string inputHex)
        {
            if (inputHex.Length % 2 > 0)
                throw new FormatException("Hex string cannot have an odd number of characters");
            var result = new byte[inputHex.Length / 2];
            for (int i = 0; i < inputHex.Length; i += 2)
            {
                var hex = inputHex.Substring(i, 2);
                result[i / 2] = Convert.ToByte(hex, 16); //hex = base16
            }
            return result;
        }

        public static byte[] Hash(this byte[] inputBytes)
        { //always 32 bytes (256 bits), or 64 hex characters
            using (var sha = SHA256Managed.Create())
            {
                return sha.ComputeHash(inputBytes);
            }
        }

        public static byte[] Append(this byte[] bytesOne, byte[] bytesTwo)
        {
            if (bytesOne == null) return bytesTwo;
            var bytes = new byte[bytesOne.Length + bytesTwo.Length];
            bytesOne.CopyTo(bytes, 0);
            bytesTwo.CopyTo(bytes, bytesOne.Length);
            return bytes;
            //this basically mimics Linq .AddRange functionality using other Linq function .CopyTo
        }

        public static BitArray Append(this BitArray array, BitArray other)
        {
            if (array == null) return other;
            var result = new BitArray(array.Length + other.Length);
            for (int i = 0; i < array.Length; i++)
            {
                result.Set(i, array[i]);
            }
            for (int i = array.Length; i < result.Length; i++)
            {
                result.Set(i, other[i - array.Length]);
            }
            return result;
        }

        public static string BitString(this BitArray input)
        {
            //for display purposes
            var builder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (i > 0 && i % 8 == 0) builder.Append(" ");
                var bitString = "0";
                if (input[i] == true) bitString = "1";
                builder.Append(bitString);
            }
            return builder.ToString();
        }

        public static BitArray RoundToWholeByte(this BitArray bits)
        {
            //add zeroes at the end
            //untill the length of the array equals x bytes
            //where x is divisable by 8
            var workBits = (BitArray)bits.Clone(); //prevent modifying original array by reference, BitVector32 would have been a better design decision
            if (workBits.Length % 8 > 0)
            {
                workBits.Length++;
                return workBits.RoundToWholeByte();
            } else
            {
                return workBits;
            }
        }

        public static byte[] Serialize(this object obj)
        {
            if (obj == null) return null;
            else
            {
                using (var ms = new MemoryStream())
                {
                    //serialize object to stream
                    new BinaryFormatter().Serialize(ms, obj);
                    //return bytes from stream
                    return ms.ToArray();
                }
            }
        }
        public static T DeserializeTo<T>(this byte[] octets)
        {
            if (octets == null || octets.Count() == 0) return default(T);
            else
            {
                using (var ms = new MemoryStream())
                {
                    //write bytes to stream
                    ms.Write(octets, 0, octets.Length);
                    //reset position
                    ms.Position = 0;
                    //deserialize stream to object
                    return (T)new BinaryFormatter().Deserialize(ms);
                }
            }
        }
    }
}