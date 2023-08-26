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
            // if (ExecutionEngine.CallingScriptHash != Oracle.Hash) throw new Exception("Unauthorized!");
            if (code != OracleResponseCode.Success) throw new Exception("Oracle response failure with code " + (byte)code);

            object ret = StdLib.JsonDeserialize(result); // [ "hello world" ]
            object[] arr = (object[])ret;
            string value = (string)arr[0];

            Runtime.Log("userdata: " + userdata);
            Runtime.Log("response value: " + value);
        }


        //     function addLongLiquidity(user, amount):
        // L_long = L_long + amount
        // P_mark = calculateMarkPrice()
        // user_value[user] = amount * P_mark  // Store the value of the user's liquidity based on the current mark price.


        //     function addShortLiquidity(user, amount):
        // L_short = L_short + amount
        // P_mark = calculateMarkPrice()
        // user_value[user] = amount * P_mark  // Store the value of the user's liquidity based on the current mark price.

        //     function removeLongLiquidity(user, amount):
        // current_value = user_value[user] / P_mark  // Calculate the current liquidity of the user based on the mark price.
        // if amount <= current_value:
        //     L_long = L_long - amount
        //     P_mark = calculateMarkPrice()
        //     user_value[user] = user_value[user] - amount * P_mark  // Deduct the removed liquidity from the user's value.
        // else:
        //     raise Exception("Cannot remove more than available long liquidity based on the current mark price.")


// function removeShortLiquidity(user, amount):
//     current_value = user_value[user] / P_mark  // Calculate the current liquidity of the user based on the mark price.
//     if amount <= current_value:
//         L_short = L_short - amount
//         P_mark = calculateMarkPrice()
//         user_value[user] = user_value[user] - amount * P_mark  // Deduct the removed liquidity from the user's value.
//     else:
//         raise Exception("Cannot remove more than available short liquidity based on the current mark price.")

        
        public static void AddLongLiquidity(BigInteger amount)
        {
            var l_long = (BigInteger)Storage.Get(Storage.CurrentContext, "l_long");
            l_long += amount;
            Storage.Put(Storage.CurrentContext, "l_long", l_long);

            var mark = CalculateMarkPrice();

            var currentPosition = (BigInteger)Storage.Get(Storage.CurrentContext, Tx.Sender + "val");
            currentPosition += amount * mark;
            Storage.Put(Storage.CurrentContext, Tx.Sender + "val", currentPosition);
            // contractStorage.Put(Tx.Sender, amount);
        }

        public static void AddShortLiquidity(BigInteger amount)
        {

            var l_short = (BigInteger)Storage.Get(Storage.CurrentContext, "l_short");
            l_short += amount;
            Storage.Put(Storage.CurrentContext, "l_short", l_short);

            var mark = CalculateMarkPrice();

            var currentPosition = (BigInteger)Storage.Get(Storage.CurrentContext, Tx.Sender + "val");
            currentPosition += amount * mark;
            Storage.Put(Storage.CurrentContext, Tx.Sender + "val", currentPosition);
        }

        public static void RemoveLongLiquidity(BigInteger amount){
            var mark = CalculateMarkPrice();
            var currentPosition = (BigInteger)Storage.Get(Storage.CurrentContext, Tx.Sender + "val");
            var currentValue = currentPosition / mark;

            if(amount <= currentValue)
            {
                var l_long = (BigInteger)Storage.Get(Storage.CurrentContext, "l_long");
                l_long -= amount;
                Storage.Put(Storage.CurrentContext, "l_long", l_long);

                currentPosition -= amount * mark;
                Storage.Put(Storage.CurrentContext, Tx.Sender + "val", currentPosition);
            } else {
                throw new Exception("Cannot remove more than available long liquidity based on the current mark price.");
            }
        }

        public static void RemoveShortLiquidity(BigInteger amount){
            var mark = CalculateMarkPrice();
            var currentPosition = (BigInteger)Storage.Get(Storage.CurrentContext, Tx.Sender + "val");
            var currentValue = currentPosition / mark;

            if(amount <= currentValue)
            {
                var l_short = (BigInteger)Storage.Get(Storage.CurrentContext, "l_short");
                l_short -= amount;
                Storage.Put(Storage.CurrentContext, "l_short", l_short);

                currentPosition -= amount * mark;
                Storage.Put(Storage.CurrentContext, Tx.Sender + "val", currentPosition);
            } else {
                throw new Exception("Cannot remove more than available short liquidity based on the current mark price.");
            }

        }

        public BigInteger CurrentMarkPrice()
        {
            return CalculateMarkPrice();
        }

        public BigInteger CurrentIndex()
        {
            return index;
        }

        public static BigInteger CalculateMarkPrice()
        {
            var l_long = (BigInteger)Storage.Get(Storage.CurrentContext, "l_long");
            var l_short = (BigInteger)Storage.Get(Storage.CurrentContext, "l_short");

            if(l_long + l_short == 0){
                return index;
            } else {
    
                var mark = index + (((l_long - l_short) * 10) / ((l_long + l_short) * 100));
                return mark;
            }
        }
        static BigInteger index = 2000;
    }
}
