using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class BMenuEditorWindow : BMenuEditorWindowBase
{
    [MenuItem("Happy/MenuEditorWindow")]
    public static void Open()
    {
        GetWindow<BMenuEditorWindow>();
    }

    protected override BMenuView BuildMenuTree(TreeViewState _treeViewState)
    {
        BMenuView treeView = new BMenuView(_treeViewState);

        treeView.AddMenuItem("1");
        treeView.AddMenuItem("3").contentDrawer = (_rect) => {
            GUILayout.Button("3");
            GUILayout.Button("4");
        };
        treeView.AddMenuItem("3/5");
        treeView.AddMenuItem("2");

        return treeView;
    }
}
