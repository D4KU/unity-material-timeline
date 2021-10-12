using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// This base class exists because a <see cref="PropertyDrawer"/> can't
/// serialize generic types
/// </summary>
public abstract class MaterialClipBase : PlayableAsset {}

public abstract class MaterialClip<T> : MaterialClipBase where T : class, IPlayableBehaviour, new()
{
    public T data = new T();

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        => ScriptPlayable<T>.Create(graph, data);
}
