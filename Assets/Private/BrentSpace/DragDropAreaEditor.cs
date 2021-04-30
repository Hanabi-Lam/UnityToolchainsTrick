using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CustomEditor(typeof(DragDropArea))]
public class DragDropAreaEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //绘制一个Box区域
        var dragArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
        GUIContent title = new GUIContent("Drag Object here from Project view to get the object");    
        GUI.Box(dragArea, title);
        //区域的时间检测
        var  objs =  DragDropAreaMulti(dragArea);

        if (objs!= null)
        {
            Debug.Log(objs[0].name);
        }
  
    }


#if UNITY_EDITOR
    public static UnityEngine.Object[] DragDropAreaMulti(Rect rect)
    {
        Event evt = Event.current;
        if (!rect.Contains(evt.mousePosition)) return null;

        UnityEngine.Object[] temp = null;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    temp = DragAndDrop.objectReferences;
                }
                break;
            case EventType.Repaint:
                if (DragAndDrop.visualMode == DragAndDropVisualMode.Copy)
                {
                    EditorGUI.DrawRect(rect, new Color(0f, 1f, 0f, 0.5f));
                }
                break;
            default:
                break;
        }
        return temp;
    }

#endif

}
