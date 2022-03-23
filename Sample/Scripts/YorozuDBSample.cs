using UnityEngine;
using UnityEngine.UI;
using Yorozu.DB;

public class YorozuDBSample : MonoBehaviour
{
    [SerializeField]
    private YorozuDBEnumDataObject _enumData;

    [SerializeField]
    private YorozuDBDataObject[] _data;

    [SerializeField]
    private Button _button;

    private void Awake()
    {
        YorozuDB.SetEnum(_enumData);
        YorozuDB.SetData(_data);
        _button.onClick.AddListener(Click);
    }

    private void Click()
    {
        var data = YorozuDB.Find<SampleData>(Yorozu.DB.Sample.A);
        if (data == null)
            return;

        Debug.Log(data.ToString());
    }
}