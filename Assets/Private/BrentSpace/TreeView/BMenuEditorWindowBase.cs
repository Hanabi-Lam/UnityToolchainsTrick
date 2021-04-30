using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>
/// 菜单窗口基类 
/// </summary>
public abstract class BMenuEditorWindowBase : EditorWindow
{
    BMenuView menuView;
    TreeViewState viewState = new TreeViewState();

    void OnEnable()
    {
        searchField = new SearchField();
        menuView = BuildMenuTree(viewState);
        menuView.Reload();//调用这个函数强制TreeView重载它的数据。这反过来又会导致BuildRoot和BuildRows被调用。
    }
    void OnGUI()
    {
        DrawSplitMenu();
        Rect sideRect = resizableAreaRect;
        sideRect.x += sideRect.width;
        sideRect.width = 1;
        EditorGUI.DrawRect(sideRect, new Color(0.7f, 0.7f, 0.7f, 0.2f));
        DrawSeachTextField();
        DrawMenu();
   

        DrawSplitContent();
    }

    protected abstract BMenuView BuildMenuTree(TreeViewState state);




    #region 搜索框
    string searchText;
    Color gary = new Color(1, 1,1, 1f);
    SearchField searchField;

    Rect SearchTextFieldRect()
    {
        Rect rect = resizableAreaRect;

        rect.x += 5;
        rect.y += 3;
        rect.width -= 10;
        rect.height = 20;

        return rect;

    }
    void DrawSeachTextField()
    {
        var rect = SearchTextFieldRect();
        string tmp = searchField.OnGUI(rect, searchText);
        if (tmp != searchText)
        {
            searchText = tmp;
            menuView.searchString = searchText;
        }

    }
    #endregion


    #region split window


    private Rect resizableAreaRect = new Rect(0, 0, 150, 150);
    private Rect contentAreaRect = new Rect(0, 0, 150, 150);
    private Rect menuRect = new Rect(0, 0, 150, 150);
    private Rect cursorRect = new Rect(148, 0, 4, 150);


    
    #endregion
    /// <summary>
    /// 绘制左边的菜单
    /// </summary>
    private void DrawSplitMenu()
    {
        resizableAreaRect.height = position.height;
        //GUI.DrawTexture(resizableAreaRect, EditorGUIUtility.whiteTexture);
        cursorRect.height = position.height;
    }
    void DrawMenu()
    {
        menuRect = resizableAreaRect;
        menuRect.y += 20;
        menuRect.height -= 20;
        menuView.OnGUI(menuRect);
    }
    void DrawSplitContent()
    {
        contentAreaRect = resizableAreaRect;
        contentAreaRect.x += contentAreaRect.width;
        contentAreaRect.width = position.width - resizableAreaRect.width - 4;
        contentAreaRect.height = position.height;

        GUILayout.BeginArea(contentAreaRect);
        IList<int> selection = menuView.GetSelection();
        if (selection.Count > 0)
            ContentGUI(contentAreaRect, menuView.Find(selection[0]));
        GUILayout.EndArea();
    }
    protected virtual void ContentGUI(Rect _rect, BMenuViewItem _selectedItem)
    {
        _selectedItem.contentDrawer?.Invoke(_rect);
    }
    /// <summary>
    /// 对源生的TreeView的封装
    /// </summary>
    public class BMenuView : TreeView
    {
        List<TreeViewItem> items = new List<TreeViewItem>();
        Dictionary<int, BMenuViewItem> treeViewItemMap = new Dictionary<int, BMenuViewItem>();

        //文字居中
        private static GUIStyle m_labelStyle;
        public static GUIStyle LableStyle
        {
            get
            {
                if (m_labelStyle == null)
                {
                    m_labelStyle = new GUIStyle(GUI.skin.label);
                    LableStyle.alignment = TextAnchor.MiddleLeft;
                }
                return m_labelStyle;
            }
        }

        public BMenuView(TreeViewState state) : base(state)
        {
            rowHeight = 20;
          
        }
        //外部接口

        public BMenuViewItem AddMenuItem(string path)
        {
            return AddMenuItem(path, null);
        }
        int autoId = 0;
        public BMenuViewItem AddMenuItem(string path, Texture2D icon)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var current = items;
            string[] m_path = path.Split('/');


            if (m_path.Length > 1)
            {
                for (int i = 0; i < m_path.Length - 1; i++)
                {
                    BMenuViewItem currentParent = current.Find(t => t.displayName == m_path[0]) as BMenuViewItem;

                    if (currentParent == null)
                    {
                        currentParent = new BMenuViewItem(m_path[i]);
                        currentParent.children = new List<TreeViewItem>();
                        currentParent.displayName = m_path[i];
                        currentParent.id = autoId;
                        current.Add(currentParent);
                        treeViewItemMap[autoId] = currentParent;
                        autoId++;
                    }
                    current = currentParent.children;
                }
            }
            //一级菜单
            BMenuViewItem item = new BMenuViewItem(path);
            item.id = autoId;
            item.displayName = m_path[m_path.Length - 1];
            item.children = new List<TreeViewItem>();
            item.icon = icon;
            current.Add(item);
            treeViewItemMap[autoId] = item;
            autoId++;
            return item;
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1, -1, "Root");
            root.children = items;

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }



        public BMenuViewItem Find(int id)
        {
            treeViewItemMap.TryGetValue(id, out BMenuViewItem item);
            return item;
        }
        protected override bool CanRename(TreeViewItem item)
        {
            return false;
        }
        protected override void RenameEnded(RenameEndedArgs args)
        {
        }
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }


        /// <summary>
        /// 矩形菜单
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            //绘制一条线
            Rect lineRect = args.rowRect;
            lineRect.x = 10;
            lineRect.y += lineRect.height;
            lineRect.height = 1;
            lineRect.width -= 20; 
            EditorGUI.DrawRect(lineRect, new Color(0.7f, 0.7f, 0.7f, 0.2f));

            var item = args.item as BMenuViewItem;

            Rect labelRect = args.rowRect;
            labelRect.x += item.depth * depthIndentWidth + depthIndentWidth;
            GUI.Label(labelRect, new GUIContent(item.displayName, item.icon), LableStyle);
            item.itemDrawer?.Invoke(args.rowRect);
        }

    }


    /// <summary>
    /// 对源生的TreeViewItem的封装
    /// TreeViewItem (TreeViewItem)包含关于单个TreeView项的数据，
    /// 用于在编辑器中构建树结构的表示。
    /// 每个TreeViewItem必须构造一个唯一的整数ID(在TreeView的所有条目中唯一)。
    /// ID用于在树中查找选择状态、展开状态和导航的项。如果树代表Unity对象，使用每个对象的GetInstanceID作为TreeViewItem的ID。
    /// 当重新加载脚本或在编辑器中进入播放模式时，在TreeViewState中使用id来保存用户更改的状态(如扩展项)。
    /// 所有的TreeViewItems都有一个depth属性，用于指示视觉上的缩进。
    /// </summary>
    public class BMenuViewItem : TreeViewItem
    {
        string path;
        public Action<Rect> itemDrawer;
        public Action<Rect> contentDrawer;

        public string Path => path;
        public BMenuViewItem(string _path) : base()
        {
            path = _path;
        }
    }
}
