using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEditor;
using System.Linq;

namespace Thirties.UnofficialBang
{
    public abstract class BaseAssetTable<T> : ScriptableObject
        where T : Object
    {
        [SerializeField]
        [PropertyOrder(2)]
        [PropertySpace]
        private List<T> resources;

#if UNITY_EDITOR

        [SerializeField]
        [PropertyOrder(0)]
        [FilePath]
        private string[] folders;

        private bool IsValidFoldersPath => folders != null && folders.Length > 0;

        [EnableIf("IsValidFoldersPath")]
        [PropertyOrder(1)]
        [Button]
        private void LoadFolders()
        {
            resources = new List<T>();

            string filter = $"t:{typeof(T).Name.ToLower()}";
            string[] guids = AssetDatabase.FindAssets(filter, folders);

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    resources.Add(asset);
                }
            }
            Debug.Log("<color=yellow>LOADED: " + resources.Count + " " + typeof(T) + "s</color>");
        }

#endif

        public T Get(string name)
        {
            return resources?.SingleOrDefault(r => r.name == name);
        }
    }
}