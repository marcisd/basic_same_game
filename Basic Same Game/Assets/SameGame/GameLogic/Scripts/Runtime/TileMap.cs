using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace MSD.BasicSameGame.GameLogic
{
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

        public void Clear()
        {
            Array.Clear(_tiles, 0, _tiles.Length);
        }

        public void RandomizeTileForCell(Vector2Int cellPosition)
        {
            this[cellPosition] = Random.Range(0, _tileTypeCount) + 1;
        }

        public void RemoveTileForCell(Vector2Int cellPosition)
        {
            this[cellPosition] = 0;
        }

        public void SetTileForCell(Vector2Int cellPosition, int tile)
        {
            // warn user if tile value is 0;
            this[cellPosition] = tile;
        }

        public bool IsSameTileForCell(Vector2Int cellPosition, int tile)
        {
            return this[cellPosition] == tile;
        }

        public bool IsEmptyCell(Vector2Int cellPosition)
        {
            return this[cellPosition] == 0;
        }
    }
}

