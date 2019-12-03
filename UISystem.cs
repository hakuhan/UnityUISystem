using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BaiUISystem
{
    public class UISystem : CSharpSingleton<UISystem>
        {
            private Dictionary<E_ViewType, Canvas> m_canvases;
            private Dictionary<E_ViewType, BaseView> m_views;

            public ShowViewHandler m_handler;

            // record last view befor gaming
            private E_ViewType m_lastMainView = E_ViewType.MainMenu;

            private E_ViewType m_openView = E_ViewType.MainMenu;


            public UISystem()
            {
                m_canvases = new Dictionary<E_ViewType, Canvas>();
                m_views = new Dictionary<E_ViewType, BaseView>();
                m_handler = new ShowViewHandler();
            }

            /// <summary>
            /// Open view with type
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public BaseView OpenView(E_ViewType type)
            {
                BaseView _view;

                // 重新排序
                ResetOrder();

                if (m_views.ContainsKey(type))
                {
                    // pop to front
                    _view = m_views[type];
                    

                    PopToFront(type);
                }
                else
                {
                    GameObject _canvasObj = GetCanvas(type);
                    PopToFront(type);
                    
                    GameObject _viewObj = ResourceManager.Instance.LoadGameObject(AppConst.VIEW_PATH + type.ToString(), true, _canvasObj.transform);
                    _viewObj.name = type.ToString();

                    _view = _viewObj.GetComponent<BaseView>();
                    if (_view == null)
                        _view = _viewObj.AddComponent<BaseView>();
                    _view.OnOpen();
                    _view.RegistEvents();

                    // Store
                    m_views.Add(type, _view);

                    // show effect
                    m_handler.ShowView(_viewObj);
                }

                if (type != E_ViewType.GamingUI
                    && type != E_ViewType.PopWindowView
                    && type != E_ViewType.SettingsView
                    && type != E_ViewType.VictoryView
                    &&type!=E_ViewType.AchievementView)
                    
                    m_lastMainView = type;
                
                m_openView = type;
                //发送view 改变事件，native 接收到到改变自身行为
                ViewManagerEvent.OnViewHierarchyChange();

                Resources.UnloadUnusedAssets();

                return _view;
            }

            public E_ViewType GetOpenType()
            {
                return m_openView;
            }

            /// <summary>
            /// Pop View in front of all views
            /// </summary>
            /// <param name="type"></param>
            public void PopToFront(E_ViewType type)
            {
                if (m_canvases.ContainsKey(type))
                {
                    Canvas _c = m_canvases[type];
                    
                    // order
                    int _order = AppConst.POP_WINDOW_ORDER;
                    switch (type)
                    {
                        case E_ViewType.PopWindowView:
                            _order = 100;
                        break;

                        default:
                            _order = GetLargestOrder() + 1;
                        break;
                    }

                    _c.sortingOrder = _order;

                    _c.gameObject.SetActive(true);
                    if (m_views.ContainsKey(type))
                    {
                        m_views[type].gameObject.SetActive(true);
                    }
                }
            }

            public void HideView(E_ViewType type)
            {
                if (m_views.ContainsKey(type))
                {
                    m_views[type].gameObject.SetActive(false);
                }
                ViewManagerEvent.OnViewHierarchyChange();
            }

            public GameObject GetCanvas(E_ViewType type)
            {
                GameObject _canvasObj;

                if (m_canvases.ContainsKey(type))
                {
                    _canvasObj = m_canvases[type].gameObject;
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
                        case E_ViewType.PopWindowView:
                            _order = 100;
                        break;

                        default:
                            _order = GetLargestOrder() + 1;
                        break;
                    }

                    _canvas.sortingOrder = _order;

                    CanvasScaler _scaler = _canvasObj.AddComponent<CanvasScaler>();
                    _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    _scaler.referenceResolution = new Vector2(AppConst.ScreenWidth, AppConst.ScrennHeight);
                    // _scaler.referenceResolution = new Vector2(1080, 1920);
                    if (type == E_ViewType.GamingUI || type == E_ViewType.VictoryView || type == E_ViewType.PopWindowView)
                    {
                        _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                    }
                    else
                    {
                        _scaler.matchWidthOrHeight = 0f;
                    }

                    // 设置页面字体锐化
                    if (type == E_ViewType.SettingsView)
                    {
                        _canvas.pixelPerfect = true;
                    }

                    _canvasObj.AddComponent<GraphicRaycaster>();
                    m_canvases.Add(type, _canvas);

                }

                return _canvasObj;
            }

            /// <summary>
            /// 得到最上曾的canvas
            /// </summary>
            /// <returns></returns>
            public GameObject GetUperCanvas(List<E_ViewType>exceptType = null)
            {
                int _order = -1;
                E_ViewType index = m_canvases.Keys.First();
                foreach (var _c in m_canvases.Keys)
                {
                    if (m_canvases[_c].sortingOrder > _order && (exceptType == null || exceptType.Contains(_c) == false))
                    {
                        if (m_canvases[_c].transform.childCount > 0)
                        {
                            index = _c;
                            _order = m_canvases[_c].sortingOrder;
                        }
                    }
                }

                return m_canvases[index].gameObject;
            }

            /// <summary>
            /// 给定view是否正在显示
            /// </summary>
            /// <param name="view"></param>
            /// <returns></returns>
            public bool IsViewShowing(E_ViewType view)
            {
                int order = GetLargestOrder();

                if (m_canvases.ContainsKey(view))
                {
                    return m_canvases[view].sortingOrder == order;
                }

                return false;
            }

            public int GetLargestOrder()
            {
                int _order = 0;
                foreach (var _c in m_canvases.Keys)
                {
                    if (m_canvases[_c].sortingOrder > _order && _c != E_ViewType.PopWindowView)
                        {
                            _order = m_canvases[_c].sortingOrder;
                        }
                }

                return _order;
            }

            void ResetOrder()
            {
                if (m_canvases.Values.Count > 0)
                {
                    var _dicSort = from _dic in m_canvases.Values orderby _dic.sortingOrder select _dic;
                    int _minOrder = _dicSort.First<Canvas>().sortingOrder;
                    foreach (var _c in m_canvases.Keys)
                    {
                        if (_c != E_ViewType.PopWindowView)
                            m_canvases[_c].sortingOrder -= _minOrder;
                    }
                }
            }

            /// <summary>
            /// Close view
            /// </summary>
            /// <param name="type"></param>
            public void CloseView(E_ViewType type)
            {
                if (m_views.ContainsKey(type))
                {
                    m_views[type].UnRegistEvents();

                    m_handler.CloseView(m_views[type].gameObject, () =>
                    {
                        // GameObject.Destroy(m_views[type].gameObject);
                        m_views[type].OnClose();

                        m_views.Remove(type);
                    });

                    GameObject.Destroy(m_canvases[type].gameObject);
                    m_canvases.Remove(type);
                }

                ViewManagerEvent.OnViewHierarchyChange();
            }

            /// <summary>
            /// Update camera of all cancas
            /// </summary>
            public void UpdateCamera()
            {
                foreach (var _key in m_canvases.Keys)
                {
                    if (m_canvases[_key] != null)
                        m_canvases[_key].worldCamera = Camera.main;
                }
            }

            public BaseView ReopenView(E_ViewType type)
            {
                CloseView(type);
                return OpenView(type);
            }

            public GameObject OpenViewTool(E_viewToolType type, Transform parent)
            {
                return ResourceManager.Instance.LoadGameObject(AppConst.VIEW_TOOL_PATH + type.ToString(), true, parent);
            }

            public GameObject OpenViewMainMenuPage(E_mainMenuPageType pageType, Transform parent)
            {
                return ResourceManager.Instance.LoadGameObject(AppConst.VIEW_MAINMENU_PAGE_PATH + pageType.ToString(), true,
                    parent);
            }

            public BaseView OpenRecentMainView()
            {
                var _result = m_lastMainView;
                m_lastMainView = E_ViewType.MainMenu;
                return OpenView(_result);
            }
        }
}