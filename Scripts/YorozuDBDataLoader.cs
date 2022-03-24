using System;
using UnityEngine;

namespace Yorozu.DB
{
    [Serializable]
    public class YorozuDBDataLoader
    {
        [SerializeField]
        private YorozuDBEnumDataObject _enum;
        [SerializeField]
        private YorozuDBDataObject[] _data;

        public void Load()
        {
            YorozuDB.SetEnum(_enum);
            YorozuDB.SetData(_data);
        }
    }
}