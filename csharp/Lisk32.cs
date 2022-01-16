/* Copyright (c) 2017 Pieter Wuille
 * Copyright (c) 2021 hirishh
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Lisk32
{
	class Lisk32ConversionException: Exception
	{
		public Lisk32ConversionException()
			: base() {
		}
		public Lisk32ConversionException(String message)
			: base(message) {
		}
	}
	public static class Converter
	{
		private const string HRT = "lsk";
		private const string CHARSET_BECH32 = "zxvcpmbn3465o978uyrtkqew2adsjhfg";
		// https://go.dev/play/p/SsuX334se98
		private static readonly sbyte[] DICT_BECH32 = new sbyte[128]{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 24,  8,  9, 11, 10, 14, 15, 13, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 25,  6,  3, 26, 22, 30, 31, 29, -1, 28, 20, -1,  5,  7, 12,
			 4, 21, 18, 27, 19, 16,  2, 23,  1, 17,  0, -1, -1, -1, -1, -1,
		};
		private static uint PolyMod(byte[] input) {
			uint startValue = 1;
			for (uint i = 0; i < input.Length; i++) {
				uint c0 = startValue >> 25;
				startValue = (uint)(((startValue & 0x1ffffff) << 5) ^
					(input[i]) ^
					(-((c0 >> 0) & 1) & 0x3b6a57b2) ^
					(-((c0 >> 1) & 1) & 0x26508e6d) ^
					(-((c0 >> 2) & 1) & 0x1ea119fa) ^
					(-((c0 >> 3) & 1) & 0x3d4233dd) ^
					(-((c0 >> 4) & 1) & 0x2a1462b3));
			}
			return startValue ^ 1;
		}

		public static byte[] Combine(byte[] first, byte[] second)
		{
			byte[] ret = new byte[first.Length + second.Length];
			Buffer.BlockCopy(first, 0, ret, 0, first.Length);
			Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
			return ret;
		}
		private static byte[] createChecksum(byte[] data) {
			uint mod = PolyMod(data);
			byte[] ret = new byte[6];
			for (int i = 0; i < 6; i++) {
				ret[i] = (byte) ((mod >> (5 * (5 - i))) & 31);
			}
			return ret;
		}
		private static bool verifyChecksum(byte[] dataWithChecksum) {
			return PolyMod(dataWithChecksum) == 0;
		}
		private static int convertBits(byte[] bytes, int inBits, uint outBits, byte[] converted, int len, bool pad=true) {
			//byte[] converted = new byte[bytes.Length * inBits / outBits + (bytes.Length * inBits % outBits != 0 ? 1 : 0)];
			uint bits = 0, maxv = (uint) ((1 << (int) outBits) - 1), val = 0, inPos = 0, outPos=0;
			//int len = bytes.Length - inPos;
			while (len-- != 0) {
				val = (val << inBits) | bytes[inPos++];
				bits += (uint) inBits;
				while (bits >= outBits) {
					bits -= outBits;
					converted[outPos++] = (byte) ((val >> (int) bits) & maxv);
				}
			}
			if (pad) {
				if (bits != 0) {
					converted[outPos++] = (byte)((val << (int)(outBits - bits)) & maxv);
				}
			} else if ((((val << (int) (outBits - bits)) & maxv) != 0) || bits >= inBits) {
				throw new Lisk32ConversionException("Bit conversion error! bits:" + bits + " | inBits:" + inBits + " - " + ((val << (int) (outBits - bits)) & maxv));
			}
			return (int) outPos;
		}

		// address = 20 byte Lisk address
		public static string EncodeLisk32(byte[] address)
		{
			if (address.Length != 20) {
				throw new Lisk32ConversionException("Invalid lisk address!");
			}
			// byte[] data = new byte[100];
			byte[] data = Enumerable.Repeat((byte)0x00, 100).ToArray();
			int len = convertBits(address, 8, 5, data, address.Length);
			Array.Resize(ref data, len + 6); // Add checksum zeros
			byte[] checksum = createChecksum(data);

			StringBuilder ret = new StringBuilder(HRT);
			for (int i = 0; i < len; i++) {
				ret.Append(CHARSET_BECH32[data[i]]);
			}
			for (int i = 0; i < 6; i++) {
				ret.Append(CHARSET_BECH32[checksum[i]]);
			}
			return ret.ToString();
		}

		public static byte[] DecodeLisk32(string addr) {
			if (!addr.StartsWith(HRT)) {
				throw new Lisk32ConversionException("Invalid Lisk32 address: wrong prefix (lsk)");
			}
			int dataLen = addr.Length - 3;
			byte[] data = new byte[dataLen];
			for (int i = 0; i < dataLen; i++) {
				data[i] = (byte) addr[3 + i];
			}
			sbyte err = 0;
			for (int i = 0; i < dataLen; i++) {
				sbyte k = DICT_BECH32[data[i]];
				err |= k;
				data[i] = unchecked((byte) k);
			}
			if (err == -1 || !verifyChecksum(data)) {
				throw new Lisk32ConversionException("Invalid Lisk32 address: wrong checksum");
			}
			byte[] decoded = new byte[40];
			int decodedLen = convertBits(data, 5, 8, decoded, dataLen - 6);
			if (decodedLen != 20) {
				throw new Lisk32ConversionException("Invalid Lisk32 address: wrong decoded length");
			}
			byte[] final = new byte[decodedLen];
			System.Buffer.BlockCopy(decoded, 0, final, 0, decodedLen);
			return final;
		}
	}
}
