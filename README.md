# UnityYorozuDB

Unity用のデータを扱えるDBみたいなもので 、Runtime 中に値を変えても反映されるような作りとなっています

YorozuDB ではデータ定義、データ、Enum の3種類のデータを利用して管理します

## ツール

"Tools/YorozuDB" より ツールを開くことができます

<img src="https://github.com/yayorozu/ImageUploader/blob/master/YorozuDB/Top.png" width="700">

### データ定義の作成

`Create Data Define Asset` からファイルを選択するとデータ定義ファイルが作成できます

これを作成することで以下のデータ型から利用したい型を選択して定義をいじれます

* Int
* String
* Float,
* Bool
* Sprite
* GameObject
* ScriptableObject
* UnityObject
* Enum,
* Vector2
* Vector3
* Vector2Int
* Vector3Int

### データ定義の編集

<img src="https://github.com/yayorozu/ImageUploader/blob/master/YorozuDB/DefineEdit.png" width="700">

データ定義を作成すると一覧に表示されるので、選択すると編集メニューが開きます

`Add Field` で適切なデータを定義して `Add` を押すと `Fields` に追加されます

Int, String, Enum の場合は検索のKeyに設定することができるため、○を押して☆にすることでそれを Key として扱えます

また、初期値も定義できます

### データの追加

左のデータ定義を右クリックして `Create Data` をすることでデータを追加できます

### データの編集

<img src="https://github.com/yayorozu/ImageUploader/blob/master/YorozuDB/DataEdit.png" width="700">

データをクリックするとデータ編集メニューが開きます

ここでのフィールドの定義はデータ定義で設定した順番になるので、必要であればそちらから並べ替えてください

右上の `Add Row` で1行分データが追加できるので必要な分だけ追加していじってください

### Enum の追加

左上の `Create Enum Data Asset` から Enum用のアセットを作成します

作成すると `Enum` が追加されるのでクリックすると Enum 編集メニューが開きます

<img src="https://github.com/yayorozu/ImageUploader/blob/master/YorozuDB/EnumEdit.png" width="700">

右上に 名前を入れて `Add Enum` を押すと Enum の定義が生成さるので、適切に名前を入れてください

※従来の Enum と違って値を指定することはできません

### スクリプトの生成

データにアクセスするためには、自動生成されたクラスを利用する必要があるので

左下の `Generate Script From Define` を押してください、最初は生成場所を聴かれるため

生成場所を選ぶとデータのクラスと Enum が出力されます

## サンプル

YorozuDBEnumDataObject と YorozuDBDataObject を準備して
YorozuDB に渡すとデータを探してくることができます

```c#
[SerializeField]
private YorozuDBEnumDataObject _enumData;

[SerializeField]
private YorozuDBDataObject[] _data;

private void Prepare()
{
    YorozuDB.SetEnum(_enumData);
    YorozuDB.SetData(_data);
}
```

あとは YorozuDB.Find でクラスと Key に指定した値を渡すことでデータが取得できます

```c#
private void Log()
{
    var data = YorozuDB.Find<SampleData>(1);
    if (data == null)
        return;

    Debug.Log(data.ToString());
}
```

同じ Key を複数セットしていれば、複数取得することも可能です

```c#
private void Log()
{
    var data = YorozuDB.FindMany<SampleData>(1);
    foreach (var d in data)
    {
        Debug.Log(d.ToString());
    }
}
```

データクラスは ToString を override してるため中身を見る際にはこれをログに渡せばお手軽に確認できます

```C#
public override string ToString()
{
    var builder = new System.Text.StringBuilder();
    builder.AppendLine($"Type: {GetType().Name}");
    builder.AppendLine($"Key: {Key.ToString()}");
    builder.AppendLine($"Value: {Value.ToString()}");
    builder.AppendLine($"EnumKey: {EnumKey.ToString()}");
    builder.AppendLine($"Vector3Data: {Vector3Data.ToString()}");
    return builder.ToString();
}
```