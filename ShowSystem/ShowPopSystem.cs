using System;
using UnityEngine;

namespace BaiUISystem
{
    public class ShowPopSystem : IShowViewSystem
    {
        public void ShowView(GameObject viewObj, Action callback = null)
        {
            if (callback != null)
                callback();
        }
         public void CloseView(GameObject viewObj, Action callback = null)
         {
             if (callback != null)
             {
                 callback();
             }
         }
    }
}