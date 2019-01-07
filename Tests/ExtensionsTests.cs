using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Domain;
using System.Collections;

namespace Tests
{
    [TestFixture]
    public class ExtensionsTests
    {
        [TestCase(new byte[] { 1 }, new byte[] { 3 }, new byte[] { 1, 3 })]
        [TestCase(new byte[] { 1 }, new byte[] { 3, 4 }, new byte[] { 1, 3, 4 })]
        [TestCase(new byte[] { 1, 2 }, new byte[] { 3 }, new byte[] { 1, 2, 3 })]
        [TestCase(new byte[] { 1, 2 }, new byte[] { 3, 4 }, new byte[] { 1, 2, 3, 4 })]
        public void Extensions_AppendBytesToBytesShouldReturnExpectedResult(byte[] one, byte[] two, byte[] expected)
        {
            Assert.AreEqual(one.Append(two), expected);
        }

        [TestCase(new byte[] { 1, 2 }, "a12871fee210fb8619291eaea194581cbd2531e4b23759d225f6806923f63222")]
        [TestCase(new byte[] { 10, 11 }, "bea0b72e71bfe7f15a88c25305bf96a9681e34d3aabe0c9a1b7093cb32d8ff05")]
        public void Extensions_HashShouldReturnCorrectHash(byte[] input, string expected)
        {
            var hash = input.Hash().ToHex();
            Assert.AreEqual(expected, hash);
        }

        [TestCase(new byte[] { 1, 2 }, "0102")]
        [TestCase(new byte[] { 10, 11 }, "0a0b")]
        public void Extensions_ToBytesShouldReturnCorrectBytes(byte[] expected, string input)
        {
            Assert.AreEqual(expected, input.HexToBytes());
        }

        [TestCase(new byte[] { 1, 2 }, "0102")]
        [TestCase(new byte[] { 10, 11 }, "0a0b")]
        public void Extensions_ToHexShouldReturnCorrectString(byte[] input, string expected)
        {
            Assert.AreEqual(expected, input.ToHex());
        }

        [Test]
        public void Extensions_ToHexAndBackShouldReturnOriginalInput()
        {
            var input = new byte[] { 1, 2, 3, 4 };
            var hex = input.ToHex();
            Assert.AreEqual(input, hex.HexToBytes());
        }

        [Test]
        public void Extensions_ToBytesAndBackShouldReturnOriginalInput()
        {
            var input = "01020304";
            var bytes = input.HexToBytes();
            Assert.AreEqual(input, bytes.ToHex());
        }

        [Test]
        public void Extensions_ToBytesShouldThrowCorrectException()
        {
            var input = "odd_chars";
            Assert.Throws<FormatException>(() => input.HexToBytes());
            input = "invalid_hex";
            Assert.Throws<FormatException>(() => input.HexToBytes());
        }

        [Test]
        public void Extensions_BitArrayBitStringTest()
        {
            var array = new BitArray(9);
            Assert.AreEqual("00000000 0", array.BitString());
        }

        //[Test]
        public void Extensions_BitArrayGetCloneShouldNotChangeWhenSourceObjectChanges()
        { //testing custom method
            var bits = new BitArray(2);
            bits.Set(0, false);
            bits.Set(1, true);
            BitArray newBits = null;
            //newBits = bits.GetClone();
            bits.Not();
            Assert.AreNotEqual(bits.BitString(), newBits.BitString());
            Assert.AreNotSame(bits, newBits);
        }

        [Test]
        public void Extensions_BitArrayCloneShouldNotChangeWhenSourceObjectChanges()
        { //testing framework method
            var bits = new BitArray(2);
            bits.Set(0, false);
            bits.Set(1, true);
            var newBits = (BitArray)bits.Clone();
            bits.Not();
            Assert.AreNotEqual(bits.BitString(), newBits.BitString());
            Assert.AreNotSame(bits, newBits);
        }

        [Test]
        public void Extensions_BitArrayAppendTest()
        {
            var bits = new BitArray(1);
            bits.SetAll(true);
            var nextBits = new BitArray(2);
            nextBits.SetAll(true);
            var expectedBits = new BitArray(bits.Length + nextBits.Length);
            expectedBits.SetAll(true);
            var result = bits.Append(nextBits);
            Assert.AreEqual(expectedBits, result);
        }

        [Test]
        public void Extensions_BitArrayRoundToWholeByteTest()
        {
            var bits = new BitArray(2);
            bits.SetAll(true);
            bits = bits.RoundToWholeByte();
            var expected = new BitArray(8);
            expected.Set(0, true);
            expected.Set(1, true);
            Assert.AreEqual(expected, bits);
        }

        [Test]
        public void Extensions_ObjectToByteArrayAndReverseShouldBeEqual()
        {
            var obj = "test string";
            var bytes = obj.Serialize();
            Assert.AreEqual(obj, bytes.DeserializeTo<string>());
        }
    }
}
