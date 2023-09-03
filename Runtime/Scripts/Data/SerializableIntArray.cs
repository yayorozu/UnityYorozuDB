using System;
using UnityEngine;

namespace Yorozu.DB
{
    [Serializable]
    internal class SerializableIntArray
    {
        [SerializeField]
        internal int[] IntArray;

        internal SerializableIntArray(int size = 2)
        {
            IntArray = new int[size];
            for (var i = 0; i < size; i++)
            {
                IntArray[i] = 0;
            }
        }
    }
}