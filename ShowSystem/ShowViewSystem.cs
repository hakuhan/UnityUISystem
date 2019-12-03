using System;
using UnityEngine;

namespace BaiUISystem
{
    public class ShowViewSystem : IShowViewSystem
    {
        public void ShowView(GameObject obj, Action callback = null)
        {
            if (callback != null)
                callback();
        }

        public void CloseView(GameObject obj, Action callback = null)
        {
            if (callback != null)
                callback();
        }
    }
}