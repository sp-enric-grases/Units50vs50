using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityObject = UnityEngine.Object;

public class TransformTreeView : TreeView
{
    public GameObject rootGo;
    public BakeAnimationConfigureScriptableObject configureFile;
    public TransformTreeView(TreeViewState state, GameObject rootGo, BakeAnimationConfigureScriptableObject configureFile)
        : base(state)
    {
        this.rootGo = rootGo;
        this.configureFile = configureFile;
        Reload();
    }

    protected override TreeViewItem BuildRoot()
    {
        return new TreeViewItem { id = 0, depth = -1 };
    }

    protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
    {
        IList<TreeViewItem> rows = GetRows();
        if (rows == null)
        {
            rows = new List<TreeViewItem>(200);
        }
        // We use the GameObject instanceIDs as ids for items as we want to 
        // select the game objects and not the transform components.
        rows.Clear();

        TreeViewItem item = CreateTreeViewItemForGameObject(rootGo);
        root.AddChild(item);
        rows.Add(item);
        if (rootGo.transform.childCount > 0)
        {
            if (IsExpanded(item.id))
            {
                AddChildrenRecursive(rootGo, item, rows);
            }
            else
            {
                item.children = CreateChildListForCollapsedParent();
            }
        }

        SetupDepthsFromParentsAndChildren(root);
        return rows;
    }

    void AddChildrenRecursive(GameObject go, TreeViewItem item, IList<TreeViewItem> rows)
    {
        int childCount = go.transform.childCount;

        item.children = new List<TreeViewItem>(childCount);
        for (int i = 0; i < childCount; i++)
        {
            Transform childTransform = go.transform.GetChild(i);
            TreeViewItem childItem = CreateTreeViewItemForGameObject(childTransform.gameObject);
            item.AddChild(childItem);
            rows.Add(childItem);

            if (childTransform.childCount > 0)
            {
                if (IsExpanded(childItem.id))
                {
                    AddChildrenRecursive(childTransform.gameObject, childItem, rows);
                }
                else
                {
                    childItem.children = CreateChildListForCollapsedParent();
                }
            }
        }
    }

    static TreeViewItem CreateTreeViewItemForGameObject(GameObject gameObject)
    {
        // We can use the GameObject instanceID for TreeViewItem id, as it ensured to be unique among other items in the tree.
        // To optimize reload time we could delay fetching the transform.name until it used for rendering (prevents allocating strings 
        // for items not rendered in large trees)
        // We just set depth to -1 here and then call SetupDepthsFromParentsAndChildren at the end of BuildRootAndRows to set the depths.
        return new TreeViewItem(gameObject.GetInstanceID(), -1, gameObject.name);
    }

    protected override IList<int> GetAncestors(int id)
    {
        // The backend needs to provide us with this info since the item with id
        // may not be present in the rows
        Transform transform = GetGameObject(id).transform;

        List<int> ancestors = new List<int>();
        while (transform.parent != null)
        {
            ancestors.Add(transform.parent.gameObject.GetInstanceID());
            transform = transform.parent;
        }

        return ancestors;
    }

    protected override IList<int> GetDescendantsThatHaveChildren(int id)
    {
        Stack<Transform> stack = new Stack<Transform>();

        Transform start = GetGameObject(id).transform;
        stack.Push(start);

        List<int> parents = new List<int>();
        while (stack.Count > 0)
        {
            Transform current = stack.Pop();
            parents.Add(current.gameObject.GetInstanceID());
            for (int i = 0; i < current.childCount; ++i)
            {
                if (current.childCount > 0)
                {
                    stack.Push(current.GetChild(i));
                }

            }
        }

        return parents;
    }

    GameObject GetGameObject(int instanceID)
    {
        return (GameObject)EditorUtility.InstanceIDToObject(instanceID);
    }

    // Custom GUI
    protected override void RowGUI(RowGUIArgs args)
    {
        Event evt = Event.current;
        extraSpaceBeforeIconAndLabel = 18.0f;


        GameObject gameObject = GetGameObject(args.item.id);

        Rect toggleRect = args.rowRect;
        toggleRect.x = toggleRect.x + GetContentIndent(args.item);
        toggleRect.width = 16.0f;

        // Ensure row is selected before using the toggle (usability)
        if (evt.type == EventType.MouseDown && toggleRect.Contains(evt.mousePosition))
        {
            SelectionClick(args.item, false);
        }


        EditorGUI.BeginChangeCheck();

        bool flag = false;

        if (gameObject == configureFile.rootBoneGameObject)
        {
            flag = true;
        }
        else
        {
            flag = false;
        }

        EditorGUI.Toggle(toggleRect, flag);

        if (EditorGUI.EndChangeCheck())
        {
            configureFile.rootBoneGameObject = gameObject;
        }

        // Text
        base.RowGUI(args);
    }

    // Selection
    protected override void SelectionChanged(IList<int> selectedIds)
    {
        Selection.instanceIDs = selectedIds.ToArray();
    }


}

