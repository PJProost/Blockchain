using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Domain
{
    [System.Diagnostics.DebuggerDisplay("{Hash.ToHex()}")]
    public class MerkleTree
    {
        public MerkleTree(byte[] bytes)
        {
            Depth = 0;
            Hash = bytes.Hash();
        }
        public MerkleTree(MerkleTree leafOne, MerkleTree leafTwo)
        {
            Depth = leafOne.Depth + 1;
            if (leafTwo.Depth > Depth)
            {
                Depth = leafTwo.Depth + 1;
            }

            BranchOne = leafOne;
            BranchTwo = leafTwo;

            byte[] bytes = leafOne.Hash.Append(leafTwo.Hash);
            Hash = bytes.Hash();
        }

        public void Prune(int depthToRetain)
        {
            if (Depth == 0 || depthToRetain == 0)
            {
                //set new depth
                Depth = 0;
                //make sure there is nothing below this depth
                BranchOne = null;
                BranchTwo = null;
            }
            else if (Depth > depthToRetain)
            {
                //set new depth
                Depth = depthToRetain;
                //check one level deeper
                BranchOne.Prune(depthToRetain - 1);
                BranchTwo.Prune(depthToRetain - 1);
            }
            //else, no action needed (Depth <= depthToRetain)
        }

        public bool Contains(MerkleTree tree)
        {
            return Contains(tree.Hash);
        }
        public bool Contains(byte[] hash)
        {
            if (hash == Hash
                || (BranchOne != null && BranchOne.Contains(hash))
                || (BranchTwo != null && BranchTwo.Contains(hash)) )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int Depth { get; private set; }
        public byte[] Hash { get; private set; }
        public MerkleTree BranchOne { get; private set; }
        public MerkleTree BranchTwo { get; private set; }
    }
}
