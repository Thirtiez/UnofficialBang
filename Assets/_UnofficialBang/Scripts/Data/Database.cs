using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEditor;
using System.Linq;

namespace Thirties.UnofficialBang
{
    public abstract class Database<T> : ScriptableObject
        where T : Object
    {
        [SerializeField]
        [FolderPath]
        private string[] folders;

        [SerializeField]
        private List<T> resources;

#if UNITY_EDITOR

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
            return resources.SingleOrDefault(x => x.name == name);
        }
    }
}