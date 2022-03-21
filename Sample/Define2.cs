using UnityEngine;

namespace Yorozu.DB
{
    public class Define2 : DataAbstract, IIntKey
    {
       int IIntKey.Key => Key2;
       public int Key2 => Int(1);

    }
}

