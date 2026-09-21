using System;
using System.Collections.Generic;
using UnityEngine;

public enum GridType
{
	Empty, // Can Place Towers
	Spawn, // Enemy Spawn
	Path, // Enemy Walk Down This
	Tower, // A Tower Is On This Node
	Obstacle, // Decorational Node (Can't Place Towers)
	Goal, // Enemy Goal
}

public class GridNode
{
	public Vector2Int Position;
	public GridType Type;
	public GameObject PlacedTower;
	public Tower Tower;

	public GridNode(Vector2Int position, GridType type, GameObject placedTower, Tower tower = null)
	{
		Position = position;
		Type = type;
		PlacedTower = placedTower;
		Tower = tower;
	}

	public bool CanPlaceTower => PlacedTower == null && Type == GridType.Empty;
}

public class GridManager : MonoSingleton<GridManager>
{
	[Header("Paths")]
	[SerializeField]
	private List<PathData> _paths = new();

	[SerializeField]
	private List<Vector2Int> _obstacles = new();

	[Header("Settings")]
	[SerializeField]
	private Vector2 _originPosition;

	[SerializeField]
	private float _cellSize = 0.5f;

	[Header("Level Data Target")]
	[SerializeField]
	private LevelSO _targetLevelSO;

	[Header("Debug Visualization")]
	[SerializeField]
	private bool _debugVisualization = true;

	[SerializeField]
	private bool _showCoordinateLabels = false;

	private const int Rows = 16;
	private const int Columns = 20;
	public int GetMaxRows => Rows;
	public int GetMaxColumns => Columns;
	public float CellSize => _cellSize;
	public Vector2 OriginPosition => _originPosition;
	public LevelSO TargetLevelSO
	{
		get => _targetLevelSO;
		set => _targetLevelSO = value;
	}

	public List<PathData> Paths => _paths;
	public int PathCount
	{
		get
		{
			EnsurePathsInitialized();
			return _paths.Count;
		}
	}

	private readonly GridNode[] _grid = new GridNode[Columns * Rows];
	public List<Vector2Int> EnemyWaypoints => GetPath(0);
	public List<Vector2Int> Obstacles => _obstacles;
	public float TotalPathDistance => GetPathDistance(0);

	private readonly List<float> _pathDistances = new();

	private int ToIndex(int x, int y) => x + (y * Columns);

	private int ToIndex(Vector2Int pos) => pos.x + (pos.y * Columns);

	// Events
	[HideInInspector]
	public Action<Vector2Int, GridType> OnTileStateChanged;

	[HideInInspector]
	public Action<Vector2Int, GameObject> OnTowerPlaced;

	[HideInInspector]
	public Action<Vector2Int> OnTowerRemoved;

	public Action<Tower> OnTowerSpawned;
	public Action<Tower> OnTowerDespawned;

	private readonly List<Tower> _activeTowers = new();
	public IReadOnlyList<Tower> ActiveTowers => _activeTowers;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		InitializeGrid();
	}

	private void InitializeGrid()
	{
		for (int x = 0; x < Columns; x++)
		{
			for (int y = 0; y < Rows; y++)
			{
				_grid[ToIndex(x, y)] = new GridNode(new Vector2Int(x, y), GridType.Empty, null);
			}
		}

		LevelSO level = GameManager.SelectedLevel != null ? GameManager.SelectedLevel : GameManager.Instance.Level;
		if (level != null)
		{
			if (level.Paths.Count > 0)
			{
				_paths = new List<PathData>();
				for (int i = 0; i < level.Paths.Count; i++)
				{
					_paths.Add(level.Paths[i].Clone());
				}
			}
			_obstacles = new List<Vector2Int>(level.Obstacles);
		}

		EnsurePathsInitialized();

		foreach (Vector2Int t in _obstacles)
		{
			if (IsValidGridPos(t))
			{
				SetGridType(t, GridType.Obstacle);
			}
		}

		InitializePath();
	}

	public void EnsurePathsInitialized()
	{
		_paths ??= new List<PathData>();
		if (_paths.Count == 0)
		{
			_paths.Add(new PathData("Path 1"));
		}
	}

	public List<Vector2Int> GetPath(int index = 0)
	{
		EnsurePathsInitialized();
		index = Mathf.Clamp(index, 0, _paths.Count - 1);
		return _paths[index].Waypoints;
	}

	public float GetPathDistance(int index = 0)
	{
		if (_pathDistances != null && _pathDistances.Count > 0)
		{
			index = Mathf.Clamp(index, 0, _pathDistances.Count - 1);
			return _pathDistances[index];
		}
		return 0f;
	}

	public int AddPath(string name = null)
	{
		EnsurePathsInitialized();
		string pathName = string.IsNullOrEmpty(name) ? $"Path {_paths.Count + 1}" : name;
		_paths.Add(new PathData(pathName));
		return _paths.Count - 1;
	}

	public bool RemovePath(int index)
	{
		EnsurePathsInitialized();
		if (index < 0 || index >= _paths.Count || _paths.Count <= 1)
		{
			return false;
		}
		_paths.RemoveAt(index);
		return true;
	}

	private void InitializePath()
	{
		EnsurePathsInitialized();
		_pathDistances.Clear();

		for (int p = 0; p < _paths.Count; p++)
		{
			List<Vector2Int> waypoints = _paths[p].Waypoints;
			if (waypoints.Count <= 1)
			{
				_pathDistances.Add(0f);
				continue;
			}

			float dist = 0f;
			for (int i = 0; i < waypoints.Count - 1; i++)
			{
				dist += Vector3.Distance(GridToWorld(waypoints[i]), GridToWorld(waypoints[i + 1]));
			}
			_pathDistances.Add(dist);

			List<Vector2Int> pathTiles = CalculatePathTiles(waypoints);
			for (int i = 0; i < pathTiles.Count; i++)
			{
				SetGridType(pathTiles[i], GridType.Path);
			}

			// Mark Spawn & Goal Point
			SetGridType(waypoints[0], GridType.Spawn);
			SetGridType(waypoints[^1], GridType.Goal);
		}
	}

	public bool CanPlaceTower(Vector2Int position, int width = 2, int height = 2)
	{
		if (!IsValidGridArea(position.x, position.y, width, height))
		{
			return false;
		}

		for (int x = position.x; x < position.x + width; x++)
		{
			for (int y = position.y; y < position.y + height; y++)
			{
				if (!_grid[ToIndex(x, y)].CanPlaceTower)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool PlaceTower(Vector2Int position, TowerSO towerSO, int width = 2, int height = 2)
	{
		if (!CanPlaceTower(position, width, height))
		{
			return false;
		}

		Vector3 worldPos = GridToWorld(position, width, height);
		Tower tower = TowerPool.Instance.Get(worldPos, towerSO);

		for (int x = position.x; x < position.x + width; x++)
		{
			for (int y = position.y; y < position.y + height; y++)
			{
				SetGridType(x, y, GridType.Tower);
				SetGridTower(x, y, tower);
			}
		}

		_activeTowers.Add(tower);
		OnTowerPlaced?.Invoke(position, tower.gameObject);
		OnTowerSpawned?.Invoke(tower);
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.PickUp_Sfx);
		return true;
	}

	public bool CanRemoveTower(Vector2Int position, int width = 2, int height = 2)
	{
		if (!IsValidGridArea(position.x, position.y, width, height))
		{
			return false;
		}

		GameObject target = GetGridNode(position.x, position.y).PlacedTower;
		if (!target)
		{
			return false;
		}

		for (int x = position.x; x < position.x + width; x++)
		{
			for (int y = position.y; y < position.y + height; y++)
			{
				if (GetGridNode(x, y).PlacedTower != target)
				{
					return false;
				}
			}
		}

		return true;
	}

	public Tower GetTower(Vector2Int position, int width = 2, int height = 2)
	{
		return CanRemoveTower(position, width, height) ? GetGridNode(position.x, position.y).Tower : null;
	}

	public bool RemoveTower(Vector2Int position, int width = 2, int height = 2)
	{
		if (!CanRemoveTower(position, width, height))
		{
			return false;
		}

		Tower tower = GetGridNode(position.x, position.y).Tower;

		for (int x = position.x; x < position.x + width; x++)
		{
			for (int y = position.y; y < position.y + height; y++)
			{
				SetGridType(x, y, GridType.Empty);
				SetGridTower(x, y, null);
			}
		}

		_activeTowers.Remove(tower);
		TowerPool.Instance.Release(tower);
		OnTowerRemoved?.Invoke(position);
		OnTowerDespawned?.Invoke(tower);
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.PickUp_Sfx);
		return true;
	}

	public Vector3 GridToWorld(int x, int y, int width = 1, int height = 1)
	{
		return new Vector3(
			_originPosition.x + ((x + (width * 0.5f)) * _cellSize),
			_originPosition.y + ((y + (height * 0.5f)) * _cellSize),
			0f
		);
	}

	public Vector3 GridToWorld(Vector2Int position, int width = 1, int height = 1)
	{
		return GridToWorld(position.x, position.y, width, height);
	}

	public Vector2Int WorldToGrid(Vector3 worldPosition)
	{
		int x = Mathf.FloorToInt((worldPosition.x - _originPosition.x) / _cellSize);
		int y = Mathf.FloorToInt((worldPosition.y - _originPosition.y) / _cellSize);
		return new Vector2Int(x, y);
	}

	public bool IsValidGridPos(Vector2Int gridPos)
	{
		return gridPos.x >= 0 && gridPos.x < Columns && gridPos.y >= 0 && gridPos.y < Rows;
	}

	public bool IsValidGridPos(int x, int y)
	{
		return x >= 0 && x < Columns && y >= 0 && y < Rows;
	}

	public bool IsValidGridArea(int x, int y, int width, int height)
	{
		return x >= 0 && x + width <= Columns && y >= 0 && y + height <= Rows;
	}

	public bool IsValidGridArea(Vector2Int position, Vector2Int size)
	{
		return IsValidGridArea(position.x, position.y, size.x, size.y);
	}

	public GridNode GetGridNode(Vector2Int position)
	{
		return !IsValidGridPos(position) ? null : _grid[ToIndex(position)];
	}

	public GridNode GetGridNode(int x, int y)
	{
		return !IsValidGridPos(x, y) ? null : _grid[ToIndex(x, y)];
	}

	// Helper Methods

	private void SetGridTower(Vector2Int position, Tower tower)
	{
		GridNode node = _grid[ToIndex(position)];
		node.PlacedTower = tower ? tower.gameObject : null;
		node.Tower = tower;
	}

	private void SetGridTower(int x, int y, Tower tower)
	{
		GridNode node = _grid[ToIndex(x, y)];
		node.PlacedTower = tower ? tower.gameObject : null;
		node.Tower = tower;
	}

	private void SetGridType(Vector2Int position, GridType type)
	{
		_grid[ToIndex(position)].Type = type;
	}

	private void SetGridType(int x, int y, GridType type)
	{
		_grid[ToIndex(x, y)].Type = type;
	}

	// Editor & Drawing Helpers

	public static List<Vector2Int> CalculatePathTiles(List<Vector2Int> waypoints)
	{
		List<Vector2Int> tiles = new();
		if (waypoints == null || waypoints.Count == 0)
		{
			return tiles;
		}

		if (waypoints.Count == 1)
		{
			tiles.Add(waypoints[0]);
			return tiles;
		}

		for (int i = 0; i < waypoints.Count - 1; i++)
		{
			Vector2Int start = waypoints[i];
			Vector2Int end = waypoints[i + 1];

			Vector2Int current = start;
			if (tiles.Count == 0 || tiles[^1] != current)
			{
				tiles.Add(current);
			}

			int dx = Math.Sign(end.x - current.x);
			while (current.x != end.x)
			{
				current = new Vector2Int(current.x + dx, current.y);
				tiles.Add(current);
			}

			int dy = Math.Sign(end.y - current.y);
			while (current.y != end.y)
			{
				current = new Vector2Int(current.x, current.y + dy);
				tiles.Add(current);
			}
		}

		return tiles;
	}

	public bool HasObstacle(Vector2Int pos) => _obstacles.Contains(pos);

	public bool AddObstacle(Vector2Int pos)
	{
		if (!IsValidGridPos(pos) || _obstacles.Contains(pos))
		{
			return false;
		}
		_obstacles.Add(pos);
		return true;
	}

	public bool RemoveObstacle(Vector2Int pos) => _obstacles.Remove(pos);

	public void ClearObstacles() => _obstacles.Clear();

	public void AddWaypoint(Vector2Int pos, int pathIndex = 0)
	{
		if (!IsValidGridPos(pos))
		{
			return;
		}
		List<Vector2Int> waypoints = GetPath(pathIndex);
		if (waypoints.Count > 0 && waypoints[^1] == pos)
		{
			return;
		}
		waypoints.Add(pos);
	}

	public void InsertWaypoint(int index, Vector2Int pos, int pathIndex = 0)
	{
		if (!IsValidGridPos(pos))
		{
			return;
		}
		List<Vector2Int> waypoints = GetPath(pathIndex);
		waypoints.Insert(Mathf.Clamp(index, 0, waypoints.Count), pos);
	}

	public bool RemoveWaypoint(int index, int pathIndex = 0)
	{
		List<Vector2Int> waypoints = GetPath(pathIndex);
		if (index < 0 || index >= waypoints.Count)
		{
			return false;
		}
		waypoints.RemoveAt(index);
		return true;
	}

	public bool RemoveWaypointAtPos(Vector2Int pos, int pathIndex = 0)
	{
		List<Vector2Int> waypoints = GetPath(pathIndex);
		int index = waypoints.IndexOf(pos);
		if (index < 0)
		{
			return false;
		}
		waypoints.RemoveAt(index);
		return true;
	}

	public void SetWaypoint(int index, Vector2Int pos, int pathIndex = 0)
	{
		List<Vector2Int> waypoints = GetPath(pathIndex);
		if (!IsValidGridPos(pos) || index < 0 || index >= waypoints.Count)
		{
			return;
		}
		waypoints[index] = pos;
	}

	public void ClearWaypoints(int pathIndex = 0) => GetPath(pathIndex).Clear();

	public void ReverseWaypoints(int pathIndex = 0) => GetPath(pathIndex).Reverse();

	public void LoadFromLevel(LevelSO level)
	{
		if (!level)
		{
			return;
		}
		_paths = new List<PathData>();
		for (int i = 0; i < level.Paths.Count; i++)
		{
			_paths.Add(level.Paths[i].Clone());
		}
		_obstacles = new List<Vector2Int>(level.Obstacles);
		EnsurePathsInitialized();
	}

	public void SaveToLevel(LevelSO level)
	{
		if (!level)
		{
			return;
		}
		EnsurePathsInitialized();
		level.Paths = new List<PathData>();
		for (int i = 0; i < _paths.Count; i++)
		{
			level.Paths.Add(_paths[i].Clone());
		}
		level.Obstacles = new List<Vector2Int>(_obstacles);
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(level);
		UnityEditor.AssetDatabase.SaveAssets();
#endif
	}

	// Debug
#if UNITY_EDITOR
	private static readonly Color[] PathPalette =
	{
		new Color(0.95f, 0.85f, 0.2f),
		new Color(0.2f, 0.8f, 1f),
		new Color(0.95f, 0.45f, 0.7f),
		new Color(0.3f, 0.9f, 0.5f),
		new Color(1f, 0.55f, 0.2f),
		new Color(0.7f, 0.4f, 1f),
	};

	public static Color GetPathColor(int index) => PathPalette[Mathf.Abs(index) % PathPalette.Length];

	private void OnDrawGizmos()
	{
		if (!_debugVisualization)
		{
			return;
		}

		// Draw grid cell outlines
		Gizmos.color = new Color(0.2f, 0.6f, 0.3f, 0.25f);
		for (int x = 0; x < Columns; x++)
		{
			for (int y = 0; y < Rows; y++)
			{
				Vector3 center = GridToWorld(new Vector2Int(x, y));
				Gizmos.DrawWireCube(center, new Vector3(_cellSize, _cellSize, 0.01f));

				if (_showCoordinateLabels)
				{
					var style = new GUIStyle
					{
						normal = { textColor = new Color(1f, 1f, 1f, 0.45f) },
						fontSize = 8,
						alignment = TextAnchor.MiddleCenter,
					};
					UnityEditor.Handles.Label(center, $"{x},{y}", style);
				}
			}
		}

		// Draw Obstacles
		for (int i = 0; i < _obstacles.Count; i++)
		{
			Vector3 center = GridToWorld(_obstacles[i]);
			center.z = -0.05f;
			Gizmos.color = new Color(0.95f, 0.35f, 0.15f, 0.7f);
			Gizmos.DrawCube(center, new Vector3(_cellSize * 0.92f, _cellSize * 0.92f, 0.05f));
			Gizmos.color = new Color(1f, 0.45f, 0.2f, 0.95f);
			Gizmos.DrawWireCube(center, new Vector3(_cellSize * 0.92f, _cellSize * 0.92f, 0.05f));
		}

		// Draw all Paths
		EnsurePathsInitialized();
		for (int p = 0; p < _paths.Count; p++)
		{
			List<Vector2Int> waypoints = _paths[p].Waypoints;
			if (waypoints.Count == 0)
			{
				continue;
			}

			Color pColor = GetPathColor(p);
			List<Vector2Int> pathTiles = CalculatePathTiles(waypoints);
			for (int i = 0; i < pathTiles.Count; i++)
			{
				Vector3 center = GridToWorld(pathTiles[i]);
				center.z = -0.03f;
				Gizmos.color = new Color(pColor.r, pColor.g, pColor.b, 0.4f);
				Gizmos.DrawCube(center, new Vector3(_cellSize * 0.92f, _cellSize * 0.92f, 0.04f));
			}

			if (waypoints.Count > 1)
			{
				Gizmos.color = new Color(pColor.r, pColor.g, pColor.b, 0.9f);
				for (int i = 0; i < waypoints.Count - 1; i++)
				{
					Vector3 p1 = GridToWorld(waypoints[i]);
					Vector3 p2 = GridToWorld(waypoints[i + 1]);
					p1.z = -0.06f;
					p2.z = -0.06f;
					Gizmos.DrawLine(p1, p2);
				}

				// Draw Goal
				Vector3 goalCenter = GridToWorld(waypoints[^1]);
				goalCenter.z = -0.04f;
				Gizmos.color = new Color(0.95f, 0.2f, 0.35f, 0.75f);
				Gizmos.DrawCube(goalCenter, new Vector3(_cellSize * 0.95f, _cellSize * 0.95f, 0.06f));
			}

			// Draw Spawn
			Vector3 spawnCenter = GridToWorld(waypoints[0]);
			spawnCenter.z = -0.04f;
			Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.75f);
			Gizmos.DrawCube(spawnCenter, new Vector3(_cellSize * 0.95f, _cellSize * 0.95f, 0.06f));
		}
	}
#endif
}
