// <auto-generated>
// This code was auto-generated by a tool, every time
// the tool executes this code will be reset.
//
// If you need to extend the classes generated to add
// fields or methods to them, please create partial
// declarations in another file.
// </auto-generated>
#pragma warning disable 0109
#pragma warning disable 1591


namespace Quantum {
  using UnityEngine;
  
  [UnityEngine.DisallowMultipleComponent()]
  public unsafe partial class QPrototypeKCC : QuantumUnityComponentPrototype<Quantum.Prototypes.KCCPrototype>, IQuantumUnityPrototypeWrapperForComponent<Quantum.KCC> {
    partial void CreatePrototypeUser(Quantum.QuantumEntityPrototypeConverter converter, ref Quantum.Prototypes.KCCPrototype prototype);
    [DrawInline()]
    [ReadOnly(InEditMode = false)]
    public Quantum.Prototypes.Unity.KCCPrototype Prototype;
    public override System.Type ComponentType {
      get {
        return typeof(Quantum.KCC);
      }
    }
    public override ComponentPrototype CreatePrototype(Quantum.QuantumEntityPrototypeConverter converter) {
      Quantum.Prototypes.KCCPrototype result;
      converter.Convert(Prototype, out result);
      CreatePrototypeUser(converter, ref result);
      return result;
    }
  }
}
#pragma warning restore 0109
#pragma warning restore 1591
