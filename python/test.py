#!/usr/bin/python3

# Copyright (c) 2021 hirish
# Copyright (c) 2017, 2020 Pieter Wuille
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.


"""Reference tests for lisk32 adresses"""

import binascii
import unittest
import lisk32

def segwit_scriptpubkey(witver, witprog):
    """Construct a Segwit scriptPubKey for a given witness program."""
    return bytes([witver + 0x50 if witver else 0, len(witprog)] + witprog)

VALID_ADDRESS = [
    ["lsk24cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5eu", "c247a42e09e6aafd818821f75b2f5b0de47c8235"],
    ["lskxwnb4ubt93gz49w3of855yy9uzntddyndahm6s", "0dce64c0d36a3e04b6e8679eb5c62d800f3d6a27"],
    ["lskzkfw7ofgp3uusknbetemrey4aeatgf2ntbhcds", "053d7733df22210dd0e6b4ec595a29cdb33ffb07"],
]

INVALID_ADDRESS = [
    "24cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5eu"    ,  # missing prefix
    "lsk24cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5e"  ,  # incorrect length (length 40 instead of 41)
    "lsk24cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5euu",  # incorrect length (length 42 instead of 41)
    "LSK24cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5eu" ,  # incorrect prefix
    "tsk24cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5eu" ,  # incorrect prefix
    "lsk24cd35u4jdq8sz03pnsqe5dsxwrnazyqqqg5eu" ,  # invalid character (contains a zero)
    "lsk24Cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5eu" ,  # invalid character (contains an upper case 'C' instead of lower case 'c')
    "lsk24dc35u4jdq8szo3pnsqe5dsxwrnazyqqqg5eu" ,  # invalid checksum due to character swap
]

class TestLisk32Address(unittest.TestCase):
    """Unit test class for lisk32 addressess."""

    def test_valid_address(self):
        """Test whether valid addresses decode to the correct output."""
        for (address, binAddress) in VALID_ADDRESS:
            decoded = lisk32.decode(address)
            self.assertIsNotNone(decoded, address)
            self.assertEqual(bytes(decoded), binascii.unhexlify(binAddress))
            addr = lisk32.encode(decoded)
            self.assertEqual(address.lower(), addr)

    def test_invalid_address(self):
        """Test whether invalid addresses fail to decode."""
        for test in INVALID_ADDRESS:
            res = lisk32.decode(test)
            self.assertIsNone(res)

if __name__ == "__main__":
    unittest.main()