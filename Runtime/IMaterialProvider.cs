using UnityEngine;
using System.Collections.Generic;

public interface IMaterialProvider
{
    public IEnumerable<Material> Materials { get; }
}
