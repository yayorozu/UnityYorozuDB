// -------------------- //
// Auto Generate Code.  //
// Do not edit!!!       //
// -------------------- //
using UnityEngine;

namespace Yorozu.DB
{
    public class SampleData : DataAbstract, IIntKey
    {
       int IIntKey.Key => Key;
        public int Key => Int(1);

        public string Value => String(2);

        public Sample EnumValue1 => (Sample) Enum(3, 1);

        public Vector3 Vector3Data => Vector3(4);

        public GameObject Obj => GameObject(5);

        public override string ToString()
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine($"Type: {GetType().Name}");
            builder.AppendLine($"Key: {Key.ToString()}");
            builder.AppendLine($"Value: {Value.ToString()}");
            builder.AppendLine($"EnumValue1: {EnumValue1.ToString()}");
            builder.AppendLine($"Vector3Data: {Vector3Data.ToString()}");
            builder.AppendLine($"Obj: {Obj.ToString()}");
            return builder.ToString();
        }
    }
}

