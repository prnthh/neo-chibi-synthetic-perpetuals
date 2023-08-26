using System;
using System.ComponentModel;
using System.Numerics;

using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace NeoChibiPerpetuals
{
    [DisplayName("Milady.NeoChibiPerpetualsContract")]
    [ManifestExtra("Author", "Milady")]
    [ManifestExtra("Email", "milady@remilia.co")]
    [ManifestExtra("Description", "Neo chibi perpetuals.")]
    public class NeoChibiPerpetualsContract : SmartContract
    {
        const byte Prefix_NumberStorage = 0x00;
        const byte Prefix_ContractOwner = 0xFF;
        private static Transaction Tx => (Transaction) Runtime.ScriptContainer;

        [DisplayName("NumberChanged")]
        public static event Action<UInt160, BigInteger> OnNumberChanged;

        public static bool ChangeNumber(BigInteger positiveNumber)
        {
            if (positiveNumber < 0)
            {
                throw new Exception("Only positive numbers are allowed.");
            }

            StorageMap contractStorage = new(Storage.CurrentContext, Prefix_NumberStorage);
            contractStorage.Put(Tx.Sender, positiveNumber);
            OnNumberChanged(Tx.Sender, positiveNumber);
            return true;
        }

        public static ByteString GetNumber()
        {
            StorageMap contractStorage = new(Storage.CurrentContext, Prefix_NumberStorage);
            return contractStorage.Get(Tx.Sender);
        }

        [DisplayName("_deploy")]
        public static void Deploy(object data, bool update)
        {
            if (update) return;

            var key = new byte[] { Prefix_ContractOwner };
            Storage.Put(Storage.CurrentContext, key, Tx.Sender);
        }
        
        public static void Update(ByteString nefFile, string manifest)
        {
            var key = new byte[] { Prefix_ContractOwner };
            var contractOwner = (UInt160)Storage.Get(Storage.CurrentContext, key);

            if (!contractOwner.Equals(Tx.Sender))
            {
                throw new Exception("Only the contract owner can update the contract");
            }

            ContractManagement.Update(nefFile, manifest, null);
        }

        public static void DoRequest()
        {
            string url = "https://raw.githubusercontent.com/neo-project/examples/master/csharp/Oracle/example.json"; // the content is  { "value": "hello world" }
            string filter = "$.value";  // JSONPath format https://github.com/atifaziz/JSONPath
            string callback = "callback"; // callback method
            object userdata = "userdata"; // arbitrary type
            long gasForResponse = Oracle.MinimumResponseFee;

            Oracle.Request(url, filter, callback, userdata, gasForResponse);
        }

        public static void Callback(string url, string userdata, OracleResponseCode code, string result)
        {
            if (ExecutionEngine.CallingScriptHash != Oracle.Hash) throw new Exception("Unauthorized!");
            if (code != OracleResponseCode.Success) throw new Exception("Oracle response failure with code " + (byte)code);

            object ret = StdLib.JsonDeserialize(result); // [ "hello world" ]
            object[] arr = (object[])ret;
            string value = (string)arr[0];

            Runtime.Log("userdata: " + userdata);
            Runtime.Log("response value: " + value);
        }
    }
}
