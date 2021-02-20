using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using Thirties.UnofficialBang;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu(menuName = "Card Set")]
public class CardSet : ScriptableObject
{
    [SerializeField]
    [FilePath(Extensions = "txt", RequireExistingPath = true)]
    private string jsonFilePath;

    [SerializeField]
    [TableList]
    private List<Card> cards;
    public List<Card> Cards => cards;

    private bool IsValidFilePath => !string.IsNullOrEmpty(jsonFilePath) && jsonFilePath.EndsWith(".txt");

    [EnableIf("IsValidFilePath")]
    [Button]
    private void Import()
    {
        var json = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonFilePath);
        if (json == null)
        {
            Debug.Log($"Json file {jsonFilePath} not found");
        }
        cards = JsonConvert.DeserializeObject<List<Card>>(json.text);
    }
}
