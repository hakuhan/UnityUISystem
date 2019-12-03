using System.Collections.Generic;
using BaiSingleton;
using UnityEngine;

namespace BaiUISystem
{
    public class UIPopupSystem : CSharpSingleton<UIPopupSystem>
    {
        public UIPopupComponent popCp;
        public IShowViewSystem popuiShowSystem;
        public UIPoolSystem poolSystem;

        public UIPopupSystem()
        {
            popCp = new UIPopupComponent();
            poolSystem = new UIPoolSystem();
            popuiShowSystem = new ShowPopSystem();
        }

        /// <summary>
        /// Current showing view
        /// </summary>
        /// <param name="type"></param>
        /// <param name="AttachedView"></param>
        public GameObject PopWindow(E_PopupType type, E_UIType attachedView = E_UIType.PopWindowView, bool isSingleton = false)
        {
            int popIndex = GetPopupIndex(type);
            if (isSingleton && popIndex != -1)
            {
                return popCp.popups[popIndex].uis[0];
            }

            Transform _parent = UISystem.Instance.GetCanvas(attachedView).transform;
            UISystem.Instance.PopToFront(E_UIType.PopWindowView);

            var _pop = UISystemUtil.CreateUIFromPrefab(poolSystem, (int)type, popCp.POP_PATH + type.ToString(), _parent);
            _pop.transform.SetSiblingIndex(0);

            // Update container
            if (popIndex == -1)
            {
                popCp.popups.Add(new PopupDatas(type, new GameObject[] { _pop }));
            }
            else
            {
                popCp.popups[popIndex].uis.Add(_pop);
            }

            popuiShowSystem.ShowView(_pop);

            return _pop;
        }

        public void CloseWindow(E_PopupType type)
        {
            int popIndex = GetPopupIndex(type);
            if (popIndex != -1)
            {
                var popData = popCp.popups[popIndex];
                if (popData.uis.Count > 0)
                {
                    popuiShowSystem.CloseView(popData.uis[popData.uis.Count - 1]);
                    popData.uis.RemoveAt(popData.uis.Count - 1);
                }
            }
        }

        /// <summary>
        /// 是否只包含对应的类型
        /// </summary>
        /// <param name="pType"></param>
        /// <returns></returns>
        public bool IsOnlyContainsType(List<E_PopupType> pTypes)
        {
            bool isRight = true;
            if (popCp.popups.Count == 0)
            {
                isRight = true;
            }
            else
            {
                foreach (var popData in popCp.popups)
                {
                    if (pTypes.Contains(popData.type) == false)
                    {
                        isRight = false;
                        break;
                    }
                }
            }


            return isRight;
        }

        /// <summary>
        /// 是否包含以下类型
        /// </summary>
        /// <param name="pTypes"></param>
        /// <returns></returns>
        public bool IsContainsTypes(List<E_PopupType> pTypes)
        {
            bool isRight = false;
            if (popCp.popups.Count == 0)
            {
                isRight = false;
            }
            else
            {
                foreach (var popData in popCp.popups)
                {
                    if (pTypes.Contains(popData.type))
                    {
                        isRight = true;
                        break;
                    }
                }
            }


            return isRight;
        }

        public int GetPopupIndex(E_PopupType type)
        {
            return popCp.popups.FindIndex(p => p.type == type);
        }
    }
}