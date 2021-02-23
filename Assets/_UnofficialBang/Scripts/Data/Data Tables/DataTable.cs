using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

namespace Thirties.UnofficialBang
{
    public abstract class DataTable<T> : SerializedScriptableObject
    where T : BaseData
    {
        [OdinSerialize]
        [PropertyOrder(2)]
        [PropertySpace]
        [TableList]
        private List<T> records;
        public List<T> Records => records;

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

            records = JsonConvert.DeserializeObject<List<T>>(json.text);
            records.OrderBy(x => x.Id);

            EditorUtility.SetDirty(this);
        }

#endif
    }
}
