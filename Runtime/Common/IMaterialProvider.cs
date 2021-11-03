using UnityEngine;
using System.Collections.Generic;

namespace MaterialTrack
{
/// <summary>
/// An object bound to a track found in this package must be able to
/// provide one ore more materials to operate on.
/// </summary>
public interface IMaterialProvider
{
    /// <summary>
    /// The material(s) the timeline track operates on
    /// </summary>
    public IEnumerable<Material> Materials { get; }
}
}
