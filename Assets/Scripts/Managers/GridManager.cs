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

	[SerializeField]
	private float _cellSize = 0.5f;

	[Header("Temp Game Data Fields")]
	[SerializeField]
	private bool _debugVisualization = true;

	[SerializeField]
	private bool _showCoordinateLabels = true;

	private const int Rows = 16;
	private const int Columns = 20;
	public int GetMaxRows => Rows;
	public int GetMaxColumns => Columns;

	private readonly GridNode[] _grid = new GridNode[Columns * Rows];
	public List<Vector2Int> EnemyWaypoints => _enemyWaypoints;
	public float TotalPathDistance { get; private set; }

	private int ToIndex(int x, int y) => x + (y * Columns);

	private int ToIndex(Vector2Int pos) => pos.x + (pos.y * Columns);

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
				_grid[ToIndex(x, y)] = new GridNode(new Vector2Int(x, y), GridType.Empty, null);
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

		TotalPathDistance = 0f;
		for (int i = 0; i < _enemyWaypoints.Count - 1; i++)
		{
			TotalPathDistance += Vector3.Distance(
				GridToWorld(_enemyWaypoints[i]),
				GridToWorld(_enemyWaypoints[i + 1])
			);
		}

		// Mark Spawn Point
		SetGridType(_enemyWaypoints[0], GridType.Spawn);

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
				if (_grid[ToIndex(current)].Type != GridType.Spawn)
				{
					SetGridType(current, GridType.Path);
				}
				current = new(current.x + dx, current.y + dy);
			}
		}

		// Mark Goal Point
		SetGridType(_enemyWaypoints[^1], GridType.Goal);
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

	public bool PlaceTower(Vector2Int position, GameObject placedTower, int width = 2, int height = 2)
	{
		if (!CanPlaceTower(position, width, height))
		{
			return false;
		}

		for (int x = position.x; x < position.x + width; x++)
		{
			for (int y = position.y; y < position.y + height; y++)
			{
				SetGridType(x, y, GridType.Tower);
				SetGridTower(x, y, placedTower);
			}
		}

		OnTowerPlaced?.Invoke(position, placedTower);
		return true;
	}

	public void RemoveTower(Vector2Int position, int width = 2, int height = 2)
	{
		for (int x = position.x; x < position.x + width; x++)
		{
			for (int y = position.y; y < position.y + height; y++)
			{
				if (GetGridNode(x, y).Type == GridType.Tower)
				{
					SetGridType(x, y, GridType.Empty);
					SetGridTower(x, y, null);
				}
			}
		}

		OnTowerRemoved?.Invoke(position);
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

	private void SetGridTower(Vector2Int position, GameObject tower)
	{
		_grid[ToIndex(position)].PlacedTower = tower;
	}

	private void SetGridTower(int x, int y, GameObject tower)
	{
		_grid[ToIndex(x, y)].PlacedTower = tower;
	}

	private void SetGridType(Vector2Int position, GridType type)
	{
		_grid[ToIndex(position)].Type = type;
	}

	private void SetGridType(int x, int y, GridType type)
	{
		_grid[ToIndex(x, y)].Type = type;
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
				Gizmos.DrawWireCube(center, new Vector3(_cellSize * 0.95f, _cellSize * 0.95f, 0.05f));

				if (_showCoordinateLabels)
				{
					var style = new GUIStyle
					{
						normal = { textColor = Color.white },
						fontSize = 8,
						alignment = TextAnchor.MiddleCenter,
					};
					UnityEditor.Handles.Label(center, $"{x},{y}", style);
				}
			}
		}
		if (_enemyWaypoints.Count > 1)
		{
			Gizmos.color = Color.yellow;
			for (int i = 0; i < _enemyWaypoints.Count - 1; i++)
			{
				Gizmos.DrawLine(GridToWorld(_enemyWaypoints[i]), GridToWorld(_enemyWaypoints[i + 1]));
			}
		}
	}
#endif
}
