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

		private readonly MatchRegistry _matchCollector = new MatchRegistry();

		public event Action<Vector2Int, int> OnTileCreated = delegate { };

		public event Action<Vector2Int> OnTileDestroyed = delegate { };

		public event Action<Vector2Int, Vector2Int> OnTileMoved = delegate { };

		public bool IsInitialized { get; private set; }

		public int TileCount { get; private set; }

		public Vector2Int GridSize => _grid.Size;

		public int BiggestMatch => _matchCollector.BiggestMatch;

		public int MatchesCount => _matchCollector.Count;

		public SameGame(Vector2Int size, int tileTypeCount, int minimumMatchCount)
		{
			_grid = new Grid(size);
			_tileMap = new TileMap(size, tileTypeCount);
			_minimumMatchCount = minimumMatchCount;

			IsInitialized = false;
		}

		public SameGame(SameGame sameGame)
		{
			_grid = sameGame._grid;
			_minimumMatchCount = sameGame._minimumMatchCount;

			_tileMap = new TileMap(sameGame._tileMap);
			_matchCollector = new MatchRegistry(sameGame._matchCollector);

			IsInitialized = sameGame.IsInitialized;
		}

		public void Initialize()
		{
			if (IsInitialized) { return; }

			var allCells = _grid.AllCells();
			while(allCells.MoveNext()) {
				_tileMap.RandomizeTileForCell(allCells.Current);
				OnTileCreated.Invoke(allCells.Current, _tileMap[allCells.Current]);
			}

			CalculateTileDetails();

			IsInitialized = true;
		}

		public void Reset()
		{
			_tileMap.Clear();
			IsInitialized = false;
			TileCount = 0;
		}

		public bool HasValidMoves() => _matchCollector.HasValidMoves;

		public Vector2Int[][] GetMatchingCells() => _matchCollector.Groups;

		public int DestroyMatchingTilesFromCell(Vector2Int cellPosition)
		{
			if (!_matchCollector.TryExtractMatchingGroup(cellPosition, out List<Vector2Int> matchingCells)) { return 0; }

			DestroyMatchingTiles(matchingCells);

			ApplyGravity();

			CalculateTileDetails();

			return matchingCells.Count;
		}

		private void CalculateTileDetails()
		{
			EvaluateMatches();
			TileCount = _tileMap.GetNonEmptyCellsCount();
		}

		private void EvaluateMatches()
		{
			_matchCollector.Reset();

			HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
			List<Vector2Int> matchingCells = new List<Vector2Int>();

			IEnumerator<Vector2Int> nonEmptyCells = _tileMap.GetNonEmptyCellsEnumerator();
			while (nonEmptyCells.MoveNext()) {
				if (visitedCells.Contains(nonEmptyCells.Current)) { continue; }

				int tileType = _tileMap[nonEmptyCells.Current];

				matchingCells.Clear();
				HashSet<Vector2Int> tempVisitedCells = new HashSet<Vector2Int>();
				CheckMatchingAdjoinedTilesFromCellRecursively(nonEmptyCells.Current, tileType, ref tempVisitedCells, ref matchingCells);

				if (matchingCells.Count >= _minimumMatchCount) {
					_matchCollector.RegisterMatch(matchingCells);
				}

				visitedCells.UnionWith(matchingCells);
			}
		}

		private void CheckMatchingAdjoinedTilesFromCellRecursively(in Vector2Int cellPosition, in int tileType,
			ref HashSet<Vector2Int> visitedCells, ref List<Vector2Int> matchingCells)
		{
			if (_tileMap.IsEmptyCell(cellPosition)) { return; }

			if (visitedCells.Contains(cellPosition)) { return; }

			visitedCells.Add(cellPosition);

			if (!_tileMap.IsSameTileForCell(cellPosition, tileType)) { return; }

			matchingCells.Add(cellPosition);

			IEnumerator<Vector2Int> adjoinedCells = _grid.GetAdjoinedCells(cellPosition);
			while (adjoinedCells.MoveNext()) {
				CheckMatchingAdjoinedTilesFromCellRecursively(adjoinedCells.Current, tileType, ref visitedCells, ref matchingCells);
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
			ApplyGravityVertically(out List<Vector2Int> originalPositionVertical, out List<Vector2Int> newPositionVertical);

			ApplyGravityHorizontally(out List<Vector2Int> originalPositionHorizontal, out List<Vector2Int> newPositionHorizontal);

			MergeNewPositions(ref originalPositionVertical, ref newPositionVertical, in originalPositionHorizontal, in newPositionHorizontal);

			Debug.Assert(originalPositionVertical.Count == newPositionVertical.Count, "The list `originalPosition` should have the same number of contents as the `newPosition` list.");

			for (int i = 0; i < originalPositionVertical.Count; i++) {
				_tileMap.SwapTile(originalPositionVertical[i], newPositionVertical[i]);
				OnTileMoved.Invoke(originalPositionVertical[i], newPositionVertical[i]);
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

		private void MergeNewPositions(ref List<Vector2Int> originalPositionVerticalMain, ref List<Vector2Int> newPositionVerticalMain,
			in List<Vector2Int> originalPositionHorizontalAdditive, in List<Vector2Int> newPositionHorizontalAdditive)
		{
			for (int i = 0; i < originalPositionVerticalMain.Count; i++) {
				Vector2Int origPos = originalPositionVerticalMain[i];
				if (originalPositionHorizontalAdditive.Contains(origPos)) {
					int idx = originalPositionHorizontalAdditive.IndexOf(origPos);
					Vector2Int newPosX = newPositionHorizontalAdditive[idx];

					Vector2Int newPosY = newPositionVerticalMain[i];
					newPositionVerticalMain[i] = new Vector2Int(newPosX.x, newPosY.y);

					originalPositionHorizontalAdditive.RemoveAt(idx);
					newPositionHorizontalAdditive.RemoveAt(idx);
				}
			}

			originalPositionVerticalMain.AddRange(originalPositionHorizontalAdditive);
			newPositionVerticalMain.AddRange(newPositionHorizontalAdditive);
		}
	}
}
