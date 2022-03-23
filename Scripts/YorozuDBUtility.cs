using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yorozu.DB
{
    internal static class YorozuDBUtility
    {
        private static IEnumerable<Type> _types;
        
        /// <summary>
        /// YorozuDBDataObject から中身を抜き出してクラスに変換
        /// 参照を隠蔽してあげればいいんじゃないかな
        /// それでいい感じにできそうな気がする
        /// </summary>
        internal static List<DataAbstract> Convert(YorozuDBDataObject data, Type type)
        {
            var dataCount = data.DataCount;
            // データ数分確保
            var list = new List<DataAbstract>(dataCount);
            for (var row = 0; row < dataCount; row++)
            {
                var instance = Activator.CreateInstance(type) as DataAbstract;
                instance.SetUp(data, row);
                list.Add(instance);
            }

            return list;
        }
        
        /// <summary>
        /// データの型情報を取得
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static Type GetType(YorozuDBDataObject data)
        {
            if (_types == null)
            {
                _types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsSubclassOf(typeof(DataAbstract)));
            }

            // 同じ名前を探す
            return _types.First(t => t.Name == data.Define.ClassName);
        }
    }
}