using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yorozu.DB
{
    public static class YorozuDB
    {
        internal static YorozuDBEnumDataObject EnumData => _enumData; 
        private static YorozuDBEnumDataObject _enumData;
        private static Dictionary<Type, List<DataAbstract>> _data = new Dictionary<Type, List<DataAbstract>>();
        
        /// <summary>
        /// Enum の定義データを登録
        /// </summary>
        public static void SetEnum(YorozuDBEnumDataObject enumData)
        {
            _enumData = enumData;
        }

        /// <summary>
        /// 指定したデータをキャッシュ
        /// </summary>
        public static void SetData(params YorozuDBDataObject[] data)
        {
            foreach (var d in data)
            {
                var type = YorozuDBUtility.GetType(d);
                if (!_data.ContainsKey(type))
                {
                    _data.Add(type, new List<DataAbstract>());
                }

                var array = YorozuDBUtility.Convert(d, type);
                _data[type].AddRange(array);
            }
        }

        /// <summary>
        /// TODO: 指定したデータをキャッシュから削除
        /// </summary>
        public static void RemoveData(params YorozuDBDataObject[] data)
        {
        }

        private static bool ValidData(Type type, Type interfaceType)
        {
            if (!_data.TryGetValue(type, out var list))
                return false;
            
            if (list.Count <= 0)
                return false;

            // interface が定義されていない
            return type.GetInterfaces().Contains(interfaceType);
        }

        private static IEnumerable<T> FindMany<T>(string key, int limit) where T : DataAbstract
        {
            var type = typeof(T);
            if (!ValidData(typeof(T), typeof(IStringKey)))
                return new List<T>();

            return _data[type]
                .Where(v =>
                {
                    var ikey = v as IStringKey;
                    return ikey.Key == key;
                })
                .Take(limit)
                .Cast<T>()
            ;
        }

        private static IEnumerable<T> FindMany<T>(int key, int limit) where T : DataAbstract
        {
            var type = typeof(T);
            if (!ValidData(typeof(T), typeof(IIntKey)))
                return new List<T>();
            
            return _data[type]
                .Where(v =>
                {
                    var ikey = v as IIntKey;
                    return ikey.Key == key;
                })
                .Take(limit)
                .Cast<T>()
            ;
        }

        public static T Find<T>(int key) where T : DataAbstract
        {
            var finds = FindMany<T>(key, 1);
            if (!finds.Any())
                return null;
            
            return finds.First();
        }
        
        public static T Find<T>(string key) where T : DataAbstract
        {
            var finds = FindMany<T>(key, 1);
            if (!finds.Any())
                return null;
            
            return finds.First();
        }
        
        public static T Find<T>(Enum key) where T : DataAbstract
        {
            return Find<T>(key.GetHashCode());
        }
        
        public static IEnumerable<T> FindMany<T>(int key) where T : DataAbstract
        {
            return FindMany<T>(key, int.MaxValue);
        }
        
        public static IEnumerable<T> FindMany<T>(string key) where T : DataAbstract
        {
            return FindMany<T>(key, int.MaxValue);
        }
        
        public static IEnumerable<T> FindMany<T>(Enum key) where T : DataAbstract
        {
            return FindMany<T>(key.GetHashCode(), int.MaxValue);
        }

        /// <summary>
        /// 全データを取得
        /// </summary>
        public static IEnumerable<T> All<T>() where T : DataAbstract
        {
            var type = typeof(T);
            if (!_data.TryGetValue(type, out var list))
                return new List<T>();
            
            if (list.Count <= 0)
                return new List<T>();

            return _data[type].Cast<T>();
        }
    }
}