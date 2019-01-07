using System;
using System.Collections.Generic;
using System.Text;
using Domain;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class SimpleLedgerTests
    {
        [SetUp]
        public void SetUp()
        {
            minedTx1 = new SimpleTransaction(key.Address, 1);
            minedTx2 = new SimpleTransaction(key.Address, 1);
            simpleLedger.AddGenerationTransaction(minedTx1);
            simpleLedger.AddGenerationTransaction(minedTx2);

            validTx = new SimpleTransaction(key.Address, new List<SimpleTransaction>() { minedTx1 }, minedTx1.Value);
        }

        [Test]
        public void SimpleLedger_AddTransactionShouldAddValidTx()
        {
            var expectedNumberOfTx = simpleLedger.ValidatedTransactions.Count + 1;

            simpleLedger.AddTransaction(validTx);

            Assert.IsTrue(simpleLedger.ValidatedTransactions.Contains(validTx));
            Assert.AreEqual(expectedNumberOfTx, simpleLedger.ValidatedTransactions.Count);
        }

        [Test]
        public void SimpleLedger_AddTransactionShouldNotAddInvalidTx()
        {
            var newKey = new KeySet();
            var fabricatedTx = new SimpleTransaction(newKey.Address, 1); //not added to ledger
            var invalidTx = new SimpleTransaction(newKey.Address, new List<SimpleTransaction>() { fabricatedTx }, fabricatedTx.Value);
            var expectedNumberOfTx = simpleLedger.ValidatedTransactions.Count;

            Assert.Throws<SimpleLedgerException>(() => simpleLedger.AddTransaction(invalidTx));

            Assert.IsFalse(simpleLedger.ValidatedTransactions.Contains(invalidTx));
            Assert.AreEqual(expectedNumberOfTx, simpleLedger.ValidatedTransactions.Count);
        }

        //[Test]
        public void SimpleLedger_AddTransactionShouldNotAddStealTx()
        {
            var newKey = new KeySet();
            var stealTx = new SimpleTransaction(newKey.Address, new List<SimpleTransaction>() { minedTx1 }, minedTx1.Value);
            //stealTx.FromAddress = newKey.Address;
            //TODO SimpleTransaction doesn't allow creating a steal transaction, make a stub that does so this can be tested again
            var expectedNumberOfTx = simpleLedger.ValidatedTransactions.Count;

            Assert.Throws<InvalidOperationException>(() => simpleLedger.AddTransaction(stealTx));

            Assert.IsFalse(simpleLedger.ValidatedTransactions.Contains(stealTx));
            Assert.AreEqual(expectedNumberOfTx, simpleLedger.ValidatedTransactions.Count);
        }

        [Test]
        public void SimpleLedger_AddTransactionShouldNotAddDoubleSpendTx()
        {
            var doubleSpendTx = new SimpleTransaction(key.Address, new List<SimpleTransaction>() { minedTx1 }, minedTx1.Value);
            var doubleSpendTx2 = new SimpleTransaction(new KeySet().Address, new List<SimpleTransaction>() { minedTx1 }, minedTx1.Value);
            var expectedNumberOfTx = simpleLedger.ValidatedTransactions.Count + 1;

            simpleLedger.AddTransaction(validTx);
            Assert.Throws<SimpleTransactionException>(() => simpleLedger.AddTransaction(doubleSpendTx));
            Assert.Throws<SimpleTransactionException>(() => simpleLedger.AddTransaction(doubleSpendTx2));

            Assert.IsFalse(simpleLedger.ValidatedTransactions.Contains(doubleSpendTx));
            Assert.AreEqual(expectedNumberOfTx, simpleLedger.ValidatedTransactions.Count);
        }

        //[Test]
        public void SimpleLedger_AddTransactionShouldNotAddOverspendTx()
        {
            //this is already catched by the SimpleTransaction initializer so should never throw a SimpleLedgerException

            var tooHighTx = new SimpleTransaction(new KeySet().Address, new List<SimpleTransaction>() { validTx }, validTx.Value + 1);
            var expectedNumberOfTx = simpleLedger.ValidatedTransactions.Count + 1;

            simpleLedger.AddTransaction(validTx);
            Assert.Throws<SimpleLedgerException>(() => simpleLedger.AddTransaction(tooHighTx));

            Assert.IsFalse(simpleLedger.ValidatedTransactions.Contains(tooHighTx));
            Assert.AreEqual(expectedNumberOfTx, simpleLedger.ValidatedTransactions.Count);
        }

        [Test]
        public void SimpleLedger_AddTransactionShouldGenerateChangeTx()
        {
            var partialTx = new SimpleTransaction(new KeySet().Address, new List<SimpleTransaction>() { validTx }, validTx.Value * 2 / 3);
            var changeTx = partialTx.ChangeTransaction();
            var expectedNumberOfTx = simpleLedger.ValidatedTransactions.Count + 3;

            simpleLedger.AddTransaction(validTx);
            simpleLedger.AddTransaction(partialTx);
            if (changeTx != null) simpleLedger.AddTransaction(changeTx);

            Assert.IsTrue(simpleLedger.ValidatedTransactions.Contains(partialTx));
            Assert.IsTrue(simpleLedger.ValidatedTransactions.Contains(changeTx));
            Assert.AreEqual(expectedNumberOfTx, simpleLedger.ValidatedTransactions.Count);
        }

        KeySet key = new KeySet();
        SimpleTransaction minedTx1 = null;
        SimpleTransaction minedTx2 = null;
        SimpleLedger simpleLedger = new SimpleLedger();
        SimpleTransaction validTx = null;
    }
}
