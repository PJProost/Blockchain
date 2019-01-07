using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;


namespace Domain
{
    public class KeySet
    {
        public KeySet()
        {
            //generate new keys
            using (var rsaCsp = new RSACryptoServiceProvider())
            {
                PrivateKey = rsaCsp.ExportCspBlob(true).ToHex();
                PublicKey = rsaCsp.ExportCspBlob(false).ToHex();
                Address = rsaCsp.ExportCspBlob(false).Hash().ToHex();
            }
        }
        public KeySet(string privateKey)
        {
            //use provided key
            using (var rsaCsp = new RSACryptoServiceProvider())
            {
                rsaCsp.ImportCspBlob(privateKey.HexToBytes());
                if (rsaCsp.PublicOnly) throw new ArgumentException("Invalid private key (only public key)");
                PrivateKey = privateKey;
                PublicKey = rsaCsp.ExportCspBlob(false).ToHex();
                Address = rsaCsp.ExportCspBlob(false).Hash().ToHex();
            }
        }

        public string PrivateKey { get; private set; }
        public string PublicKey { get; set; }
        public string Address { get; private set; }
    }
}
