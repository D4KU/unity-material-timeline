using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Popup that wraps a TreeView element
/// </summary>
class TreeViewPopupWindow : PopupWindowContent
{
    readonly SearchField searchField;
    readonly TreeView treeView;
    bool shouldClose;

    public float Width { get; set; }

    public TreeViewPopupWindow(TreeView contents)
    {
        searchField = new SearchField();
        treeView = contents;
    }

    public override void OnGUI(Rect rect)
    {
        // Escape closes the window
        if (shouldClose ||
            Event.current.type == EventType.KeyDown &&
            Event.current.keyCode == KeyCode.Escape)
        {
            GUIUtility.hotControl = 0;
            editorWindow.Close();
            GUIUtility.ExitGUI();
        }

        const int border = 4;
        const int topPadding = 12;
        const int searchHeight = 20;
        const int remainTop = topPadding + searchHeight + border;
        var searchRect = new Rect(
            border,
            topPadding,
            rect.width - border * 2,
            searchHeight);
        var remainingRect = new Rect(
            border,
            topPadding + searchHeight + border,
            rect.width - border * 2,
            rect.height - remainTop - border);

        treeView.searchString =
            searchField.OnGUI(searchRect, treeView.searchString);
        treeView.OnGUI(remainingRect);

        if (treeView.HasSelection())
            ForceClose();
    }

    public override Vector2 GetWindowSize()
    {
        var result = base.GetWindowSize();
        result.x = Width;
        return result;
    }

    public override void OnOpen()
    {
        searchField.SetFocus();
        base.OnOpen();
    }

    public void ForceClose() => shouldClose = true;
}
