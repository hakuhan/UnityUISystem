using System.Collections.Generic;
using UnityEngine;

namespace BaiUISystem
{
    public enum E_PopupType
    {

    }
    
    public class PopupDatas
    {
        public E_PopupType type;
        public List<GameObject> uis = new List<GameObject>();

        public PopupDatas(E_PopupType type, GameObject[] ui)
        {
            this.type = type;
            uis = new List<GameObject>(ui);
        }
    }

    public class UIPopupComponent
    {
        public List<PopupDatas> popups = new List<PopupDatas>();
        public string POP_PATH = "Popup/";
    }
}