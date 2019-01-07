using System;
using System.Collections.Generic;
using System.Text;
using Domain;
using NUnit.Framework;
using System.Linq;
using System.Security.Cryptography;

namespace Tests
{
    [TestFixture]
    public class KeySetTests
    {
        [Test]
        public void KeySet_ShouldGenerateCert()
        {
            var keySet = new KeySet();
            Assert.IsNotNull(keySet.PrivateKey);
            Assert.IsNotNull(keySet.PublicKey);
            Assert.IsNotNull(keySet.Address);
        }

        [Test]
        public void KeySet_ShouldImportCert()
        {
            var keySet = new KeySet("0702000000a40000525341320004000001000100417b427c924502a6db461263a441a1c86d2b0ce215a6d29f2740703e614245efe24475fdde30fa96918b77a6333d2225b5d53b716ae5690d0172c00d58f77ecbc00e45415e6a085bb2ff9e23140f84328ee478bce31b497273cbcfb14209414aa97310aaf405ca54601486257b6778bbe97b30217dc696fd7758a31bab037fcde338a9200022b0ee476dc63c5ad63e1ee28d766de07b91c7b56d0370ac15f41b66bb1ecad352259a599f2bc34f7f2e6323a71e8bd3f5d023b02d9bbf93b846ee8b88b8dd0e469dd433b33ecf7a9310163ed3d022a39f44c169f8bd70ef064f3484b57e878ca83c3af9a3610ded65102f657359cd3e5ff230ac7936fd6a17c8dc31697206413fd61769ebc28b0e792d171b5e3a9f8006664b2bb401138b77bb3d6722ec38272f590d6a637ee096065026de04ff6038259df9ae4634bce2ae509eb50eac062b9de1ae7ec5107deee3859bbe7d2e75e1d6feb43afaa8dced302071de19dc3eace210ededb89c16336c69341f55d51294621bbc466b3e25c51b2d9fd91924b5ce71488189e27e8d45fc66d1115baa4fcd09028135f31ebc6f052fa568f34d819f5ca37f78109fda8d14a21fa6805ea5fb29aeca4152cab2ec8a1d5e690aad1c290d45d2f66ddcbc2ea0647c63b9e49e4f61bb97f1480a86065b446d8a56da517bf51a4acabb7bbba56a812bc38435b57c038bf98f9e7f1fa39cfd55a962d86b88bfb025d39469df6ac9b7dac04e864dfce03926e86d071dea3a9730af2d2b2f51f7f560a6f48dd810ea63a4919f5e5b2da6c50d21d3a20829ddb52d"); //valid private cert
            Assert.IsNotNull(keySet.PrivateKey);
            Assert.IsNotNull(keySet.PublicKey);
            Assert.IsNotNull(keySet.Address);
        }

        //[Test]
        public void KeySet_ModulusAlwaysTheSameLength()
        {
            //verify if the modulus (which defines the lengths of the signatures) is always the same length
            using (var rsaCsp = new RSACryptoServiceProvider())
            {
                rsaCsp.ImportCspBlob(new KeySet().PrivateKey.HexToBytes());
                var rsaParams = rsaCsp.ExportParameters(true);
                var mod = rsaParams.Modulus.Length;
                for (int i = 0; i < 1000; i++)
                {
                    rsaCsp.ImportCspBlob(new KeySet().PrivateKey.HexToBytes());
                    rsaParams = rsaCsp.ExportParameters(true);
                    var newMod = rsaParams.Modulus.Length;
                    Assert.AreEqual(mod, newMod); //128
                    mod = newMod;
                }
            }
        }

        //[Test]
        public void KeySet_PublicKeyAlwaysTheSameLength()
        {
            using (var rsaCsp = new RSACryptoServiceProvider())
            {
                var pk = new KeySet().PublicKey.HexToBytes().Length;
                for (int i = 0; i < 1000; i++)
                {
                    var newPk = new KeySet().PublicKey.HexToBytes().Length;
                    Assert.AreEqual(pk, newPk); //148
                    pk = newPk;
                }
            }
        }
    }
}
