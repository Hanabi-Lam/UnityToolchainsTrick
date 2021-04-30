using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class HappyEditor : EditorWindow
{
    /// <summary>
    /// 目标是做一个宽度限制的滑动列表所以可以写死宽度
    /// 当然你也可以根据窗口缩放currentScrollViewHeight = this.position.height / 2;
    /// </summary>
    private float m_currentScrollViewWidth = 100;

    /// <summary>
    /// 定义一个鼠标感应区
    /// </summary>
    private Rect cursorChangeRect;

    private Vector2 m_scrollV2;

    bool resize = false;

    string m_SearchFieldText;
    string m_lastSearchFilter;
    private void OnEnable()
    {
        m_currentScrollViewWidth = this.position.width * 0.3f;
        cursorChangeRect = new Rect(m_currentScrollViewWidth - 5, 0, 10, this.position.height);
    }

    private void OnGUI()
    {
        //Rect rect = GUILayoutUtility.GetRect(120f, 300f, 18f, 20f, EditorStyles.toolbarSearchField, GUILayout.MinWidth(120f), GUILayout.MaxWidth(300f));
        //m_lastSearchFilter = EditorGUI.TextField(rect, m_SearchFieldText, EditorStyles.toolbarSearchField);
        TopToolBarDrawer();

        GUILayout.BeginVertical();


        m_scrollV2 = GUILayout.BeginScrollView(m_scrollV2, GUILayout.Width(m_currentScrollViewWidth));
        for (int i = 0; i < 40; i++)

            GUILayout.Label("菜菜菜菜单单单单单", "Box");
        GUILayout.EndScrollView();
        ResizeScrollView();


        GUILayout.EndVertical();
        Repaint();

    }

    private void ResizeScrollView()
    {
        //var orignColor = GUI.color;
        //GUI.color = Color.gray;
        ////绘制一个底图
        //GUI.DrawTexture(cursorChangeRect, EditorGUIUtility.whiteTexture);

        //TODO备注
        EditorGUIUtility.AddCursorRect(cursorChangeRect, MouseCursor.ResizeHorizontal);
        if (Event.current.type == EventType.MouseDown && cursorChangeRect.Contains(Event.current.mousePosition))
        {
            resize = true;
        }
        if (resize)
        {
            m_currentScrollViewWidth = Event.current.mousePosition.x;
            cursorChangeRect.Set(m_currentScrollViewWidth, cursorChangeRect.y, cursorChangeRect.width, cursorChangeRect.height);
        }
        if (Event.current.type == EventType.MouseUp)
            resize = false;
        //GUI.color = orignColor;
    }

    void TopToolBarDrawer()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Clear All",EditorStyles.toolbarButton,GUILayout.MaxWidth(100f)))
        {

        }
        if (GUILayout.Button("下拉列表",EditorStyles.toolbarPopup, GUILayout.MaxWidth(200f)))
        {

        }
        Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.IconContent("CreateAddNew"), EditorStyles.toolbarDropDown, GUILayout.MaxWidth(240f), GUILayout.MinWidth(120f));
        if (EditorGUI.DropdownButton(rect, EditorGUIUtility.IconContent("CreateAddNew"), FocusType.Passive, EditorStyles.toolbarDropDown))
        {
            GUIUtility.hotControl = 0;
            EditorUtility.DisplayPopupMenu(rect, "Assets/Create", null);
        }
        
        //SearchField
         rect = GUILayoutUtility.GetRect(120f, 300f, 18f, 20f, EditorStyles.toolbarSearchField, GUILayout.MinWidth(120f), GUILayout.MaxWidth(300f));
        m_lastSearchFilter = EditorGUI.TextField(rect, m_SearchFieldText, EditorStyles.toolbarSearchField);
        if (m_lastSearchFilter != m_SearchFieldText)
        {
            m_SearchFieldText = m_lastSearchFilter;
        }
        GUILayout.EndHorizontal();
    }





    [MenuItem("Happy/Editor")]
    static void PopUp()
    {
       var window = GetWindowWithRect<HappyEditor>(new Rect(200, 300, 500, 500));
        window.titleContent = new GUIContent("快乐编辑器", EditorGUIUtility.FindTexture("BuildSettings.Editor.Small"), "哈哈哈哈");
    }

    //private class ItemDrawer: 
}