using System.Collections.Generic;
using UnityEngine;

namespace BaiUISystem
{
    public enum E_UIType
    {
        PopWindowView,
        Max,
    }

    public class Views
    {
        public E_UIType uiType;
        public List<GameObject> uis = new List<GameObject>();

        public Views(E_UIType type, GameObject[] ui)
        {
            uiType = type;
            uis = new List<GameObject>(ui);
        }
    }

    public class UISystemComponent
    {
        public string UI_PATH = "Ui/";
        public int POP_WINDOW_ORDER = 100;
        public Dictionary<E_UIType, Canvas> canvases = new Dictionary<E_UIType, Canvas>();
        public List<Views> views = new List<Views>();

        // record last view befor gaming
        public E_UIType lastMainView = E_UIType.Max;

        public E_UIType openView = E_UIType.Max;
    }
}