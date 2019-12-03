using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BaiSingleton;
using System;
using System.IO;
using UnityEditor;

namespace BaiUISystem
{
    public class UISystem : CSharpSingleton<UISystem>
    {
        public UISystemComponent uiCp;
        public ShowViewSystem showSystem;
        public UIPoolSystem poolSystem;

        public UISystem()
        {
            uiCp = new UISystemComponent();
            poolSystem = new UIPoolSystem();
            showSystem = new ShowViewSystem();

#if UNITY_EDITOR
            string folderName = Application.dataPath + "/Resources/" + uiCp.UI_PATH.Substring(0, uiCp.UI_PATH.Length - 1);
            if (Directory.Exists(folderName) == false)
            {
                Directory.CreateDirectory(folderName);
                AssetDatabase.Refresh();
            }
#endif
        }


        int GetViewTypeIndex(E_UIType type)
        {
            return uiCp.views.FindIndex(v => v.uiType == type);
        }

        GameObject GetUI(int uiIndex)
        {
            try
            {
                var uiInfo = uiCp.views[uiIndex];
                return uiInfo.uis[uiInfo.uis.Count - 1];
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        GameObject CreateUI(E_UIType type, bool isSingleton = false)
        {
            int index = GetViewTypeIndex(type);

            if ((isSingleton && index == -1) || !isSingleton)
            {
                GameObject canvasObj = GetCanvas(type);

                // Instantiate
                // var prefab = poolSystem.Get((int)type);
                // if (prefab == null)
                // {
                //     prefab = Resources.Load<GameObject>(uiCp.UI_PATH + type.ToString());
                //     poolSystem.Push((int)type, prefab);
                // }
                // var ui = GameObject.Instantiate(prefab, canvasObj.transform);
                var ui = UISystemUtil.CreateUIFromPrefab(poolSystem, 
                    (int)type, 
                    uiCp.UI_PATH + type.ToString(), 
                    canvasObj.transform);

                // Add to container
                if (index == -1)
                {
                    uiCp.views.Add(new Views(type, new GameObject[] { ui }));
                }
                else
                {
                    uiCp.views[index].uis.Add(ui);
                }

                index = uiCp.views.Count - 1;
            }

            return GetUI(index);
        }

        /// <summary>
        /// Open view with type
        /// </summary>
        public GameObject OpenUI(E_UIType type, bool singleton = false)
        {
            GameObject _view;

            // 重新排序
            ResetOrder();

            _view = CreateUI(type, singleton);

            // pop to front
            PopToFront(type);

            showSystem.ShowView(_view);

            uiCp.lastMainView = type;

            uiCp.openView = type;

            return _view;
        }

        public E_UIType GetOpenType()
        {
            return uiCp.openView;
        }

        /// <summary>
        /// Pop View in front of all views
        /// </summary>
        /// <param name="type"></param>
        public void PopToFront(E_UIType type)
        {
            if (uiCp.canvases.ContainsKey(type))
            {
                Canvas _c = uiCp.canvases[type];

                // order
                int _order = uiCp.POP_WINDOW_ORDER;
                switch (type)
                {
                    case E_UIType.PopupUI:
                        _order = uiCp.POP_WINDOW_ORDER;
                        break;

                    default:
                        _order = GetLargestOrder() + 1;
                        break;
                }

                _c.sortingOrder = _order;

                _c.gameObject.SetActive(true);
                int index = GetViewTypeIndex(type);
                if (index != -1)
                {
                    GetUI(index).gameObject.SetActive(true);
                }
            }
        }

        public void HideView(E_UIType type)
        {
            int index = GetViewTypeIndex(type);

            if (index != -1)
            {
                GetUI(index).gameObject.SetActive(false);
            }
        }

        public GameObject GetCanvas(E_UIType type)
        {
            GameObject _canvasObj;

            if (uiCp.canvases.ContainsKey(type))
            {
                _canvasObj = uiCp.canvases[type].gameObject;
            }
            else
            {
                _canvasObj = new GameObject(type.ToString() + "_canvas");
                GameObject.DontDestroyOnLoad(_canvasObj);
                Canvas _canvas = _canvasObj.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                _canvas.pixelPerfect = false;
                if (_canvas.worldCamera == null)
                    _canvas.worldCamera = Camera.main;

                // Set oreder, get largest order
                int _order = 0;
                switch (type)
                {
                    case E_UIType.PopupUI:
                        _order = 100;
                        break;

                    default:
                        _order = GetLargestOrder() + 1;
                        break;
                }

                _canvas.sortingOrder = _order;

                CanvasScaler _scaler = _canvasObj.AddComponent<CanvasScaler>();
                _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
                _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

                _canvasObj.AddComponent<GraphicRaycaster>();
                uiCp.canvases.Add(type, _canvas);

            }

            return _canvasObj;
        }

        /// <summary>
        /// 得到最上曾的canvas
        /// </summary>
        /// <returns></returns>
        public GameObject GetUperCanvas(List<E_UIType> exceptType = null)
        {
            int _order = -1;
            E_UIType index = uiCp.canvases.Keys.First();
            foreach (var _c in uiCp.canvases.Keys)
            {
                if (uiCp.canvases[_c].sortingOrder > _order && (exceptType == null || exceptType.Contains(_c) == false))
                {
                    if (uiCp.canvases[_c].transform.childCount > 0)
                    {
                        index = _c;
                        _order = uiCp.canvases[_c].sortingOrder;
                    }
                }
            }

            return uiCp.canvases[index].gameObject;
        }

        /// <summary>
        /// 给定view是否正在显示
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public bool IsViewShowing(E_UIType view)
        {
            int order = GetLargestOrder();

            if (uiCp.canvases.ContainsKey(view))
            {
                return uiCp.canvases[view].sortingOrder == order;
            }

            return false;
        }

        public int GetLargestOrder()
        {
            int _order = 0;
            foreach (var _c in uiCp.canvases.Keys)
            {
                if (uiCp.canvases[_c].sortingOrder > _order && _c != E_UIType.PopupUI)
                {
                    _order = uiCp.canvases[_c].sortingOrder;
                }
            }

            return _order;
        }

        void ResetOrder()
        {
            if (uiCp.canvases.Values.Count > 0)
            {
                var _dicSort = from _dic in uiCp.canvases.Values orderby _dic.sortingOrder select _dic;
                int _minOrder = _dicSort.First<Canvas>().sortingOrder;
                foreach (var _c in uiCp.canvases.Keys)
                {
                    if (_c != E_UIType.PopupUI)
                        uiCp.canvases[_c].sortingOrder -= _minOrder;
                }
            }
        }

        /// <summary>
        /// Close view
        /// </summary>
        /// <param name="type"></param>
        public void CloseUI(E_UIType type)
        {
            int index = GetViewTypeIndex(type);
            if (index != -1)
            {
                var ui = GetUI(index);

                showSystem.CloseView(ui);
                RemoveUI(index);

                CheckCanvasUsable(index);
            }
        }

        void RemoveUI(int uiIndex)
        {
            var uiContainer = uiCp.views[uiIndex].uis;
            if (uiContainer.Count > 0)
            {
                uiContainer.RemoveAt(uiContainer.Count - 1);
            }
        }

        void CheckCanvasUsable(int uiIndex)
        {
            if (uiCp.views[uiIndex].uis.Count == 0)
            {
                var type = uiCp.views[uiIndex].uiType;
                GameObject.Destroy(uiCp.canvases[type].gameObject);
                uiCp.canvases.Remove(type);
            }
        }

        /// <summary>
        /// Update camera of all cancas
        /// </summary>
        public void UpdateCamera()
        {
            foreach (var _key in uiCp.canvases.Keys)
            {
                if (uiCp.canvases[_key] != null)
                    uiCp.canvases[_key].worldCamera = Camera.main;
            }
        }

        public GameObject ReopenView(E_UIType type)
        {
            CloseUI(type);
            return OpenUI(type);
        }
    }
}