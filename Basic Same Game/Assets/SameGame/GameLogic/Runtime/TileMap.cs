using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

namespace MSD.BasicSameGame.GameLogic
{
    internal class TileMap
    {
        private readonly int _tileTypeCount;

        private readonly int[,] _tiles;

        public int SizeX => _tiles.GetLength(0);

        public int SizeY => _tiles.GetLength(1);

        public int this[Vector2Int cellPosition] {
            get { return _tiles[cellPosition.x, cellPosition.y]; }
            private set {
                _tiles[cellPosition.x, cellPosition.y] = value;
            }
        }

        public TileMap(Vector2Int size, int tileTypeCount)
        {
            if (size.x == 0 || size.y == 0) throw new ArgumentException("Tile map size cannot be empoty!", nameof(size));

            _tileTypeCount = tileTypeCount;
            _tiles = new int[size.x, size.y];
        }

        public TileMap(int horizontal, int vertical, int tileTypeCount)
            : this (new Vector2Int(horizontal, vertical), tileTypeCount) { }

        public TileMap(TileMap tileMap)
        {
            _tileTypeCount = tileMap._tileTypeCount;
            _tiles = tileMap._tiles.Clone() as int[,];
        }

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
            if (tile == 0) { throw new ArgumentException("Cannot explicitly set to empty. Use RemoveTileForCell instead."); }

            this[cellPosition] = tile;
        }

        public void SwapTile(Vector2Int lhs, Vector2Int rhs)
        {
            int temp = this[lhs];
            this[lhs] = this[rhs];
            this[rhs] = temp;
        }

        public bool IsSameTileForCell(Vector2Int cellPosition, int tile)
        {
            return this[cellPosition] == tile;
        }

        public bool IsEmptyCell(Vector2Int cellPosition)
        {
            return this[cellPosition] == 0;
        }

        public bool IsEmptyColumn(int columnPosition)
        {
            for (int i = 0; i < _tiles.GetLength(1); i++) {
                if (_tiles[columnPosition, i] != 0) { return false; }
            }
            return true;
        }

        public IEnumerator<Vector2Int> GetNonEmptyCellsEnumerator()
        {
            for (int i = 0; i < _tiles.GetLength(0); i++) {
                for (int j = 0; j < _tiles.GetLength(1); j++) {
                    if (_tiles[i, j] != 0) {
                        yield return new Vector2Int(i, j);
                    }
                }
            }
        }

        public int GetNonEmptyCellsCount()
        {
            int counter = 0;
            for (int i = 0; i < _tiles.GetLength(0); i++) {
                for (int j = 0; j < _tiles.GetLength(1); j++) {
                    if (_tiles[i, j] != 0) {
                        counter++;
                    }
                }
            }
            return counter;
        }

    }
}

