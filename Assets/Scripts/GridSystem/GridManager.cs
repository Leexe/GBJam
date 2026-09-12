using UnityEngine;
using UnityEngine.Tilemaps;

namespace GridSystem
{
    public sealed class GridManager : MonoSingleton<GridManager>
    {
        [Header("Grid Dimensions")]
        [SerializeField] private int _width = 10;
        [SerializeField] private int _height = 9;
        [SerializeField] private Vector2Int _origin = Vector2Int.zero;
        [SerializeField] private float _cellSize = 1f;

        [Header("Tilemap References")]
        [SerializeField] private UnityEngine.Grid _unityGrid;
        [SerializeField] private Tilemap _groundTilemap;
        [SerializeField] private Tilemap _wallsTilemap;
        [SerializeField] private Tilemap _pathTilemap;
        [SerializeField] private Tilemap _propsTilemap;

        private GridCell[,] _cells;

        public int Width => _width;
        public int Height => _height;
        public Vector2Int Origin => _origin;
        public float CellSize => _cellSize;

        protected override void OnInitialized()
        {
            BuildGrid();
        }

        public void BuildGrid()
        {
            _cells = new GridCell[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Vector2Int coord = new Vector2Int(x + _origin.x, y + _origin.y);
                    Vector3 worldPos = CalculateWorldPosition(coord);
                    GridCellType cellType = DetermineCellType(coord);

                    _cells[x, y] = new GridCell(coord, worldPos, cellType);
                }
            }
        }

        public bool IsInBounds(Vector2Int coord)
        {
            int localX = coord.x - _origin.x;
            int localY = coord.y - _origin.y;
            return localX >= 0 && localX < _width && localY >= 0 && localY < _height;
        }

        public GridCell GetCell(Vector2Int coord)
        {
            if (!IsInBounds(coord) || _cells == null)
            {
                return null;
            }

            int localX = coord.x - _origin.x;
            int localY = coord.y - _origin.y;
            return _cells[localX, localY];
        }

        public bool TryGetCell(Vector2Int coord, out GridCell cell)
        {
            cell = GetCell(coord);
            return cell != null;
        }

        public Vector3 CalculateWorldPosition(Vector2Int coord)
        {
            if (_unityGrid != null)
            {
                return _unityGrid.GetCellCenterWorld(new Vector3Int(coord.x, coord.y, 0));
            }

            return new Vector3(coord.x + 0.5f, coord.y + 0.5f, 0f) * _cellSize;
        }

        public Vector2Int WorldToGridCoordinate(Vector3 worldPosition)
        {
            if (_unityGrid != null)
            {
                Vector3Int cellPos = _unityGrid.WorldToCell(worldPosition);
                return new Vector2Int(cellPos.x, cellPos.y);
            }

            int x = Mathf.FloorToInt(worldPosition.x / _cellSize);
            int y = Mathf.FloorToInt(worldPosition.y / _cellSize);
            return new Vector2Int(x, y);
        }

        private GridCellType DetermineCellType(Vector2Int coord)
        {
            Vector3Int tilePos = new Vector3Int(coord.x, coord.y, 0);

            if (_pathTilemap != null && _pathTilemap.HasTile(tilePos))
            {
                return GridCellType.Path;
            }

            if (_wallsTilemap != null && _wallsTilemap.HasTile(tilePos))
            {
                return GridCellType.Wall;
            }

            if (_groundTilemap != null && _groundTilemap.HasTile(tilePos))
            {
                return GridCellType.Ground;
            }

            return GridCellType.Empty;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Vector2Int coord = new Vector2Int(x + _origin.x, y + _origin.y);
                    Vector3 center = CalculateWorldPosition(coord);
                    Gizmos.DrawWireCube(center, Vector3.one * _cellSize * 0.95f);
                }
            }
        }
    }
}
