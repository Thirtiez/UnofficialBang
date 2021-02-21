using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using Thirties.UnofficialBang;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu(menuName = "Card Set Data")]
public class CardSetData : SerializedScriptableObject
{
    [OdinSerialize]
    [PropertyOrder(2)]
    [PropertySpace]
    [TableList]
    private List<CardData> cards;
    public List<CardData> Cards => cards;

#if UNITY_EDITOR
    [SerializeField]
    [PropertyOrder(0)]
    [FilePath(Extensions = "txt", RequireExistingPath = true)]
    private string jsonFilePath;

    private bool IsValidFilePath => !string.IsNullOrEmpty(jsonFilePath) && jsonFilePath.EndsWith(".txt");

    [EnableIf("IsValidFilePath")]
    [PropertyOrder(1)]
    [Button]
    private void Import()
    {
        var json = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonFilePath);
        if (json == null)
        {
            Debug.Log($"Json file {jsonFilePath} not found");
        }

        cards = JsonConvert.DeserializeObject<List<CardData>>(json.text);

        EditorUtility.SetDirty(this);
    }
#endif
}
