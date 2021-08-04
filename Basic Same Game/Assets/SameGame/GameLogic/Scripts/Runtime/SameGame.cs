using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSD.BasicSameGame.GameLogic
{
	public class SameGame
	{
		private readonly Grid _grid;

		private readonly TileMap _tileMap;

		private readonly int _minimumMatchCount;

		public event Action<Vector2Int, int> OnTileCreated = delegate { };

		public event Action<Vector2Int> OnTileDestroyed = delegate { };

		public event Action<Vector2Int, Vector2Int> OnTileMoved = delegate { };

		public bool IsInitialized { get; private set; }

		public Vector2Int GridSize => _grid.Size;

		public SameGame(Vector2Int size, int tileTypeCount, int minimumMatchCount)
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
				OnTileCreated.Invoke(position, _tileMap[position]);
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

		public int DestroyMatchingTilesFromCell(Vector2Int cellPosition)
		{
			int tileType = _tileMap[cellPosition];
			HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
			List<Vector2Int> matchingCells = new List<Vector2Int>();

			CheckMatchingAdjoinedTilesFromCell(cellPosition, tileType, ref visitedCells, ref matchingCells);

			if (matchingCells.Count < _minimumMatchCount) { return 0; }

			DestroyMatchingTiles(matchingCells);

			FreeFallFloatingTiles();

			return matchingCells.Count;
		}

		private void CheckMatchingAdjoinedTilesFromCell(in Vector2Int cellPosition, in int tileType, ref HashSet<Vector2Int> visitedCells, ref List<Vector2Int> matchingCells)
		{
			if (_tileMap.IsEmptyCell(cellPosition)) { return; }

			if (visitedCells.Contains(cellPosition)) { return; }

			visitedCells.Add(cellPosition);

			if (!_tileMap.IsSameTileForCell(cellPosition, tileType)) { return; }

			matchingCells.Add(cellPosition);

			IEnumerator<Vector2Int> adjoinedCells = _grid.AdjoinedCells(cellPosition);
			while (adjoinedCells.MoveNext()) {
				CheckMatchingAdjoinedTilesFromCell(adjoinedCells.Current, tileType, ref visitedCells, ref matchingCells);
			}
		}

		private void DestroyMatchingTiles(in List<Vector2Int> matchingTilesPositions)
		{
			foreach (Vector2Int cellPosition in matchingTilesPositions) {
				_tileMap.RemoveTileForCell(cellPosition);
				OnTileDestroyed(cellPosition);
			}
		}

		private void FreeFallFloatingTiles()
		{
			List<Vector2Int> originalPosition = new List<Vector2Int>();
			List<Vector2Int> newPosition = new List<Vector2Int>();

			for (int i = 0; i < _grid.Size.x; i++) {
				FreeFallFloatingTilesForColumn(i, _grid.Size.y, ref originalPosition, ref newPosition);
			}

			// horizontal shift

			Debug.Assert(originalPosition.Count == newPosition.Count, "The list `originalPosition` should have the same number of contents as the `newPosition` list.");

			for (int i = 0; i < originalPosition.Count; i++) {
				_tileMap.SwapTile(originalPosition[i], newPosition[i]);
				OnTileMoved.Invoke(originalPosition[i], newPosition[i]);
			}
		}

		private void FreeFallFloatingTilesForColumn(in int column, in int size, ref List<Vector2Int> originalPosition, ref List<Vector2Int> newPosition)
		{
			GetEmptyAndFloatingCellsForColumn(column, size, out Queue<int> floatingCells, out List<int> emptyCells);

			while (floatingCells.Count > 0) {
				int floatingYPos = floatingCells.Dequeue();
				int emptyYPos = emptyCells[0];

				originalPosition.Add(new Vector2Int(column, floatingYPos));
				newPosition.Add(new Vector2Int(column, emptyYPos));

				emptyCells.RemoveAt(0);
				emptyCells.Add(floatingYPos);
				emptyCells.Sort();
			}

		}

		private void GetEmptyAndFloatingCellsForColumn(in int column, in int size, out Queue<int> floatingCells, out List<int> emptyCells)
		{
			floatingCells = new Queue<int>();
			emptyCells = new List<int>();

			for (int i = 0; i < size; i++) {
				Vector2Int cellPos = new Vector2Int(column, i);

				if (_tileMap.IsEmptyCell(cellPos)) {
					emptyCells.Add(i);
				} else {
					if (emptyCells.Count > 0) {
						floatingCells.Enqueue(i);
					}
				}
			}
		}
	}
}
