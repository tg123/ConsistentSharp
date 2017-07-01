using System.Linq;

namespace ConsistentSharp
{
    /**
     * This code is borrowed from https://github.com/damieng/DamienGKit/blob/master/CSharp/DamienG.Library/Security/Cryptography/Crc32.cs
     * 
     * Copyright (c) Damien Guard.  All rights reserved.
     * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
     * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
     * Originally published at http://damieng.com/blog/2006/08/08/calculating_crc32_in_c_and_net
     */
    internal static class Crc32
    {
        private const uint DefaultPolynomial = 0xEDB88320;

        private static readonly uint[] Table = Enumerable.Range(0, 256).Select(i =>
        {
            var entry = (uint) i;

            for (var j = 0; j < 8; j++)
            {
                if ((entry & 1) == 1)
                {
                    entry = (entry >> 1) ^ DefaultPolynomial;
                }
                else
                {
                    entry = entry >> 1;
                }
            }

            return entry;
        }).ToArray();

        public static uint Hash(byte[] data)
        {
            return ~data.Aggregate(0xFFFFFFFFU, (hash, b) => (hash >> 8) ^ Table[b ^ (hash & 0xFF)]);
        }
    }
}