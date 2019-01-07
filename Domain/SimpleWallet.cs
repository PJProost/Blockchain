using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class SimpleWallet
    {
        public SimpleWallet()
        {
            Keys = new List<KeySet>();
        }
        public void AddKeySet(KeySet keys)
        {
            Keys.Add(keys);
        }
        public List<KeySet> Keys { get; private set; }
    }
}