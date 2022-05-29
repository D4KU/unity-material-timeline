using UnityEngine;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEditor.Timeline;

namespace MaterialTrack
{
[CustomTimelineEditor(typeof(SlotTrack))]
public class SlotTrackEditor : TrackEditor
{
    public override TrackDrawOptions GetTrackOptions(TrackAsset track, Object binding)
    {
        var options = base.GetTrackOptions(track, binding);
        var iconContent = EditorGUIUtility.IconContent("Material Icon");
        options.icon = iconContent.image as Texture2D;
        return options;
    }
}
}
