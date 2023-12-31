//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NeoChibiPerpetualsTests {
    #if NETSTANDARD || NETFRAMEWORK || NETCOREAPP
    [System.CodeDom.Compiler.GeneratedCode("Neo.BuildTasks","3.5.17.56371")]
    #endif
    [System.ComponentModel.Description("Milady.NeoChibiPerpetualsContract")]
    interface NeoChibiPerpetualsContract {
        void update(byte[] nefFile, string manifest);
        void requestEthereumPrice();
        void handleEthereumPrice(string url, string userdata, System.Numerics.BigInteger code, string result);
        void addLongLiquidity(System.Numerics.BigInteger amount);
        void addShortLiquidity(System.Numerics.BigInteger amount);
        void removeLongLiquidity(System.Numerics.BigInteger amount);
        void removeShortLiquidity(System.Numerics.BigInteger amount);
        System.Numerics.BigInteger currentMarkPrice();
        System.Numerics.BigInteger currentIndex();
        System.Numerics.BigInteger calculateMarkPrice();
        interface Events {
            void NumberChanged(Neo.UInt160 arg1, System.Numerics.BigInteger arg2);
        }
    }
}
