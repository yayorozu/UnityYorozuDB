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
            if (_enum != null)
            {
                YorozuDB.SetEnum(_enum);
            }
            else
            {
                var enumData = Resources.LoadAll<YorozuDBEnumDataObject>("YorozuDB/Define/");
                if (enumData != null && enumData.Length > 0)
                {
                    YorozuDB.SetEnum(enumData[0]);
                }
            }

            YorozuDB.SetData(_data);
            
            // 指定のフォルダよりロード
            var resourcesData = Resources.LoadAll<YorozuDBDataObject>("YorozuDB/Data/");
            if (resourcesData != null && resourcesData.Length > 0)
            {
                YorozuDB.SetData(resourcesData);
            }
        }
    }
}