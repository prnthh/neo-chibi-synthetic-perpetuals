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
    [System.ComponentModel.Description("YourName.NeoChibiPerpetualsContract")]
    interface NeoChibiPerpetualsContract {
        bool changeNumber(System.Numerics.BigInteger positiveNumber);
        byte[] getNumber();
        void update(byte[] nefFile, string manifest);
        interface Events {
            void NumberChanged(Neo.UInt160 arg1, System.Numerics.BigInteger arg2);
        }
    }
}
