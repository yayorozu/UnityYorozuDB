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

        public string Name => String(2);

        public Yorozu.DB.Fruits Fruits => (Yorozu.DB.Fruits) Enum(6, 2);

        public override string ToString()
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine($"Type: {GetType().Name}");
            builder.AppendLine($"Key: {Key.ToString()}");
            builder.AppendLine($"Name: {Name.ToString()}");
            builder.AppendLine($"Fruits: {Fruits.ToString()}");
            return builder.ToString();
        }
    }
}

