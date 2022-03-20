using System;
using System.Collections.Generic;
using System.Linq;

namespace Yorozu.DB
{
    internal class YorozuDBUtility
    {
        /// <summary>
        /// YorozuDBDataObject から中身を抜き出してクラスに変換
        /// 参照を隠蔽してあげればいいんじゃないかな
        /// それでいい感じにできそうな気がする
        /// </summary>
        internal static List<YorozuDBData> Convert<T>(YorozuDBDataObject src) where T : YorozuDBData
        {
            var list = new List<YorozuDBData>(src.Groups.Count());
            var type = typeof(T);
            if (src.Groups.Count > 0)
            {
                var count = src.Groups[0].Data.Count;
                // 要素分データを生成 
                for (var i = 0; i < count; i++)
                {
                    var instance = Activator.CreateInstance(type) as YorozuDBData;
                    instance.SetUp(src, i);
                    list.Add(instance as T);
                }
            }

            return list;
        }
    }
}