using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : OdinEditor
{
	public enum GridEditMode
	{
		None,
		DrawPath,
		PaintObstacles,
		Erase,
	}

	private static GridEditMode _activeMode = GridEditMode.None;
	private static int _activePathIndex = 0;
	private Vector2Int _lastInteractedCell = new(-999, -999);

	protected override void OnEnable()
	{
		base.OnEnable();
		_lastInteractedCell = new Vector2Int(-999, -999);
	}

	public override void OnInspectorGUI()
	{
		var grid = (GridManager)target;
		grid.EnsurePathsInitialized();
		_activePathIndex = Mathf.Clamp(_activePathIndex, 0, grid.PathCount - 1);

		DrawToolToolbar(grid);

		EditorGUILayout.Space(8);
		DrawPathSelectorSection(grid);

		EditorGUILayout.Space(8);
		DrawLevelSyncSection(grid);

		EditorGUILayout.Space(8);
		DrawQuickActions(grid);

		EditorGUILayout.Space(12);
		base.OnInspectorGUI();
	}

	private void DrawToolToolbar(GridManager grid)
	{
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		EditorGUILayout.LabelField("Grid Drawing Tools", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();

		GUI.backgroundColor = _activeMode == GridEditMode.None ? new Color(0.4f, 0.8f, 1f) : Color.white;
		if (GUILayout.Button("View / Select", GUILayout.Height(30)))
		{
			_activeMode = GridEditMode.None;
			SceneView.RepaintAll();
		}

		GUI.backgroundColor = _activeMode == GridEditMode.DrawPath ? new Color(1f, 0.9f, 0.2f) : Color.white;
		if (GUILayout.Button("Draw Path", GUILayout.Height(30)))
		{
			_activeMode = GridEditMode.DrawPath;
			SceneView.RepaintAll();
		}

		GUI.backgroundColor = _activeMode == GridEditMode.PaintObstacles ? new Color(1f, 0.5f, 0.2f) : Color.white;
		if (GUILayout.Button("Paint Obstacles", GUILayout.Height(30)))
		{
			_activeMode = GridEditMode.PaintObstacles;
			SceneView.RepaintAll();
		}

		GUI.backgroundColor = _activeMode == GridEditMode.Erase ? new Color(1f, 0.35f, 0.35f) : Color.white;
		if (GUILayout.Button("Erase", GUILayout.Height(30)))
		{
			_activeMode = GridEditMode.Erase;
			SceneView.RepaintAll();
		}

		GUI.backgroundColor = Color.white;
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space(4);
		string helpText = _activeMode switch
		{
			GridEditMode.DrawPath =>
				$"• Currently editing: {grid.Paths[_activePathIndex].Name} (Path {_activePathIndex + 1})\n• Click tiles in Scene View to add Waypoints.\n• Drag numbered waypoint handles to move.\n• Right-click or Shift+Click to delete waypoint.",
			GridEditMode.PaintObstacles =>
				"• Left-Click / Drag in Scene View to paint Obstacles.\n• Right-Click or Shift+Click / Drag to erase Obstacles.",
			GridEditMode.Erase => "• Left-Click / Drag in Scene View to remove any obstacle or waypoint.",
			_ => "Select a tool above to start drawing in the Scene View. Press Esc to exit painting.",
		};
		EditorGUILayout.HelpBox(helpText, MessageType.Info);

		EditorGUILayout.EndVertical();
	}

	private void DrawPathSelectorSection(GridManager grid)
	{
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		EditorGUILayout.LabelField("Path Management", EditorStyles.boldLabel);

		string[] pathNames = new string[grid.PathCount];
		for (int i = 0; i < grid.PathCount; i++)
		{
			pathNames[i] = $"{grid.Paths[i].Name} ({grid.Paths[i].Waypoints.Count})";
		}

		int newSelection = GUILayout.Toolbar(_activePathIndex, pathNames);
		if (newSelection != _activePathIndex)
		{
			_activePathIndex = newSelection;
			SceneView.RepaintAll();
		}

		EditorGUILayout.Space(4);
		PathData currentPath = grid.Paths[_activePathIndex];
		EditorGUI.BeginChangeCheck();
		string updatedName = EditorGUILayout.TextField("Active Path Name", currentPath.Name);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(grid, "Rename Path");
			currentPath.Name = updatedName;
			EditorUtility.SetDirty(grid);
		}

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("+ Add New Path", GUILayout.Height(24)))
		{
			Undo.RecordObject(grid, "Add Path");
			_activePathIndex = grid.AddPath();
			EditorUtility.SetDirty(grid);
			SceneView.RepaintAll();
		}

		EditorGUI.BeginDisabledGroup(grid.PathCount <= 1);
		if (GUILayout.Button("Delete Current Path", GUILayout.Height(24)))
		{
			Undo.RecordObject(grid, "Delete Path");
			grid.RemovePath(_activePathIndex);
			_activePathIndex = Mathf.Clamp(_activePathIndex, 0, grid.PathCount - 1);
			EditorUtility.SetDirty(grid);
			SceneView.RepaintAll();
		}
		EditorGUI.EndDisabledGroup();

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
	}

	private void DrawLevelSyncSection(GridManager grid)
	{
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		EditorGUILayout.LabelField("Level ScriptableObject Sync", EditorStyles.boldLabel);

		EditorGUI.BeginChangeCheck();
		var targetLevel = (LevelSO)
			EditorGUILayout.ObjectField("Target Level SO", grid.TargetLevelSO, typeof(LevelSO), false);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(grid, "Change Target Level");
			grid.TargetLevelSO = targetLevel;
			EditorUtility.SetDirty(grid);
		}

		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginDisabledGroup(grid.TargetLevelSO == null);

		if (GUILayout.Button("Load from Level SO", GUILayout.Height(24)))
		{
			Undo.RecordObject(grid, "Load From Level");
			grid.LoadFromLevel(grid.TargetLevelSO);
			_activePathIndex = Mathf.Clamp(_activePathIndex, 0, grid.PathCount - 1);
			EditorUtility.SetDirty(grid);
			SceneView.RepaintAll();
		}

		if (GUILayout.Button("Save to Level SO", GUILayout.Height(24)))
		{
			Undo.RecordObject(grid.TargetLevelSO, "Save To Level");
			grid.SaveToLevel(grid.TargetLevelSO);
			EditorUtility.SetDirty(grid.TargetLevelSO);
			AssetDatabase.SaveAssets();
		}

		EditorGUI.EndDisabledGroup();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();
	}

	private void DrawQuickActions(GridManager grid)
	{
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		EditorGUILayout.LabelField("Quick Actions & Stats", EditorStyles.boldLabel);

		PathData currentPath = grid.Paths[_activePathIndex];
		int pathTilesCount = GridManager.CalculatePathTiles(currentPath.Waypoints).Count;
		EditorGUILayout.LabelField(
			$"Paths: {grid.PathCount}  |  Active: {currentPath.Name} ({currentPath.Waypoints.Count} WPs, {pathTilesCount} Tiles)  |  Obs: {grid.Obstacles.Count}"
		);

		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Clear Active Path", GUILayout.Height(22)))
		{
			Undo.RecordObject(grid, "Clear Active Path");
			grid.ClearWaypoints(_activePathIndex);
			EditorUtility.SetDirty(grid);
			SceneView.RepaintAll();
		}

		if (GUILayout.Button("Reverse Active Path", GUILayout.Height(22)))
		{
			Undo.RecordObject(grid, "Reverse Active Path");
			grid.ReverseWaypoints(_activePathIndex);
			EditorUtility.SetDirty(grid);
			SceneView.RepaintAll();
		}

		if (GUILayout.Button("Clear Obstacles", GUILayout.Height(22)))
		{
			Undo.RecordObject(grid, "Clear Obstacles");
			grid.ClearObstacles();
			EditorUtility.SetDirty(grid);
			SceneView.RepaintAll();
		}

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
	}

	private void OnSceneGUI()
	{
		var grid = (GridManager)target;
		grid.EnsurePathsInitialized();
		_activePathIndex = Mathf.Clamp(_activePathIndex, 0, grid.PathCount - 1);

		DrawGridOutlines(grid);
		DrawPaths(grid);
		DrawObstacles(grid);
		DrawWaypointHandles(grid);
		DrawSceneOverlay(grid);
		HandleSceneInput(grid);
	}

	private void DrawGridOutlines(GridManager grid)
	{
		var prevZTest = Handles.zTest;
		Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
		Handles.color = new Color(0.3f, 0.8f, 0.4f, 0.2f);
		for (int x = 0; x < grid.GetMaxColumns; x++)
		{
			for (int y = 0; y < grid.GetMaxRows; y++)
			{
				Vector3 center = grid.GridToWorld(x, y);
				center.z = -0.01f;
				Handles.DrawWireCube(center, new Vector3(grid.CellSize, grid.CellSize, 0.001f));
			}
		}
		Handles.zTest = prevZTest;
	}

	private void DrawObstacles(GridManager grid)
	{
		if (grid.Obstacles.Count == 0)
		{
			return;
		}

		var prevZTest = Handles.zTest;
		Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

		Color fillColor = new Color(0.95f, 0.35f, 0.15f, 0.7f);
		Color borderColor = new Color(1f, 0.5f, 0.2f, 1f);
		Color xColor = new Color(1f, 0.2f, 0.1f, 0.95f);

		for (int i = 0; i < grid.Obstacles.Count; i++)
		{
			Vector2Int obs = grid.Obstacles[i];
			Vector3 center = grid.GridToWorld(obs);
			center.z = -0.05f;

			Vector3[] corners = GetCellCorners(center, grid.CellSize * 0.94f);
			Handles.DrawSolidRectangleWithOutline(corners, fillColor, borderColor);

			float d = grid.CellSize * 0.3f;
			Handles.color = xColor;
			Handles.DrawLine(new Vector3(center.x - d, center.y - d, center.z), new Vector3(center.x + d, center.y + d, center.z), 2.5f);
			Handles.DrawLine(new Vector3(center.x - d, center.y + d, center.z), new Vector3(center.x + d, center.y - d, center.z), 2.5f);
		}

		Handles.zTest = prevZTest;
	}

	private void DrawPaths(GridManager grid)
	{
		var prevZTest = Handles.zTest;
		Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

		for (int p = 0; p < grid.Paths.Count; p++)
		{
			PathData path = grid.Paths[p];
			if (path.Waypoints.Count == 0)
			{
				continue;
			}

			bool isActive = (p == _activePathIndex);
			Color pColor = GridManager.GetPathColor(p);
			float fillAlpha = isActive ? 0.55f : 0.22f;
			float borderAlpha = isActive ? 0.95f : 0.45f;
			Color pathFill = new Color(pColor.r, pColor.g, pColor.b, fillAlpha);
			Color pathBorder = new Color(pColor.r, pColor.g, pColor.b, borderAlpha);

			List<Vector2Int> pathTiles = GridManager.CalculatePathTiles(path.Waypoints);
			for (int i = 0; i < pathTiles.Count; i++)
			{
				Vector3 center = grid.GridToWorld(pathTiles[i]);
				center.z = -0.03f;
				Vector3[] corners = GetCellCorners(center, grid.CellSize * 0.92f);
				Handles.DrawSolidRectangleWithOutline(corners, pathFill, pathBorder);
			}

			// Connecting lines
			if (path.Waypoints.Count > 1)
			{
				Handles.color = new Color(pColor.r, pColor.g, pColor.b, isActive ? 1f : 0.5f);
				float lineWidth = isActive ? 3.5f : 1.5f;
				for (int i = 0; i < path.Waypoints.Count - 1; i++)
				{
					Vector3 p1 = grid.GridToWorld(path.Waypoints[i]);
					Vector3 p2 = grid.GridToWorld(path.Waypoints[i + 1]);
					p1.z = -0.06f;
					p2.z = -0.06f;
					Handles.DrawLine(p1, p2, lineWidth);
				}

				// Goal
				Vector3 goalCenter = grid.GridToWorld(path.Waypoints[^1]);
				goalCenter.z = -0.04f;
				Handles.DrawSolidRectangleWithOutline(
					GetCellCorners(goalCenter, grid.CellSize * 0.96f),
					new Color(0.95f, 0.2f, 0.35f, isActive ? 0.85f : 0.4f),
					Color.red
				);
			}

			// Spawn
			Vector3 spawnCenter = grid.GridToWorld(path.Waypoints[0]);
			spawnCenter.z = -0.04f;
			Handles.DrawSolidRectangleWithOutline(
				GetCellCorners(spawnCenter, grid.CellSize * 0.96f),
				new Color(0.2f, 0.9f, 0.3f, isActive ? 0.85f : 0.4f),
				Color.green
			);
		}

		Handles.zTest = prevZTest;
	}

	private void DrawWaypointHandles(GridManager grid)
	{
		PathData currentPath = grid.Paths[_activePathIndex];
		if (currentPath.Waypoints.Count == 0)
		{
			return;
		}

		Color pColor = GridManager.GetPathColor(_activePathIndex);

		for (int i = 0; i < currentPath.Waypoints.Count; i++)
		{
			Vector2Int wp = currentPath.Waypoints[i];
			Vector3 wpWorld = grid.GridToWorld(wp);
			wpWorld.z = -0.07f;
			float handleSize = HandleUtility.GetHandleSize(wpWorld) * 0.14f;

			Color handleColor =
				i == 0
					? new Color(0.2f, 1f, 0.3f, 0.95f)
					: (
						i == currentPath.Waypoints.Count - 1
							? new Color(1f, 0.25f, 0.25f, 0.95f)
							: new Color(pColor.r, pColor.g, pColor.b, 0.95f)
					);
			Handles.color = handleColor;

			EditorGUI.BeginChangeCheck();
			Vector3 newPos = Handles.FreeMoveHandle(wpWorld, handleSize, Vector3.zero, Handles.CircleHandleCap);

			if (EditorGUI.EndChangeCheck())
			{
				Vector2Int newCell = grid.WorldToGrid(newPos);
				if (grid.IsValidGridPos(newCell) && newCell != wp)
				{
					Undo.RecordObject(grid, "Move Waypoint");
					grid.SetWaypoint(i, newCell, _activePathIndex);
					EditorUtility.SetDirty(grid);
				}
			}

			string label =
				i == 0
					? $"S{_activePathIndex + 1}"
					: (i == currentPath.Waypoints.Count - 1 ? $"G{_activePathIndex + 1}" : $"{i}");
			var labelStyle = new GUIStyle
			{
				normal = { textColor = handleColor },
				fontStyle = FontStyle.Bold,
				fontSize = 11,
				alignment = TextAnchor.LowerCenter,
			};
			Handles.Label(wpWorld + new Vector3(0, grid.CellSize * 0.45f, 0), label, labelStyle);
		}
	}

	private void DrawSceneOverlay(GridManager grid)
	{
		Handles.BeginGUI();

		GUILayout.BeginArea(new Rect(12, 12, 230, 190), GUI.skin.box);
		EditorGUILayout.LabelField("Grid Manager Tools", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();
		GUI.backgroundColor = _activeMode == GridEditMode.None ? new Color(0.4f, 0.8f, 1f) : Color.white;
		if (GUILayout.Button("View", GUILayout.Height(24)))
		{
			_activeMode = GridEditMode.None;
		}

		GUI.backgroundColor = _activeMode == GridEditMode.DrawPath ? new Color(1f, 0.9f, 0.2f) : Color.white;
		if (GUILayout.Button("Path", GUILayout.Height(24)))
		{
			_activeMode = GridEditMode.DrawPath;
		}

		GUI.backgroundColor = _activeMode == GridEditMode.PaintObstacles ? new Color(1f, 0.5f, 0.2f) : Color.white;
		if (GUILayout.Button("Obstacle", GUILayout.Height(24)))
		{
			_activeMode = GridEditMode.PaintObstacles;
		}

		GUI.backgroundColor = _activeMode == GridEditMode.Erase ? new Color(1f, 0.35f, 0.35f) : Color.white;
		if (GUILayout.Button("Erase", GUILayout.Height(24)))
		{
			_activeMode = GridEditMode.Erase;
		}
		GUI.backgroundColor = Color.white;
		EditorGUILayout.EndHorizontal();

		// Path selection row
		EditorGUILayout.Space(2);
		EditorGUILayout.BeginHorizontal();
		GUI.backgroundColor = GridManager.GetPathColor(_activePathIndex);
		if (GUILayout.Button("<", EditorStyles.miniButtonLeft, GUILayout.Width(22)))
		{
			_activePathIndex = Mathf.Max(0, _activePathIndex - 1);
		}
		if (GUILayout.Button($"{grid.Paths[_activePathIndex].Name}", EditorStyles.miniButtonMid))
		{
			_activePathIndex = (_activePathIndex + 1) % grid.PathCount;
		}
		if (GUILayout.Button(">", EditorStyles.miniButtonRight, GUILayout.Width(22)))
		{
			_activePathIndex = Mathf.Min(grid.PathCount - 1, _activePathIndex + 1);
		}
		GUI.backgroundColor = Color.white;

		if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.Width(22)))
		{
			Undo.RecordObject(grid, "Add Path");
			_activePathIndex = grid.AddPath();
			EditorUtility.SetDirty(grid);
		}
		if (GUILayout.Button("x", EditorStyles.miniButtonRight, GUILayout.Width(22)) && grid.PathCount > 1)
		{
			Undo.RecordObject(grid, "Delete Path");
			grid.RemovePath(_activePathIndex);
			_activePathIndex = Mathf.Clamp(_activePathIndex, 0, grid.PathCount - 1);
			EditorUtility.SetDirty(grid);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space(2);
		PathData currentPath = grid.Paths[_activePathIndex];
		int pathTilesCount = GridManager.CalculatePathTiles(currentPath.Waypoints).Count;
		EditorGUILayout.LabelField(
			$"Active: {currentPath.Name} ({currentPath.Waypoints.Count} WPs, {pathTilesCount} Tiles) | Obs: {grid.Obstacles.Count}",
			EditorStyles.miniLabel
		);

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Clear Path", EditorStyles.miniButton))
		{
			Undo.RecordObject(grid, "Clear Path");
			grid.ClearWaypoints(_activePathIndex);
			EditorUtility.SetDirty(grid);
		}
		if (GUILayout.Button("Clear Obs", EditorStyles.miniButton))
		{
			Undo.RecordObject(grid, "Clear Obstacles");
			grid.ClearObstacles();
			EditorUtility.SetDirty(grid);
		}
		EditorGUILayout.EndHorizontal();

		if (grid.TargetLevelSO != null)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Load Level", EditorStyles.miniButton))
			{
				Undo.RecordObject(grid, "Load Level");
				grid.LoadFromLevel(grid.TargetLevelSO);
				_activePathIndex = Mathf.Clamp(_activePathIndex, 0, grid.PathCount - 1);
				EditorUtility.SetDirty(grid);
			}
			if (GUILayout.Button("Save Level", EditorStyles.miniButton))
			{
				Undo.RecordObject(grid.TargetLevelSO, "Save Level");
				grid.SaveToLevel(grid.TargetLevelSO);
			}
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.LabelField("Esc to exit paint mode", EditorStyles.centeredGreyMiniLabel);
		GUILayout.EndArea();

		Handles.EndGUI();
	}

	private void HandleSceneInput(GridManager grid)
	{
		Event e = Event.current;

		if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
		{
			_activeMode = GridEditMode.None;
			e.Use();
			SceneView.RepaintAll();
			return;
		}

		Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
		var gridPlane = new Plane(Vector3.forward, new Vector3(0, 0, 0));
		if (!gridPlane.Raycast(ray, out float enterDist))
		{
			return;
		}

		Vector3 worldHit = ray.GetPoint(enterDist);
		Vector2Int hoveredCell = grid.WorldToGrid(worldHit);
		bool isInsideGrid = grid.IsValidGridPos(hoveredCell);

		// Draw Hover Highlight
		if (isInsideGrid)
		{
			Vector3 cellCenter = grid.GridToWorld(hoveredCell);
			cellCenter.z = -0.08f;
			Color pColor = GridManager.GetPathColor(_activePathIndex);
			Color fillColor = _activeMode switch
			{
				GridEditMode.DrawPath => new Color(pColor.r, pColor.g, pColor.b, 0.45f),
				GridEditMode.PaintObstacles => new Color(1f, 0.45f, 0.2f, 0.5f),
				GridEditMode.Erase => new Color(1f, 0.2f, 0.2f, 0.5f),
				_ => new Color(1f, 1f, 1f, 0.25f),
			};
			Color outlineColor = _activeMode == GridEditMode.None ? Color.white : fillColor;
			outlineColor.a = 0.9f;

			Handles.DrawSolidRectangleWithOutline(GetCellCorners(cellCenter, grid.CellSize), fillColor, outlineColor);

			var coordStyle = new GUIStyle
			{
				normal = { textColor = Color.white },
				fontSize = 9,
				alignment = TextAnchor.MiddleCenter,
			};
			Handles.Label(cellCenter, $"({hoveredCell.x},{hoveredCell.y})", coordStyle);
			SceneView.RepaintAll();
		}

		if (_activeMode == GridEditMode.None)
		{
			return;
		}

		// Prevent Unity default scene selection while active in drawing mode
		int controlId = GUIUtility.GetControlID(FocusType.Passive);
		HandleUtility.AddDefaultControl(controlId);

		// Mouse Input
		if (e.type == EventType.MouseUp)
		{
			_lastInteractedCell = new Vector2Int(-999, -999);
		}

		if (!isInsideGrid)
		{
			return;
		}

		if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
		{
			if (e.button == 0) // Left Click
			{
				if (_activeMode == GridEditMode.PaintObstacles)
				{
					if (hoveredCell != _lastInteractedCell)
					{
						_lastInteractedCell = hoveredCell;
						Undo.RecordObject(grid, e.shift ? "Remove Obstacle" : "Add Obstacle");
						if (e.shift)
						{
							grid.RemoveObstacle(hoveredCell);
						}
						else
						{
							grid.AddObstacle(hoveredCell);
						}
						EditorUtility.SetDirty(grid);
					}
					e.Use();
				}
				else if (_activeMode == GridEditMode.DrawPath)
				{
					if (e.type == EventType.MouseDown)
					{
						Undo.RecordObject(grid, "Add Waypoint");
						grid.AddWaypoint(hoveredCell, _activePathIndex);
						EditorUtility.SetDirty(grid);
						e.Use();
					}
				}
				else if (_activeMode == GridEditMode.Erase)
				{
					if (hoveredCell != _lastInteractedCell)
					{
						_lastInteractedCell = hoveredCell;
						Undo.RecordObject(grid, "Erase Cell");
						grid.RemoveObstacle(hoveredCell);
						grid.RemoveWaypointAtPos(hoveredCell, _activePathIndex);
						EditorUtility.SetDirty(grid);
					}
					e.Use();
				}
			}
			else if (e.button == 1) // Right Click (Erase / Delete)
			{
				if (_activeMode == GridEditMode.DrawPath)
				{
					if (e.type == EventType.MouseDown)
					{
						Undo.RecordObject(grid, "Delete Waypoint");
						PathData path = grid.Paths[_activePathIndex];
						if (!grid.RemoveWaypointAtPos(hoveredCell, _activePathIndex) && path.Waypoints.Count > 0)
						{
							grid.RemoveWaypoint(path.Waypoints.Count - 1, _activePathIndex);
						}
						EditorUtility.SetDirty(grid);
						e.Use();
					}
				}
				else if (_activeMode == GridEditMode.PaintObstacles)
				{
					if (hoveredCell != _lastInteractedCell)
					{
						_lastInteractedCell = hoveredCell;
						Undo.RecordObject(grid, "Remove Obstacle");
						grid.RemoveObstacle(hoveredCell);
						EditorUtility.SetDirty(grid);
					}
					e.Use();
				}
			}
		}
	}

	private static Vector3[] GetCellCorners(Vector3 center, float size)
	{
		float half = size * 0.5f;
		return new Vector3[]
		{
			new(center.x - half, center.y - half, center.z),
			new(center.x - half, center.y + half, center.z),
			new(center.x + half, center.y + half, center.z),
			new(center.x + half, center.y - half, center.z),
		};
	}
}
