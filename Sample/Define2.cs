using UnityEngine;

namespace Yorozu.DB
{
    public class Define2 : DataAbstract
    {
       public int Key2 => Int(1);

       public string sae => String(2);

       public Sample Kkkkk => (Sample) Enum(3, 1);

    }
}

