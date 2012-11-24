﻿/*
 * Copyright (C) 2012 Arctium <http://>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace Framework.Cryptography
{
    public class PacketCrypt
    {
        public bool IsInitialized { get; set; }

        static readonly byte[] ServerEncryptionKey = { 0x08, 0xF1, 0x95, 0x9F, 0x47, 0xE5, 0xD2, 0xDB, 0xA1, 0x3D, 0x77, 0x8F, 0x3F, 0x3E, 0xE7, 0x00 };
        static readonly byte[] ServerDecryptionKey = { 0x40, 0xAA, 0xD3, 0x92, 0x26, 0x71, 0x43, 0x47, 0x3A, 0x31, 0x08, 0xA6, 0xE7, 0xDC, 0x98, 0x2A };

        SARC4 SARC4Encrypt, SARC4Decrypt;

        public PacketCrypt()
        {
            IsInitialized = false;
        }

        public void Initialize(byte[] sessionKey)
        {
            if (IsInitialized)
                throw new InvalidOperationException("PacketCrypt already initialized!");

            SARC4Encrypt = new SARC4();
            SARC4Decrypt = new SARC4();

            SARC4Encrypt.PrepareKey(ComputeHMACHash(sessionKey, ServerEncryptionKey));
            SARC4Decrypt.PrepareKey(ComputeHMACHash(sessionKey, ServerDecryptionKey));

            byte[] PacketEncryptionDummy = new byte[0x400];
            byte[] PacketDecryptionDummy = new byte[0x400];

            SARC4Encrypt.ProcessBuffer(PacketEncryptionDummy, PacketEncryptionDummy.Length);
            SARC4Decrypt.ProcessBuffer(PacketDecryptionDummy, PacketDecryptionDummy.Length);

            IsInitialized = true;
        }

        public byte[] ComputeHMACHash(byte[] value, byte[] cryptkey)
        {
            byte[] hash = new byte[20];

            MacAlgorithmProvider hMac = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);

            var binValue = CryptographicBuffer.ConvertStringToBinary(BitConverter.ToString(value), BinaryStringEncoding.Utf8);
            var binCryptkey = CryptographicBuffer.ConvertStringToBinary(BitConverter.ToString(cryptkey), BinaryStringEncoding.Utf8);
            var key = hMac.CreateKey(binCryptkey);
            
            CryptographicBuffer.CopyToByteArray(CryptographicEngine.Sign(key, binValue), out hash);

            return hash;
        }

        public void Encrypt(byte[] data)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("PacketCrypt not initialized!");

            SARC4Encrypt.ProcessBuffer(data, 4);
        }

        public void Decrypt(byte[] data)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("PacketCrypt not initialized!");

            SARC4Decrypt.ProcessBuffer(data, 4);
        }
    }
}
