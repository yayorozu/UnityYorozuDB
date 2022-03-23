// -------------------- //
// Auto Generate Code.  //
// Do not edit!!!       //
// -------------------- //
using UnityEngine;

namespace Yorozu.DB
{
    public class SampleData : DataAbstract, IIntKey
    {
        int IIntKey.Key => (int)Key;

        public int Key => Int(1);

        public string Value => String(2);

        public Yorozu.DB.Sample EnumKey => (Yorozu.DB.Sample) Enum(6, 1);

        public Vector3 Vector3Data => Vector3(4);

        public override string ToString()
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine($"Type: {GetType().Name}");
            builder.AppendLine($"Key: {Key.ToString()}");
            builder.AppendLine($"Value: {Value.ToString()}");
            builder.AppendLine($"EnumKey: {EnumKey.ToString()}");
            builder.AppendLine($"Vector3Data: {Vector3Data.ToString()}");
            return builder.ToString();
        }
    }
}

