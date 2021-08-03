using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace MSD.BasicSameGame.GameLogic
{
    /// <summary>
	/// A map of tile contents inside the <see cref="Grid"/>
	/// </summary>
    public class TileMap
    {
        private readonly int _tileTypeCount;

        private readonly int[,] _tiles;

        public int this[Vector2Int cellPosition] {
            get { return _tiles[cellPosition.x, cellPosition.y]; }
            private set {
                _tiles[cellPosition.x, cellPosition.y] = value;
            }
        }

        public TileMap(Vector2Int size, int tileTypeCount)
        {
            _tileTypeCount = tileTypeCount;
            _tiles = new int[size.x, size.y];
        }

        public TileMap(int horizontal, int vertical, int tileTypeCount)
            : this (new Vector2Int(horizontal, vertical), tileTypeCount) { }

        /// <summary>
		/// Remove all the tiles from the <see cref="Grid"/>.
		/// </summary>
        public void Clear()
        {
            Array.Clear(_tiles, 0, _tiles.Length);
        }

        /// <summary>
		/// Randomize a tile content at the cell in the specified position.
		/// </summary>
		/// <param name="cellPosition">The cell position.</param>
        public void RandomizeTileForCell(Vector2Int cellPosition)
        {
            this[cellPosition] = Random.Range(0, _tileTypeCount) + 1;
        }

        /// <summary>
		/// Remove the tile from the cell in the specified position.
		/// </summary>
		/// <param name="cellPosition">The cell position.</param>
        public void RemoveTileForCell(Vector2Int cellPosition)
        {
            this[cellPosition] = 0;
        }

        /// <summary>
		/// Set the tile content at the cell in the specified position.
		/// </summary>
		/// <param name="cellPosition">The cell position.</param>
		/// <param name="tile">The tile index.</param>
        public void SetTileForCell(Vector2Int cellPosition, int tile)
        {
            if (tile == 0) { throw new ArgumentException("Cannot explicitly set to empty. Use RemoveTileForCell instead."); }

            this[cellPosition] = tile;
        }

        /// <summary>
		/// Checks if the tile content at the cell in the specified position matches the specified tile.
		/// </summary>
		/// <param name="cellPosition">The cell position.</param>
		/// <param name="tile">The tile index.</param>
		/// <returns><c>true</c> if the tiles match, <c>true</c> otherwise.</returns>
        public bool IsSameTileForCell(Vector2Int cellPosition, int tile)
        {
            return this[cellPosition] == tile;
        }

        /// <summary>
		/// CHecks if the cell in the specified position is empty.
		/// </summary>
		/// <param name="cellPosition">The cell position.</param>
		/// <returns><c>true</c> if the cell is empty, <c>true</c> otherwise.</returns>
        public bool IsEmptyCell(Vector2Int cellPosition)
        {
            return this[cellPosition] == 0;
        }
    }
}

