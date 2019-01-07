using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Domain
{
    public class SimpleLedger
    {
        public SimpleLedger()
        {
            ValidatedTransactions = new List<SimpleTransaction>();
        }

        public void AddGenerationTransaction(SimpleTransaction tx)
        { //only for the first tx in a block, to claim the block reward
            ValidatedTransactions.Add(tx);
        }

        public void AddTransaction(SimpleTransaction newTx)
        { //for all other transactions
            if (newTx.InputTransactionHashes == null)
                throw new SimpleLedgerException("Transaction needs input transaction(s)");

            var inputTransactions = ValidatedTransactions.Where(t => newTx.InputTransactionHashes.Exists(h => h == t.Hash)).ToList();

            if (inputTransactions.Count() < newTx.InputTransactionHashes.Count)
                throw new SimpleLedgerException("Not all input transactions are known on ledger");

            if (inputTransactions.Sum(i => i.Value) < newTx.Value)
                throw new SimpleLedgerException("Transaction value exceeds input transaction value");

            foreach (var inputTransaction in inputTransactions)
            {
                //validation of the inputs is handled by SimpleTransaction
                inputTransaction.Spend(newTx); //spend inputtx
                ValidatedTransactions.Remove(inputTransaction); //remove old inputtx from ledger
                ValidatedTransactions.Add(inputTransaction); //add new version of inputtx to ledger
            }

            ValidatedTransactions.Add(newTx); //add new transaction to ledger
        }

        public List<SimpleTransaction> ValidatedTransactions { get; private set; }
    }

    public class SimpleLedgerException : InvalidOperationException
    {
        public SimpleLedgerException(string message) : base(message)
        {
        }
        public SimpleLedgerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}