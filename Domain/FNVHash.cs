using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class FNVHash
    {
        public static UInt32 FNV1aIn32bit(byte[] array)
        {
            var hash = offset32bit;
            foreach (var octet in array)
            {
                hash = hash ^ octet;
                hash = hash * prime32bit;
            }
            return hash;
        }
        public static UInt32 FNV1In32bit(byte[] array)
        {
            var hash = offset32bit;
            foreach (var octet in array)
            {
                hash = hash * prime32bit;
                hash = hash ^ octet;
            }
            return hash;
        }
        public static UInt64 FNV1aIn64bit(byte[] array)
        {
            var hash = offset64bit;
            foreach (var octet in array)
            {
                hash = hash ^ octet;
                hash = hash * prime64bit;
            }
            return hash;
        }
        public static UInt64 FNV1In64bit(byte[] array)
        {
            var hash = offset64bit;
            foreach (var octet in array)
            {
                hash = hash * prime64bit;
                hash = hash ^ octet;
            }
            return hash;
        }

        private const UInt32 prime32bit = 16777619;
        private const UInt32 offset32bit = 2166136261;
        public const UInt64 prime64bit = 1099511628211;
        public const UInt64 offset64bit = 14695981039346656037;
    }
}
