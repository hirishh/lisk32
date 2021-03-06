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

"""Reference implementation for Lisk32 addresses. (based on bech32)"""


from enum import Enum

class Encoding(Enum):
    """Enumeration type to list the various supported encodings."""
    BECH32 = 1

CHARSET = "zxvcpmbn3465o978uyrtkqew2adsjhfg"
HRP = 'lsk'

def bech32_polymod(values):
    """Internal function that computes the Bech32 checksum."""
    generator = [0x3b6a57b2, 0x26508e6d, 0x1ea119fa, 0x3d4233dd, 0x2a1462b3]
    chk = 1
    for value in values:
        top = chk >> 25
        chk = (chk & 0x1ffffff) << 5 ^ value
        for i in range(5):
            chk ^= generator[i] if ((top >> i) & 1) else 0
    return chk

def bech32_verify_checksum(data):
    """Verify a checksum given converted data characters."""
    const = bech32_polymod(data)
    if const == 1:
        return Encoding.BECH32
    return None

def bech32_create_checksum(data):
    """Compute the checksum values of given data."""
    const = 1
    polymod = bech32_polymod(data + [0, 0, 0, 0, 0, 0]) ^ const
    return [(polymod >> 5 * (5 - i)) & 31 for i in range(6)]


def bech32_encode(hrp, data):
    """Compute a Lisk32 string given HRP and data values."""
    combined = data + bech32_create_checksum(data)
    return hrp + ''.join([CHARSET[d] for d in combined])

def bech32_decode(bech):
    """Validate a Lisk32 string, and determine HRP and data."""
    if ((any(ord(x) < 33 or ord(x) > 126 for x in bech)) or
            (bech.lower() != bech and bech.upper() != bech)):
        return (None, None, None)
    bech = bech.lower()
    pos = 3
    if not all(x in CHARSET for x in bech[pos:]):
        return (None, None, None)
    hrp = bech[:pos]
    data = [CHARSET.find(x) for x in bech[pos:]]
    spec = bech32_verify_checksum(data)
    if spec is None:
        return (None, None, None)
    return (hrp, data[:-6], spec)

def convertbits(data, frombits, tobits, pad=False):
    """General power-of-2 base conversion."""
    acc = 0
    bits = 0
    ret = []
    maxv = (1 << tobits) - 1
    max_acc = (1 << (frombits + tobits - 1)) - 1
    for value in data:
        if value < 0 or (value >> frombits):
            return None
        acc = ((acc << frombits) | value) & max_acc
        bits += frombits
        while bits >= tobits:
            bits -= tobits
            ret.append((acc >> bits) & maxv)
    if pad:
        if bits:
            ret.append((acc << (tobits - bits)) & maxv)
    elif bits >= frombits or ((acc << (tobits - bits)) & maxv):
        return None
    return ret


def decode(addr):
    """Decode a Lisk32 address."""
    hrpgot, data, spec = bech32_decode(addr)
    if hrpgot != HRP:
        return None
    decoded = convertbits(data, 5, 8, False)
    if decoded is None or len(decoded) != 20:
        return None
    if spec != Encoding.BECH32:
        return None
    return decoded


def encode(addr):
    """Encode a Lisk32 address."""
    ret = bech32_encode(HRP, convertbits(addr, 8, 5))
    if decode(ret) == None:
        return None
    return ret