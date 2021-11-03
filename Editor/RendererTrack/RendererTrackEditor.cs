using UnityEngine;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEditor.Timeline;

namespace MaterialTrack
{
[CustomTimelineEditor(typeof(RendererTrack))]
public class RendererTrackEditor : TrackEditor
{
    public override TrackDrawOptions GetTrackOptions(TrackAsset track, Object binding)
    {
        var options = base.GetTrackOptions(track, binding);

        // Give the renderer track a better icon.
        // Since all renderer subclasses contain an eye in their icon,
        // why not use the CanvasRenderer icon, that only consists of an eye?
        var iconContent = EditorGUIUtility.IconContent("CanvasRenderer Icon");
        options.icon = iconContent.image as Texture2D;
        return options;
    }
}
}
