using System.Collections.Generic;
using UnityEngine;

public interface IPathfinder
{
    List<Vector3> FindPath(Vector3 start, Vector3 goal);
}