// -------------------- //
// Auto Generate Code.  //
// Do not edit!!!       //
// -------------------- //
using UnityEngine;

namespace Yorozu.DB
{
    public partial class Extend : DataAbstract, IIntKey
    {
        int IIntKey.Key => (int)Id;

        public int Id => Int(1);

        // Extend Fields
        public int IntArray => Extend<ExtendScriptableObject>().IntArray[row];

        public string StringList => Extend<ExtendScriptableObject>().StringList[row];

        public ExtendScriptableObject.ClassSample ClassList => Extend<ExtendScriptableObject>().ClassList[row];

        public override string ToString()
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine($"Type: {GetType().Name}");
            builder.AppendLine($"Id: {Id.ToString()}");
            builder.AppendLine($"IntArray: {IntArray.ToString()}");
            builder.AppendLine($"StringList: {StringList.ToString()}");
            builder.AppendLine($"ClassList: {ClassList.ToString()}");
            return builder.ToString();
        }
    }
}

