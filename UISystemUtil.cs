using UnityEngine;

namespace BaiUISystem
{
    public class UISystemUtil
    {
        public static GameObject CreateUIFromPrefab(UIPoolSystem poolSystem, int prefabID, string prefabPath, Transform parent)
        {
            // Instantiate
            var prefab = poolSystem.Get(prefabID);
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>(prefabPath);
                poolSystem.Push(prefabID, prefab);
            }
            var ui = GameObject.Instantiate(prefab, parent);

            return ui;
        }
    }
}