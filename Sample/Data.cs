using UnityEngine;

namespace Yorozu.DB
{
    public class Data : YorozuDBData
    {
       public string Key => String(0);

       public GameObject Value => GameObject(1);

       public int Sample => Int(2);

       public Vector2 Vector2 => Vector2(3);

       public Vector3 Vector3 => Vector3(4);

       public Sprite aaa => Sprite(5);

       public UnityEngine.Object obj => UnityObject(6);

       public int complex => Int(7);

    }
}

