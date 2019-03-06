#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SelectionInfo
{
    [MenuItem("Tools/Selections/Selection Info &%i", false, 10)]
    public static void ShowInfo()
    {
        Debug.Log(Selection.objects.Length + " objects selected");
    }

    [MenuItem("Tools/Selections/Selection Info", true)]
    public static bool ShowInfoValidator()
    {
        return Selection.objects.Length > 0;
    }

    [MenuItem("Tools/Selections/Clear Selection", false, 20)]
    public static void ClearSelection()
    {
        Selection.activeObject = null;
    }

    [MenuItem("Tools/Selections/Toggle visibility selected objets _&h", false, 30)]
    public static void HideSelectedObjects()
    {
        foreach (GameObject go in Selection.objects)
            go.SetActive(go.activeSelf ? false : true);
    }

    [MenuItem("Tools/Selections/Hide selected objets", true)]
    public static bool HideSelectedObjectsValidator()
    {
        return Selection.objects.Length > 0;
    }
}
#endif
