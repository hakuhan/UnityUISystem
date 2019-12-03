using UnityEngine;
using System;

namespace BaiUISystem
{
    public interface IShowViewSystem
    {
         void ShowView(GameObject viewObj, Action callback = null);
         void CloseView(GameObject viewObj, Action callback = null);
    }
}