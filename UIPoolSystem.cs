/*
Author: baihan
this script's main purpose is: manager prefab 
*/
using UnityEngine;

namespace BaiUISystem
{
    public class UIPoolSystem
    {
        public UIPoolComponent pCp;

        public UIPoolSystem()
        {
            pCp = new UIPoolComponent();
        }

        public void Push(int id, GameObject obj)
        {
            if (pCp.IDs.Contains(id) == false)
            {
                pCp.uiPrefabs.Add(obj);
                pCp.IDs.Add(id);
                pCp.weight.Add(1);
            }
            else
            {
                Debug.LogWarning("UIPoolManager: prefab aready added! please loading prefab installed of creating");
            }
        }

        public GameObject Get(int id)
        {
            int prefabIndex = pCp.IDs.IndexOf(id);
            if (prefabIndex != -1)
            {
                UpdateWeight(id);
                return pCp.uiPrefabs[prefabIndex];
            }

            return null;
        }

        public void RemoveType(int type)
        {
            int index = pCp.IDs.IndexOf(type);
            if (index != -1)
            {
                pCp.IDs.RemoveAt(index);
                pCp.uiPrefabs.RemoveAt(index);
                pCp.weight.RemoveAt(index);
            }
            else
            {
                Debug.Log("UIPoolManager: Aready remove type " + type.ToString());
            }
        }

        public void UpdateWeight(int id)
        {
            int prefabIndex = pCp.IDs.IndexOf(id);

            if (prefabIndex != -1)
            {
                ++pCp.weight[prefabIndex];
            }
            else
            {
                Debug.LogWarning("UIPoolManager: Update wait failed, prefab not exists");
            }
        }

        public void ReleaseUselessType()
        {
            for (int i = pCp.weight.Count - 1; i >= 0; --i)
            {
                if (pCp.weight[i] == 1)
                {
                    pCp.weight.RemoveAt(i);
                    pCp.IDs.RemoveAt(i);

                    Resources.UnloadAsset(pCp.uiPrefabs[i]);
                    pCp.uiPrefabs.RemoveAt(i);
                }
            }
        }
    }
}