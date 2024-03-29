#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Yorozu.DB
{
    internal static class YorozuDBScriptGenerator
    {
        /// <summary>
        /// データからファイルを出力
        /// </summary>
        internal static void GenerateScript()
        {
            var savePath = "";
            var guids = AssetDatabase.FindAssets($"t:{nameof(YorozuDBScriptExportMarker)}");
            // 見つからなかったら作成
            if (guids.Length <= 0)
            {
                var path = EditorUtility.OpenFolderPanel("Select Export Folder", Application.dataPath, "");
                if (string.IsNullOrEmpty(path))
                    return;

                // UnityPathじゃない
                if (!path.Contains(Application.dataPath))
                    return;

                savePath = path.Replace(Application.dataPath, "Assets");
                var marker = ScriptableObject.CreateInstance<YorozuDBScriptExportMarker>();
                var tagSavePath = Path.Combine(savePath, "Marker.asset");
                AssetDatabase.CreateAsset(marker, tagSavePath);
            }
            else
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                savePath = Path.GetDirectoryName(path);
            }

            var enumData = YorozuDBEditorInternalUtility.LoadEnumDataAsset();

            CreateDefineScript(savePath, enumData);

            // Export Enum files
            CreateEnumScript(enumData, savePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// データのクラスを作成
        /// </summary>
        private static void CreateDefineScript(string savePath, YorozuDBEnumDataObject enumData)
        {
            var assets = YorozuDBEditorInternalUtility.LoadAllDefineAsset();
            var definePath = Path.Combine(savePath, "Define");
            // ディレクトリ作成
            if (!AssetDatabase.IsValidFolder(definePath))
            {
                AssetDatabase.CreateFolder(savePath, "Define");
            }

            // いじれるクラス
            var customPath = Path.Combine(savePath, "Custom");
            // ディレクトリ作成
            if (!AssetDatabase.IsValidFolder(customPath))
            {
                AssetDatabase.CreateFolder(savePath, "Custom");
            }

            foreach (var data in assets)
            {
                var exportPath = Path.Combine(definePath, $"{data.ClassName}.cs");
                // 出力
                using (StreamWriter writer = new StreamWriter(exportPath, false))
                {
                    writer.WriteLine(DataScriptString(data, enumData));
                }

                var customExportPath = Path.Combine(customPath, $"{data.ClassName}.cs");
                if (System.IO.File.Exists(customExportPath))
                    continue;

                using (StreamWriter writer = new StreamWriter(customExportPath, false))
                {
                    writer.WriteLine(CustomDataScriptString(data));
                }
            }
        }

        /// <summary>
        /// 出力用の文字列を取得
        /// </summary>
        private static string DataScriptString(YorozuDBDataDefineObject data, YorozuDBEnumDataObject enumData)
        {
            var builder = new StringBuilder();
            builder.AppendLine("// -------------------- //");
            builder.AppendLine("// Auto Generate Code.  //");
            builder.AppendLine("// Do not edit!!!       //");
            builder.AppendLine("// -------------------- //");
            builder.AppendLine("#pragma warning disable");
            builder.AppendLine("");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine("using System.Linq;");
            builder.AppendLine("");

            builder.AppendLine("namespace Yorozu.DB");
            builder.AppendLine("{");
            builder.Append($"    public partial class {data.ClassName} : {nameof(DataAbstract)}");
            var keyField = data.KeyField;
            if (keyField != null)
            {
                builder.Append($", {GetInterfaceName(keyField.DataType)}");
            }

            builder.AppendLine("");

            builder.AppendLine("    {");

            if (keyField != null)
            {
                builder.AppendLine(
                    $"        {keyField.DataType.ConvertString()} {GetInterfaceName(keyField.DataType)}.Key => ({keyField.DataType.ConvertString()}){keyField.Name};");
                builder.AppendLine("");
            }

            foreach (var field in data.Fields)
            {
                if (!string.IsNullOrEmpty(field.Memo))
                {
                    builder.AppendLine("        /// <summary>");
                    builder.AppendLine($"        /// {field.Memo}");
                    builder.AppendLine("        /// </summary>");
                }

                if (field.DataType == DataType.Enum)
                {
                    var enumDefine = enumData.Defines.FirstOrDefault(d => d.ID == field.EnumDefineId);
                    if (enumDefine != null)
                    {
                        if (field.IsArray)
                        {
                            builder.AppendLine(
                                $"        public IEnumerable<Yorozu.DB.{enumDefine.Name}> {field.Name} => {field.DataType.ToString()}s({field.ID}, {field.EnumDefineId}).Select(v => (Yorozu.DB.{enumDefine.Name})v);");
                            builder.AppendLine($"#if UNITY_EDITOR");
                            builder.AppendLine(
                                $"        public void Add{field.Name}(Yorozu.DB.{enumDefine.Name} value) => Add({field.ID}, {field.EnumDefineId}, value);");
                            builder.AppendLine($"#endif");
                        }
                        else
                        {
                            builder.AppendLine($"        public Yorozu.DB.{enumDefine.Name} {field.Name}");
                            builder.AppendLine($"        {{");
                            builder.AppendLine(
                                $"            get {{ return (Yorozu.DB.{enumDefine.Name}) {field.DataType.ToString()}({field.ID}, {field.EnumDefineId}); }}");
                            builder.AppendLine($"#if UNITY_EDITOR");
                            builder.AppendLine($"            set {{ Set({field.ID}, {field.EnumDefineId}, value); }}");
                            builder.AppendLine($"#endif");
                            builder.AppendLine($"        }}");
                        }
                    }
                }
                else if (field.DataType == DataType.Flags)
                {
                    var enumDefine = enumData.Defines.FirstOrDefault(d => d.ID == field.EnumDefineId);
                    if (enumDefine != null)
                    {
                        if (field.IsArray)
                        {
                            builder.AppendLine(
                                $"        public IEnumerable<Yorozu.DB.{enumDefine.Name}> {field.Name} => {field.DataType.ToString()}s({field.ID}, {field.EnumDefineId}).Select(v => (Yorozu.DB.{enumDefine.Name})v);");
                            builder.AppendLine($"#if UNITY_EDITOR");
                            builder.AppendLine(
                                $"        public void Add{field.Name}(Yorozu.DB.{enumDefine.Name} value) => Add({field.ID}, (int)value);");
                            builder.AppendLine($"#endif");
                        }
                        else
                        {
                            builder.AppendLine($"        public Yorozu.DB.{enumDefine.Name} {field.Name}");
                            builder.AppendLine($"        {{");
                            builder.AppendLine(
                                $"            get {{ return (Yorozu.DB.{enumDefine.Name}) {DataType.Int.ToString()}({field.ID}); }}");
                            builder.AppendLine($"#if UNITY_EDITOR");
                            builder.AppendLine($"            set {{ Set({field.ID}, (int)value); }}");
                            builder.AppendLine($"#endif");
                            builder.AppendLine($"        }}");
                        }
                    }
                }
                else if (field.DataType == DataType.DBClass)
                {
                    var className = field.ReferenceDefine.ClassName;
                    if (field.IsArray)
                    {
                        builder.AppendLine(
                            $"        public IEnumerable<{className}> {field.Name} => MultiData<{className}>({field.ID});");
                        builder.AppendLine($"");
                        builder.AppendLine(
                            $"        public IEnumerable<string> {field.Name}Keys => Strings({field.ID});");
                        builder.AppendLine($"#if UNITY_EDITOR");
                        builder.AppendLine(
                            $"        public void Add{field.Name}Keys(string value) => Add({field.ID}, value);");
                        builder.AppendLine($"#endif");
                    }
                    else
                    {
                        builder.AppendLine(
                            $"        public {className} {field.Name} => Data<{className}>({field.ID});");
                        builder.AppendLine($"");
                        builder.AppendLine($"        public string {field.Name}Key");
                        builder.AppendLine($"        {{");
                        builder.AppendLine($"            get {{ return String({field.ID}); }}");
                        builder.AppendLine($"#if UNITY_EDITOR");
                        builder.AppendLine($"            set {{ Set({field.ID}, value); }}");
                        builder.AppendLine($"#endif");
                        builder.AppendLine($"        }}");
                    }
                }
                else
                {
                    if (field.IsArray)
                    {
                        builder.AppendLine(
                            $"        public IEnumerable<{field.DataType.ConvertString()}> {field.Name} => {field.DataType.ToString()}s({field.ID});");
                        builder.AppendLine($"#if UNITY_EDITOR");
                        builder.AppendLine(
                            $"        public void Add{field.Name}({field.DataType.ConvertString()} value) => Add({field.ID}, value);");
                        builder.AppendLine($"#endif");
                    }
                    else
                    {
                        builder.AppendLine($"        public {field.DataType.ConvertString()} {field.Name}");
                        builder.AppendLine($"        {{");
                        builder.AppendLine($"            get {{ return {field.DataType.ToString()}({field.ID}); }}");
                        builder.AppendLine($"#if UNITY_EDITOR");
                        builder.AppendLine($"            set {{ Set({field.ID}, value); }}");
                        builder.AppendLine($"#endif");
                        builder.AppendLine($"        }}");
                    }
                }

                builder.AppendLine("");
            }

            var extendType = data.ExtendFieldsType;
            if (extendType != null)
            {
                builder.AppendLine($"        // Extend Fields");
                var fields = YorozuDBExtendUtility.FindFields(extendType);
                foreach (var field in fields)
                {
                    builder.AppendLine(
                        $"        public {field.FieldType.GetArrayType().ConvertGenerateString()} {field.Name}");
                    builder.AppendLine($"        {{");
                    builder.AppendLine(
                        $"            get {{ return Extend<{extendType.FullName}>().{field.Name}[row]; }}");
                    builder.AppendLine($"#if UNITY_EDITOR");
                    builder.AppendLine(
                        $"            set {{ Extend<{extendType.FullName}>().{field.Name}[row] = value; }}");
                    builder.AppendLine($"#endif");
                    builder.AppendLine($"        }}");
                    builder.AppendLine("");
                }
            }

            builder.AppendLine("        public override string ToString()");
            builder.AppendLine("        {");
            builder.AppendLine("            var builder = new System.Text.StringBuilder();");
            builder.AppendLine("            builder.AppendLine($\"Type: {GetType().Name}\");");
            foreach (var field in data.Fields)
            {
                switch (field.DataType)
                {
                    case DataType.Sprite:
                    case DataType.GameObject:
                    case DataType.AudioClip:
                    case DataType.ScriptableObject:
                    case DataType.UnityObject:
                        builder.AppendLine(
                            $"            builder.AppendLine($\"{field.Name}: {{({field.Name} == null ? \"null\" : {field.Name}.ToString())}}\");");
                        break;
                    default:
                        builder.AppendLine(
                            $"            builder.AppendLine($\"{field.Name}: {{{field.Name}.ToString()}}\");");
                        break;
                }
            }

            if (extendType != null)
            {
                var fields = YorozuDBExtendUtility.FindFields(extendType);
                foreach (var field in fields)
                {
                    builder.AppendLine(
                        $"            builder.AppendLine($\"{field.Name}: {{{field.Name}.ToString()}}\");");
                }
            }

            builder.AppendLine("            return builder.ToString();");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine("}");

            string GetInterfaceName(DataType type)
            {
                switch (type)
                {
                    case DataType.String:
                        return nameof(IStringKey);
                    case DataType.Int:
                    case DataType.Enum:
                        return nameof(IIntKey);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            return builder.ToString();
        }

        private static string CustomDataScriptString(YorozuDBDataDefineObject data)
        {
            var builder = new StringBuilder();
            builder.AppendLine("namespace Yorozu.DB");
            builder.AppendLine("{");
            builder.AppendLine($"    public partial class {data.ClassName}");
            builder.AppendLine("    {");
            builder.AppendLine("    }");
            builder.AppendLine("}");

            return builder.ToString();
        }

        /// <summary>
        /// Enum ファイルを生成
        /// </summary>
        private static void CreateEnumScript(YorozuDBEnumDataObject enumData, string savePath)
        {
            if (enumData == null)
                return;

            var enumExportPath = Path.Combine(savePath, "Enum");
            // ディレクトリ作成
            if (!AssetDatabase.IsValidFolder(enumExportPath))
            {
                AssetDatabase.CreateFolder(savePath, "Enum");
            }

            foreach (var define in enumData.Defines)
            {
                var filePath = Path.Combine(enumExportPath, $"{define.Name}.cs");

                var builder = new StringBuilder();
                builder.AppendLine("");

                builder.AppendLine("namespace Yorozu.DB");
                builder.AppendLine("{");
                if (define.Flags)
                {
                    builder.AppendLine($"    [System.Flags]");
                }

                builder.AppendLine($"    public enum {define.Name}");
                builder.AppendLine("    {");
                for (var i = 0; i < define.KeyValues.Count; i++)
                {
                    var kv = define.KeyValues[i];
                    if (define.Flags)
                    {
                        builder.AppendLine($"       {kv.Value} = 1 << {i},");
                    }
                    else
                    {
                        builder.AppendLine($"       {kv.Value},");
                    }
                }

                builder.AppendLine("    }");
                builder.AppendLine("}");

                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    writer.WriteLine(builder.ToString());
                }
            }
        }


        /// <summary>
        /// 文字列に変換
        /// </summary>
        private static string ConvertString(this DataType type)
        {
            return type switch
            {
                DataType.String => "string",
                DataType.Float => "float",
                DataType.Int => "int",
                DataType.Bool => "bool",
                DataType.Sprite => "Sprite",
                DataType.GameObject => "GameObject",
                DataType.AudioClip => "AudioClip",
                DataType.ScriptableObject => "ScriptableObject",
                DataType.UnityObject => "UnityEngine.Object",
                DataType.Vector2 => "Vector2",
                DataType.Vector3 => "Vector3",
                DataType.Vector2Int => "Vector2Int",
                DataType.Vector3Int => "Vector3Int",
                DataType.Enum => "int",
                DataType.Color => "Color",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }


        /// <summary>
        /// 生成用の名前に変換
        /// </summary>
        internal static string ConvertGenerateString(this Type type, bool isFull = true)
        {
            if (type == typeof(bool))
                return "bool";
            if (type == typeof(byte))
                return "byte";
            if (type == typeof(char))
                return "char";
            if (type == typeof(decimal))
                return "decimal";
            if (type == typeof(double))
                return "double";
            if (type == typeof(float))
                return "float";
            if (type == typeof(int))
                return "int";
            if (type == typeof(long))
                return "long";
            if (type == typeof(object))
                return "object";
            if (type == typeof(sbyte))
                return "sbyte";
            if (type == typeof(short))
                return "short";
            if (type == typeof(string))
                return "string";
            if (type == typeof(uint))
                return "uint";
            if (type == typeof(ulong))
                return "ulong";
            if (type == typeof(ushort))
                return "ushort";

            if (isFull)
                return type.FullName.Replace("+", ".");

            return type.Name.Replace("+", ".");
        }
    }
}

#endif