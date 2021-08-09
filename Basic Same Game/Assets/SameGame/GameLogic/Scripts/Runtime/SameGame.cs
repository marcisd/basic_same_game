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
			// TODO
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

			ApplyGravity();

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

		private void ApplyGravity()
		{
			ApplyGravityVertically(out List<Vector2Int> originalPosition, out List<Vector2Int> newPosition);

			ApplyGravityHorizontally(out List<Vector2Int> originalPositionH, out List<Vector2Int> newPositionH);

			for (int i = 0; i < originalPosition.Count; i++) {
				//foreach (var yyyOld in originalPosition) {
				var yyyOld = originalPosition[i];
				if (originalPositionH.Contains(yyyOld)) {
					int idx = originalPositionH.IndexOf(yyyOld);
					var xxx = newPositionH[idx];

					var yyy = newPosition[i];
					newPosition[i] = new Vector2Int(xxx.x, yyy.y);

					originalPositionH.RemoveAt(idx);
					newPositionH.RemoveAt(idx);
				}
			}

			originalPosition.AddRange(originalPositionH);
			newPosition.AddRange(newPositionH);

			Debug.Assert(originalPosition.Count == newPosition.Count, "The list `originalPosition` should have the same number of contents as the `newPosition` list.");

			for (int i = 0; i < originalPosition.Count; i++) {
				_tileMap.SwapTile(originalPosition[i], newPosition[i]);
				OnTileMoved.Invoke(originalPosition[i], newPosition[i]);
			}
		}

		private void ApplyGravityVertically(out List<Vector2Int> originalPosition, out List<Vector2Int> newPosition)
		{
			originalPosition = new List<Vector2Int>();
			newPosition = new List<Vector2Int>();

			for (int i = 0; i < _grid.Size.x; i++) {
				ApplyGravityVerticallyForColumn(i, _grid.Size.y, ref originalPosition, ref newPosition);
			}
		}

		private void ApplyGravityVerticallyForColumn(in int column, in int size, ref List<Vector2Int> originalPosition, ref List<Vector2Int> newPosition)
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

		private void ApplyGravityHorizontally(out List<Vector2Int> originalPosition, out List<Vector2Int> newPosition)
		{
			originalPosition = new List<Vector2Int>();
			newPosition = new List<Vector2Int>();

			GetGapAndFillerColumns(out Queue<int> fillerColumns, out List<int> gapColumns);

			while (fillerColumns.Count > 0) {
				int fillerXPos = fillerColumns.Dequeue();
				int gapYPos = gapColumns[0];

				for (int i = 0; i < _grid.Size.y; i++) {
					Vector2Int origPos = new Vector2Int(fillerXPos, i);
					Vector2Int newPos = new Vector2Int(gapYPos, i);

					if (_tileMap.IsEmptyCell(origPos)) { continue; }

					originalPosition.Add(origPos);
					newPosition.Add(newPos);
				}

				gapColumns.RemoveAt(0);
				gapColumns.Add(fillerXPos);
				gapColumns.Sort();
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

		private void GetGapAndFillerColumns(out Queue<int> fillerColumns, out List<int> gapColumns)
		{
			fillerColumns = new Queue<int>();
			gapColumns = new List<int>();

			for (int i = 0; i < _grid.Size.x; i++) {

				if (_tileMap.IsEmptyColumn(i)) {
					gapColumns.Add(i);
				} else {
					if (gapColumns.Count > 0) {
						fillerColumns.Enqueue(i);
					}
				}
			}
		}
	}
}
