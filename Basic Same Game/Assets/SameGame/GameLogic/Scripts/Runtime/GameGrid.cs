using System.Collections.Generic;
using UnityEngine;

namespace MSD.BasicSameGame.GameLogic
{
	public class GameGrid
	{
		private readonly Grid _grid;

		private readonly TileMap _tileMap;

		private readonly int _minimumMatchCount;

		public bool IsInitialized { get; private set; }

		public GameGrid(Vector2Int size, int tileTypeCount, int minimumMatchCount)
		{
			_grid = new Grid(size);
			_tileMap = new TileMap(size, tileTypeCount);

			_minimumMatchCount = minimumMatchCount;
			
			IsInitialized = false;
		}

		public void Initialize()
		{
			if (IsInitialized) { return; }

			_grid.ForEachCell((position) => {
				_tileMap.RandomizeTileForCell(position);
			});
			IsInitialized = true;
		}

		public void Reset()
		{
			IsInitialized = false;
			_tileMap.Clear();
		}

		public bool HasValidMoves()
		{
			return false;
		}

		public int SelectMatchingTilesFromCell(Vector2Int cellPosition)
		{
			int tileType = _tileMap[cellPosition];
			HashSet<Vector2Int> matchingCells = new HashSet<Vector2Int>();

			CheckMatchingAdjoinedTilesFromCell(cellPosition, in tileType, ref matchingCells);

			if (matchingCells.Count < _minimumMatchCount) { return 0; }

			int matchCount = matchingCells.Count;

			// destroy matching tiles

			// fall floating tiles

			return matchCount;
		}

		private void CheckMatchingAdjoinedTilesFromCell(Vector2Int cellPosition, in int tileType, ref HashSet<Vector2Int> matchingCells)
		{
			if (_tileMap.IsEmptyCell(cellPosition)) return;

			if (!_tileMap.IsSameTileForCell(cellPosition, tileType)) return;

			matchingCells.Add(cellPosition);

			IEnumerator<Vector2Int> adjoinedCells = _grid.AdjoinedCells(cellPosition);
			while (adjoinedCells.MoveNext()) {
				CheckMatchingAdjoinedTilesFromCell(adjoinedCells.Current, in tileType, ref matchingCells);
			}
		}
	}
}
