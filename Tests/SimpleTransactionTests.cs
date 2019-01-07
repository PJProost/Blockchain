using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Domain;
using NUnit.Framework;
using System.Linq;
using System.Security.Cryptography;

namespace Tests
{
    [TestFixture]
    public class SimpleTransactionTests
    {
        [SetUp]
        public void SetUp()
        {
            minedTx1 = new SimpleTransaction(key.Address, 1);
            minedTx2 = new SimpleTransaction(key.Address, 1);

            validTx = new SimpleTransaction(key.Address, new List<SimpleTransaction>() { minedTx1 }, minedTx1.Value);
        }

        [Test]
        public void SimpleTransaction_SerializeDeserialize_GenerationTx_Test()
        {
            var bytes = minedTx1.Serialize(null);
            var obj = SimpleTransaction.Deserialize(bytes);
            Assert.AreEqual(minedTx1, obj);
        }

        [Test]
        public void SimpleTransaction_SerializeDeserialize_RealTx_Test()
        {
            inputTransactions = new List<SimpleTransaction>() { minedTx1, minedTx2 };
            //generate transaction
            var validTx = new SimpleTransaction(key.Address, inputTransactions, inputTransactions.Sum(t => t.Value));
            var bytes = validTx.Serialize(new List<KeySet>() { key });
            var obj = SimpleTransaction.Deserialize(bytes);
            //asserts
            Assert.AreEqual(inputTransactions.Sum(t => t.Value), validTx.Value);
            Assert.AreEqual(inputTransactions.Count, validTx.InputTransactionHashes.Count);
            Assert.AreEqual(key.Address, validTx.ToAddress);
            Assert.AreEqual(validTx, obj);
        }

        [Test]
        public void SimpleTransaction_InitializerShouldNotAllowOverspendTx()
        {
            SimpleTransaction tooHighTx = null;
            Assert.Throws<SimpleTransactionException>(() => tooHighTx = new SimpleTransaction(new KeySet().Address, new List<SimpleTransaction>() { validTx }, validTx.Value + 1));
        }

        //[Test]
        public void SimpleTransaction_SignatureTest()
        {
            var data = new byte[] { 0, 2 };
            byte[] signature = null;
            byte[] privatekey = null;
            byte[] publickey = null;
            using (var rsaCsp = new RSACryptoServiceProvider())
            {
                privatekey = rsaCsp.ExportCspBlob(true);
                publickey = rsaCsp.ExportCspBlob(false);
                signature = rsaCsp.SignData(data, "SHA256");
            }
            using (var rsaCsp = new RSACryptoServiceProvider())
            {
                rsaCsp.ImportCspBlob(publickey);
                Assert.IsTrue(rsaCsp.VerifyData(data, "SHA256", signature));
            }
        }

        //[Test]
        public void SimpleTransaction_SplitBytesTest()
        {
            var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var signature = new List<byte>();
            var publicKey = new List<byte>();
            var transaction = new List<byte>();
            var i = 0;
            foreach (var octet in data)
            {
                if (i < 2) signature.Add(octet);
                else if (i < 2 + 4) publicKey.Add(octet);
                else transaction.Add(octet);
                i++;
            }
            Assert.AreEqual(signature.ToArray(), new byte[] { 0, 1 });
            Assert.AreEqual(publicKey.ToArray(), new byte[] { 2, 3, 4, 5 });
            Assert.AreEqual(transaction.ToArray(), new byte[] { 6, 7, 8, 9, 10 });
        }

        //[Test]
        public void SimpleTransaction_UintToIntTest()
        {
            uint value = 1;
            int expected = 1;
            Assert.AreEqual(value, expected);
        }

        KeySet key = new KeySet();
        SimpleTransaction minedTx1 = null;
        SimpleTransaction minedTx2 = null;
        List<SimpleTransaction> inputTransactions = null;
        SimpleTransaction validTx = null;
    }
}
