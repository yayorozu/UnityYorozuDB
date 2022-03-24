using System.Linq;
using System.Text;
using TMPro;
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

    [SerializeField]
    private InputField _inputField;

    [SerializeField]
    private Toggle _toggle;

    [SerializeField]
    private Text _findDataText;

    private void Awake()
    {
        YorozuDB.SetEnum(_enumData);
        YorozuDB.SetData(_data);
        _button.onClick.AddListener(Click);
    }

    private void Click()
    {
        if (!int.TryParse(_inputField.text, out int key))
            return;

        if (_toggle.isOn)
        {
             var finds = YorozuDB.FindMany<SampleData>(key);
             var builder = new StringBuilder();
             builder.AppendLine($"Find {finds.Count()} Data");
             builder.AppendLine("");
             foreach (var find in finds)
             {
                 builder.AppendLine(find.ToString());
                 builder.AppendLine("");
             }
             _findDataText.text = builder.ToString();
        }
        else
        {
            YorozuDB.Find<SampleData>(key);
            var data = YorozuDB.Find<SampleData>(key);
            if (data == null)
            {
                _findDataText.text = "Not Found";
                return;
            }
            
            _findDataText.text = data.ToString();
        }
    }
}