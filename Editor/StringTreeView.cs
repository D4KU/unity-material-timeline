using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System;

namespace MaterialTrack
{
/// <summary>
/// TreeView to show a list of strings and call a callback when one of them
/// is selected.
/// </summary>
public class StringTreeView : TreeView
{
    readonly IEnumerable<string> entries;
    readonly Action<string> onSelectionChanged;

    public StringTreeView(
        IEnumerable<string> entries,
        Action<string> onSelectionChanged)
        : base(new TreeViewState())
    {
        this.entries = entries;
        this.onSelectionChanged = onSelectionChanged;

        showAlternatingRowBackgrounds = true;
        showBorder = true;
        Reload();
    }

    // Called every time Reload is called to ensure that TreeViewItems
    // are created from data
    protected override TreeViewItem BuildRoot()
    {
        // IDs should be unique. The root item is required to have a
        // depth of -1, and the rest of the items increment from that.
        var children = new List<TreeViewItem>();
        var root = new TreeViewItem(id: 0, depth: -1, displayName: "Root");

        int id = 1;
        foreach (string s in entries)
            children.Add(new TreeViewItem(id: id++, depth: 0, displayName: s));

        SetupParentsAndChildrenFromDepths(root, children);
        return root;
    }

    protected override void SingleClickedItem(int id)
    {
        TreeViewItem item = FindItem(id, rootItem);
        onSelectionChanged(item.displayName);
    }
}
}
