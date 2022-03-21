using System;
using System.Collections.Generic;

namespace Yorozu.DB
{
    internal class YorozuDBUtility
    {
        /// <summary>
        /// YorozuDBDataObject から中身を抜き出してクラスに変換
        /// 参照を隠蔽してあげればいいんじゃないかな
        /// それでいい感じにできそうな気がする
        /// </summary>
        internal static List<YorozuDBDataAbstract> Convert<T>(YorozuDBDataObject data, YorozuDBEnumDataObject enumData) where T : YorozuDBDataAbstract
        {
            var dataCount = data.DataCount;
            // データ数分確保
            var list = new List<YorozuDBDataAbstract>();
            var type = typeof(T);
            
            for (var row = 0; row < dataCount; row++)
            {
                var instance = Activator.CreateInstance(type) as YorozuDBDataAbstract;
                instance.SetUp(data, row, enumData);
                list.Add(instance as T);
            }

            return list;
        }
    }
}