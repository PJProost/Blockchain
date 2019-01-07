using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Domain
{
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("{Hash ?? ToString()}")]
    public class SimpleTransaction : IEquatable<SimpleTransaction>, ISerializable
    {
        public SimpleTransaction(string toAddress, IEnumerable<SimpleTransaction> inputTransactions, decimal value)
        {
            Initialize(toAddress,
                inputTransactions,
                value,
                DateTime.Now.ToUniversalTime().Ticks);
        }
        public SimpleTransaction(string address, decimal generatedValue)
        {
            Initialize(address,
                null,
                generatedValue,
                DateTime.Now.ToUniversalTime().Ticks);
        }
        private void Initialize(string toAddress, IEnumerable<SimpleTransaction> inputTransactions, decimal value, long timestamp)
        {
            ToAddress = toAddress
                ?? throw new SimpleTransactionException("Transaction toAddress not set");

            InputTransactions = inputTransactions != null
                ? inputTransactions.ToList()
                : new List<SimpleTransaction>(); //generation transactions don't have inputs

            if (InputTransactions.Count > 0 && InputTransactions.Sum(i => i.Value) < value)
                throw new SimpleTransactionException("Transaction value exceeds input transaction value");

            var inputTransactionHashes = InputTransactions.Select(t => t.Hash);

            InputTransactionHashes = inputTransactionHashes.ToList();

            Value = value; //purposely allowing transactions with 0 value

            TimeStamp = timestamp != 0
                ? timestamp
                : throw new SimpleTransactionException("Transaction timestamp not set");

            Hash = this.Serialize().Hash().ToHex();

            OutputTransactions = new Dictionary<string, decimal>();
        }

        public void Spend(SimpleTransaction OutputTransaction)
        {
            if (OutputTransactions.Values.Sum() >= Value)
                throw new SimpleTransactionException($"Transaction hash {Hash} cannot be assigned a new output, as it was already fully spent");
            if (OutputTransaction == null)
                throw new SimpleTransactionException("Invalid output transaction");
            OutputTransactions.Add(OutputTransaction.Hash, OutputTransaction.Value);
        }

        public byte[] Serialize(List<KeySet> senderKeys)
        {
            //I don't want to make each transaction contain all previous transactions, because that will make the serialized transactions grow bigger and bigger over time
            //I only want to include the input transaction hashes, so the transaction size stays more or less constant
            //so the object can only contain the hashes, but the Serialize method will need the addresses of the input transactions as well
            //because it will need to sign each input hash with the corresponding private key, which can be found through the address of the keyset

            //TXHASH_HASHBASE****************************************
            //      _UINT32INPUTCOUNT_SIGNEDINPUTSWITHKEY_TRANSACTION
            //                       _HASH_SIGNATURE_KEY*_

            if (InputTransactions == null)
                throw new SimpleTransactionException("Input transactions not set, can't serialize");

            byte[] serializedTransaction = null;

            var transactionHash = new byte[32];
            byte[] hashbase = null;

            var inputCount = new byte[4];
            var inputHash = new byte[32];
            var signature = new byte[128];
            var publicKey = new byte[148];
            byte[] transaction = null;

            //UINT32INPUTCOUNT
            inputCount = BitConverter.GetBytes((uint)InputTransactionHashes.Count);
            hashbase = hashbase.Append(inputCount);

            //SIGNEDINPUTSWITHKEY
            foreach (var hash in InputTransactionHashes)
            {
                //verify parameters
                var inputTransaction = InputTransactions.FindAll(t => t.Hash == hash);
                if (inputTransaction.Count < 1) throw new SimpleTransactionException($"Input transaction hash {hash} not found in provided inputTransactions");
                var keys = senderKeys.FindAll(k => k.Address == inputTransaction[0].ToAddress);
                if (keys.Count < 1) throw new SimpleTransactionException($"Private key for input transaction hash {hash} (address {keys[0].Address}) not found in provided senderKeys");

                //sign each input
                using (var rsaCsp = new RSACryptoServiceProvider())
                {
                    rsaCsp.ImportCspBlob(keys[0].PrivateKey.HexToBytes());
                    signature = rsaCsp.SignData(hash.HexToBytes(), "SHA256");
                    if (!rsaCsp.VerifyData(hash.HexToBytes(), "SHA256", signature))
                        throw new SimpleTransactionException($"Signing of input transaction hash {hash} with private key for address {keys[0].Address} failed");
                }

                //serialize
                var signedInput = new byte[inputHash.Length + signature.Length + publicKey.Length];
                inputHash = hash.HexToBytes();
                publicKey = keys[0].PublicKey.HexToBytes();
                hashbase = hashbase.Append(inputHash).Append(signature).Append(publicKey);
            }

            //TRANSACTION
            transaction = this.Serialize();
            hashbase = hashbase.Append(transaction);

            //TXHASH
            transactionHash = Hash.HexToBytes();//hashbase.Hash();
            //Hash = transactionHash.ToHex();

            serializedTransaction = transactionHash.Append(hashbase);

            return serializedTransaction;
        }
        public static SimpleTransaction Deserialize(byte[] serializedTransaction)
        {
            //TXHASH_HASHBASE****************************************
            //      _UINT32INPUTCOUNT_SIGNEDINPUTSWITHKEY_TRANSACTION
            //                       _HASH_SIGNATURE_KEY*_

            var transactionHash = new byte[32];
            byte[] hashbase = null;

            var inputCount = new byte[4];
            var inputHash = new byte[32];
            var signature = new byte[128];
            var publicKey = new byte[148];
            byte[] transaction = null;

            var inputHashes = new List<byte[]>();

            //TXHASH
            transactionHash = serializedTransaction.Take(transactionHash.Length).ToArray();

            //HASHBASE
            hashbase = hashbase.Append(serializedTransaction.Skip(transactionHash.Length).Take(serializedTransaction.Length - transactionHash.Length).ToArray());

            //UINT32INPUTCOUNT
            inputCount = hashbase.Take(inputCount.Length).ToArray();
            var numberOfInputs = BitConverter.ToUInt32(inputCount, 0);

            //SIGNEDINPUTSWITHKEY
            var signedInput = new byte[inputHash.Length + signature.Length + publicKey.Length];
            for (int i = 0; i < numberOfInputs; i++)
            {
                //deserialize
                signedInput = hashbase.Skip(inputCount.Length + i * signedInput.Length).Take(signedInput.Length).ToArray();
                inputHash = signedInput.Take(inputHash.Length).ToArray();
                signature = signedInput.Skip(inputHash.Length).Take(signature.Length).ToArray();
                publicKey = signedInput.Skip(inputHash.Length + signature.Length).ToArray();
                inputHashes.Add(inputHash);

                //validate each input signature
                using (var rsaCsp = new RSACryptoServiceProvider())
                {
                    rsaCsp.ImportCspBlob(publicKey);
                    if (!rsaCsp.VerifyData(inputHash, "SHA256", signature))
                        throw new SimpleTransactionException($"Signature of input transaction hash {inputHash} with public key {publicKey.ToHex()} (address {publicKey.Hash().ToHex()}) failed");
                }
            }

            //TRANSACTION
            transaction = hashbase.Skip(inputCount.Length + (inputHash.Length + signature.Length + publicKey.Length) * (int)numberOfInputs).ToArray();
            var result = transaction.DeserializeTo<SimpleTransaction>();
            result.Hash = transactionHash.ToHex();//verifyHash.ToHex(); //strange that this is possible with a private setter?

            //verify hash
            var verifyHash = result.Serialize().Hash();//hashbase.Hash();
            if (!transactionHash.SequenceEqual(verifyHash))
                //just comparing the arrays using != doesn't work
                throw new SimpleTransactionException($"Transaction hash {transactionHash.ToHex()} doesn't match hashbase hash {verifyHash.ToHex()} (transaction corrupted)");

            //check if each input of the transaction has been validated as spendable by this transaction
            //this has to be a separate loop, if this would be in the SIGNEDINPUTSWITHKEY loop we would check the opposite
            //(if each signed hash had an input in the transaction)
            foreach (var hash in result.InputTransactionHashes)
            {
                if (inputHashes.FindAll(h => h.ToHex() == hash).Count < 1) throw new SimpleTransactionException("Input transaction hash {hash} not found in signed input transactions");
            }

            return result;
        }

        public SimpleTransaction ChangeTransaction()
        {
            //generate change tx if needed
            if (InputTransactions.Sum(i => i.Value) > Value)
            {
                return new SimpleTransaction(InputTransactions[0].ToAddress, InputTransactions, InputTransactions.Sum(i => i.Value) - Value);
            }
            else return null;
        }

        public SimpleTransaction(SerializationInfo info, StreamingContext context)
        {
            ToAddress = info.GetString("a");
            InputTransactionHashes = (List<string>)info.GetValue("i", typeof(List<string>));
            Value = info.GetDecimal("v");
            TimeStamp = info.GetInt64("t");
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("a", ToAddress);
            info.AddValue("i", InputTransactionHashes);
            info.AddValue("v", Value);
            info.AddValue("t", TimeStamp);
        }

        public bool Equals(SimpleTransaction otherTx)
        {
            return this.GetHashCode() == otherTx.GetHashCode();
        }
        public override int GetHashCode()
        {
            //only for internal/.NET use, the actual transaction hash is SHA256 but we need a numeric hash for GetHashCode
            //.NET GetHashCode() is not device-agnostic so overriding with a custom hash function
            return (int)FNVHash.FNV1aIn32bit(this.Serialize());
        }

        public string ToAddress { get; private set; }
        public List<string> InputTransactionHashes { get; private set; }
        public List<SimpleTransaction> InputTransactions { get; private set; }
        public decimal Value { get; private set; }
        public long TimeStamp { get; private set; }

        public string Hash { get; private set; }
        public Dictionary<string, decimal> OutputTransactions { get; private set; }
    }

    public class SimpleTransactionException : InvalidOperationException
    {
        public SimpleTransactionException(string message) : base(message)
        {
        }
        public SimpleTransactionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
