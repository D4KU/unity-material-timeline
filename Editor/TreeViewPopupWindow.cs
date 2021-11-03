using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;

namespace MaterialTrack
{
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

        const int BORDER = 4;
        const int TOP_PAD = 12;
        const int SEARCH_HEIGHT = 20;
        const int REMAIN_TOP = TOP_PAD + SEARCH_HEIGHT + BORDER;
        var searchRect = new Rect(
            BORDER,
            TOP_PAD,
            rect.width - BORDER * 2,
            SEARCH_HEIGHT);
        var remainingRect = new Rect(
            BORDER,
            TOP_PAD + SEARCH_HEIGHT + BORDER,
            rect.width - BORDER * 2,
            rect.height - REMAIN_TOP - BORDER);

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
}
