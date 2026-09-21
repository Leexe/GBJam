using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PathData
{
	public string Name = "Path 1";
	public List<Vector2Int> Waypoints = new();

	public PathData() { }

	public PathData(string name, List<Vector2Int> waypoints = null)
	{
		Name = name;
		Waypoints = waypoints != null ? new List<Vector2Int>(waypoints) : new List<Vector2Int>();
	}

	public PathData Clone()
	{
		return new PathData(Name, Waypoints);
	}
}
