using UnityEngine;

namespace GridSystem
{
    public sealed class GridCell
    {
        public Vector2Int Coordinates { get; }
        public Vector3 WorldCenterPosition { get; }
        public GridCellType CellType { get; private set; }
        public GameObject Occupant { get; private set; }

        public bool is_walkable => CellType == GridCellType.Path || CellType == GridCellType.Spawn || CellType == GridCellType.Fortress;
        public bool is_occupied => Occupant != null;

        public GridCell(Vector2Int coordinates, Vector3 worldCenterPosition, GridCellType cellType = GridCellType.Ground)
        {
            Coordinates = coordinates;
            WorldCenterPosition = worldCenterPosition;
            CellType = cellType;
            Occupant = null;
        }

        public void SetCellType(GridCellType newCellType)
        {
            CellType = newCellType;
        }

        public void SetOccupant(GameObject occupant)
        {
            Occupant = occupant;
        }

        public void ClearOccupant()
        {
            Occupant = null;
        }
    }
}
