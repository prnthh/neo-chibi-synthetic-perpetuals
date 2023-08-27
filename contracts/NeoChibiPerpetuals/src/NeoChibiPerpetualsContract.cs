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

        public static void RequestEthereumPrice()
        {
            // Replace this with the appropriate API endpoint for Ethereum price.
            string url = "https://min-api.cryptocompare.com/data/price?fsym=ETH&tsyms=USD";

            string filter = "$.USD"; // Assuming the API returns price in a field called "price_usd".
            string callback = "HandleEthereumPrice"; // Rename callback method to better reflect its purpose.
            object userdata = "ethPriceRequest"; // Updated to indicate the purpose of the request.
            long gasForResponse = Oracle.MinimumResponseFee;

            Oracle.Request(url, filter, callback, userdata, gasForResponse);
        }

        public static void HandleEthereumPrice(string url, string userdata, OracleResponseCode code, string result)
        {
            // if (ExecutionEngine.CallingScriptHash != Oracle.Hash) throw new Exception("Unauthorized!");
            if (code != OracleResponseCode.Success) throw new Exception("Oracle response failure with code " + (byte)code);

            object ret = StdLib.JsonDeserialize(result); 
            object[] arr = (object[])ret;
            BigInteger ethPrice = (BigInteger)arr[0];


            // Store the Ethereum price for use in your vAMM logic.
            Storage.Put(Storage.CurrentContext, "ethPrice", ethPrice);

            Runtime.Log("userdata: " + userdata);
            Runtime.Log("Ethereum price: " + ethPrice);
        }
        
        public static void AddLongLiquidity(BigInteger amount)
        {

            // if (Runtime.CallingScriptHash != GAS.Hash)
            // throw new Exception("Please pay with GAS");
        
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

        public static BigInteger CurrentMarkPrice()
        {
            return CalculateMarkPrice();
        }

        public static BigInteger CurrentIndex()
        {
            return (BigInteger)Storage.Get(Storage.CurrentContext, "ethPrice");
        }

        public static BigInteger CalculateMarkPrice()
        {
            RequestEthereumPrice();

            var l_long = (BigInteger)Storage.Get(Storage.CurrentContext, "l_long");
            var l_short = (BigInteger)Storage.Get(Storage.CurrentContext, "l_short");

            if(l_long + l_short == 0){
                return CurrentIndex();
            } else {
                var mark = CurrentIndex() + (sensitivity * (l_long - l_short) / ((l_long + l_short)));
                return mark;
            }
        }
        static int sensitivity = 1000;
    }
}
