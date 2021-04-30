using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>
/// �˵����ڻ��� 
/// </summary>
public abstract class BMenuEditorWindowBase : EditorWindow
{
    BMenuView menuView;
    TreeViewState viewState = new TreeViewState();

    void OnEnable()
    {
        searchField = new SearchField();
        menuView = BuildMenuTree(viewState);
        menuView.Reload();//�����������ǿ��TreeView�����������ݡ��ⷴ�����ֻᵼ��BuildRoot��BuildRows�����á�
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




    #region ������
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
    /// ������ߵĲ˵�
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
    /// ��Դ����TreeView�ķ�װ
    /// </summary>
    public class BMenuView : TreeView
    {
        List<TreeViewItem> items = new List<TreeViewItem>();
        Dictionary<int, BMenuViewItem> treeViewItemMap = new Dictionary<int, BMenuViewItem>();

        //���־���
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
        //�ⲿ�ӿ�

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
            //һ���˵�
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
        /// ���β˵�
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            //����һ����
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
    /// ��Դ����TreeViewItem�ķ�װ
    /// TreeViewItem (TreeViewItem)�������ڵ���TreeView������ݣ�
    /// �����ڱ༭���й������ṹ�ı�ʾ��
    /// ÿ��TreeViewItem���빹��һ��Ψһ������ID(��TreeView��������Ŀ��Ψһ)��
    /// ID���������в���ѡ��״̬��չ��״̬�͵���������������Unity����ʹ��ÿ�������GetInstanceID��ΪTreeViewItem��ID��
    /// �����¼��ؽű����ڱ༭���н��벥��ģʽʱ����TreeViewState��ʹ��id�������û����ĵ�״̬(����չ��)��
    /// ���е�TreeViewItems����һ��depth���ԣ�����ָʾ�Ӿ��ϵ�������
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
