using System;
using Domain;

namespace Blockchain
{
    class Program
    {
        static void Main(string[] args)
        {
            var ledger = new SimpleLedger();
            var wallet = new SimpleWallet();
            //broadcast transactions
            //receive transactions and blocks
            //hash new block using proof of work
            //broadcast proven block
            //receive block, validate transactions (not already spent)
            //maintain only the longest chain, when two chains are just as long, work on first received but keep both until next found block defines longest chain
        }
    }
}