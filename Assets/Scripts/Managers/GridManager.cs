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

	public GridNode(Vector2Int position, GridType type, GameObject placedTower)
	{
		Position = position;
		Type = type;
		PlacedTower = placedTower;
	}

	public bool CanPlaceTower => PlacedTower == null && Type == GridType.Empty;
}

public class GridManager : MonoSingleton<GridManager>
{
	[Header("Temp Game Data Fields")]
	[SerializeField]
	private List<Vector2Int> _enemyWaypoints;

	[SerializeField]
	private List<Vector2Int> _obstacles;

	[Header("Settings")]
	[SerializeField]
	private Vector2 _originPosition;

	[Header("Temp Game Data Fields")]
	[SerializeField]
	private bool _debugVisualization = true;

	[SerializeField]
	private bool _showCoordinateLabels = true;

	private const int Rows = 8;
	private const int Columns = 10;

	private GridNode[,] _grid = new GridNode[Columns, Rows];
	public List<GridNode> Path { get; private set; } = new();

	// Events
	[HideInInspector]
	public Action<Vector2Int, GridType> OnTileStateChanged;

	[HideInInspector]
	public Action<Vector2Int, GameObject> OnTowerPlaced;

	[HideInInspector]
	public Action<Vector2Int> OnTowerRemoved;

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
				_grid[x, y] = new GridNode(new Vector2Int(x, y), GridType.Empty, null);
			}
		}

		foreach (Vector2Int t in _obstacles)
		{
			if (IsValidGridPos(t))
			{
				SetGridType(t, GridType.Obstacle);
			}
		}

		InitializePath();
	}

	private void InitializePath()
	{
		if (_enemyWaypoints.Count <= 1)
		{
			return;
		}

		Path.Clear();

		// Mark Spawn Point
		SetGridType(_enemyWaypoints[0], GridType.Spawn);
		Path.Add(_grid[_enemyWaypoints[0].x, _enemyWaypoints[0].y]);

		// Mark Path
		for (int i = 0; i < _enemyWaypoints.Count - 1; i++)
		{
			Vector2Int start = _enemyWaypoints[i];
			Vector2Int end = _enemyWaypoints[i + 1];

			int dx = Math.Sign(end.x - start.x);
			int dy = Math.Sign(end.y - start.y);

			Vector2Int current = start;
			while (current != end)
			{
				if (_grid[current.x, current.y].Type != GridType.Spawn)
				{
					SetGridType(current, GridType.Path);
					Path.Add(_grid[current.x, current.y]);
				}
				current = new(current.x + dx, current.y + dy);
			}
		}

		// Mark Goal Point
		SetGridType(_enemyWaypoints[^1], GridType.Goal);
		Path.Add(_grid[_enemyWaypoints[^1].x, _enemyWaypoints[^1].y]);
	}

	public bool PlaceTower(Vector2Int position, GameObject placedTower)
	{
		if (!IsValidGridPos(position) || !_grid[position.x, position.y].CanPlaceTower)
		{
			return false;
		}

		SetGridType(position, GridType.Tower);
		SetGridTower(position, placedTower);
		OnTowerPlaced?.Invoke(position, placedTower);
		return true;
	}

	public void RemoveTower(Vector2Int position)
	{
		if (!IsValidGridPos(position) || _grid[position.x, position.y].Type != GridType.Tower)
		{
			return;
		}

		SetGridType(position, GridType.Empty);
		SetGridTower(position, null);
		OnTowerRemoved?.Invoke(position);
	}

	public Vector3 GridToWorld(int x, int y)
	{
		return new Vector3(_originPosition.x + (x + 0.5f), _originPosition.y + (y + 0.5f), 0f);
	}

	public Vector3 GridToWorld(Vector2Int position)
	{
		return new Vector3(_originPosition.x + (position.x + 0.5f), _originPosition.y + (position.y + 0.5f), 0f);
	}

	public Vector2Int WorldToGrid(Vector3 worldPosition)
	{
		int x = Mathf.FloorToInt(worldPosition.x - _originPosition.x);
		int y = Mathf.FloorToInt(worldPosition.y - _originPosition.y);
		return new Vector2Int(x, y);
	}

	public bool IsValidGridPos(Vector2Int gridPos)
	{
		return gridPos.x >= 0 && gridPos.x < Columns && gridPos.y >= 0 && gridPos.y < Rows;
	}

	public GridNode GetGridNode(Vector2Int position)
	{
		return !IsValidGridPos(position) ? null : _grid[position.x, position.y];
	}

	// Helper Methods

	private void SetGridTower(Vector2Int position, GameObject tower)
	{
		_grid[position.x, position.y].PlacedTower = tower;
	}

	private void SetGridTower(int x, int y, GameObject tower)
	{
		_grid[x, y].PlacedTower = tower;
	}

	private void SetGridType(Vector2Int position, GridType type)
	{
		_grid[position.x, position.y].Type = type;
	}

	private void SetGridType(int x, int y, GridType type)
	{
		_grid[x, y].Type = type;
	}

	// Debug
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (!_debugVisualization)
		{
			return;
		}

		for (int x = 0; x < Columns; x++)
		{
			for (int y = 0; y < Rows; y++)
			{
				Vector3 center = GridToWorld(new Vector2Int(x, y));
				Gizmos.color = Color.lightGreen;
				Gizmos.DrawWireCube(center, new Vector3(0.95f, 0.95f, 0.05f));

				if (_showCoordinateLabels)
				{
					GUIStyle style = new GUIStyle
					{
						normal = { textColor = Color.white },
						fontSize = 8,
						alignment = TextAnchor.MiddleCenter,
					};
					UnityEditor.Handles.Label(center, $"({x},{y})", style);
				}
			}
		}
		if (Path.Count > 1)
		{
			Gizmos.color = Color.yellow;
			for (int i = 0; i < Path.Count - 1; i++)
			{
				Gizmos.DrawLine(GridToWorld(Path[i].Position), GridToWorld(Path[i + 1].Position));
			}
		}
	}
#endif
}
