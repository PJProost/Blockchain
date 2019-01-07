using System;
using Domain;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MerkleTreeTests
    {
        [SetUp]
        public void SetUp()
        {
            leafOne = new MerkleTree(bytesOne); //depth 0
            leafTwo = new MerkleTree(bytesTwo); //depth 0
            branchOne = new MerkleTree(leafOne, leafTwo); //depth 1
            branchTwo = new MerkleTree(leafTwo, leafOne); //depth 1
            treeRoot = new MerkleTree(branchOne, branchTwo); //depth 2
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void MerkleTree_PruneShouldMakeTreeCorrectDepth(int depthToRetain)
        {
            treeRoot.Prune(depthToRetain);
            Assert.AreEqual(depthToRetain, treeRoot.Depth);
        }

        [Test]
        public void MerkleTree_ContainsShouldReturnTrueWhenAppropriate()
        {
            Assert.IsTrue(branchOne.Contains(leafOne));
        }
        [Test]
        public void MerkleTree_ContainsShouldReturnTrueWhenAppropriate2()
        {
            Assert.IsTrue(treeRoot.Contains(leafOne));
        }
        [Test]
        public void MerkleTree_ContainsShouldReturnFalseWhenAppropriate()
        {
            Assert.IsFalse(branchOne.Contains(branchTwo));
        }

        [Test]
        public void MerkleTree_DepthShouldReturnCorrectDepth()
        {
            Assert.AreEqual(0, leafOne.Depth);
            Assert.AreEqual(1, branchOne.Depth);
            Assert.AreEqual(2, treeRoot.Depth);
        }

        [Test]
        public void MerkleTree_ConstructingWithBytesShouldSetCorrectHash()
        {
            Assert.AreEqual(leafOne.Hash.ToHex(), "b413f47d13ee2fe6c845b2ee141af81de858df4ec549a58b7970bb96645bc8d2");
        }
        [Test]
        public void MerkleTree_ConstructingWithBytesShouldNotSetBranches()
        {
            Assert.IsNull(leafOne.BranchOne);
            Assert.IsNull(leafOne.BranchTwo);
        }

        [Test]
        public void MerkleTree_ConstructingWithTreesShouldSetCorrectHash()
        {
            Assert.AreEqual(treeRoot.Hash.ToHex(), "589694bfe76b2c8cd88c4784f85b662e2a348f2f21067a29c9a61aca65a308e8");
        }
        [Test]
        public void MerkleTree_ConstructingWithTreesShouldSetBranches()
        {
            Assert.IsNotNull(treeRoot.BranchOne);
            Assert.IsNotNull(treeRoot.BranchTwo);
        }

        byte[] bytesOne = new byte[] { 0x0, 0x1 };
        byte[] bytesTwo = new byte[] { 0x1, 0x1 };
        MerkleTree leafOne = null;
        MerkleTree leafTwo = null;
        MerkleTree branchOne = null;
        MerkleTree branchTwo = null;
        MerkleTree treeRoot = null;
    }
}
