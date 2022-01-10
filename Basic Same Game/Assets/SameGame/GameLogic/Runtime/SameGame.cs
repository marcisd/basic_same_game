using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSD.BasicSameGame.GameLogic
{
	public class SameGame
	{
		private readonly TileMap _tileMap;

		private readonly MatchRegistry _matchRegistry;

		public event Action<Vector2Int, int> OnTileCreated = delegate { };

		public event Action<Vector2Int> OnTileDestroyed = delegate { };

		public event Action<Vector2Int, Vector2Int> OnTileMoved = delegate { };

		public bool IsInitialized { get; private set; }

		public int TileCount { get; private set; }

		public int BiggestMatch => _matchRegistry.BiggestMatch;

		public int MatchesCount => _matchRegistry.Count;

		public bool HasValidMoves => _matchRegistry.HasValidMoves;

		public SameGame(Vector2Int size, int tileTypeCount, int minimumMatchCount)
		{
			_tileMap = new TileMap(size, tileTypeCount);
			_matchRegistry = new MatchRegistry(size, minimumMatchCount);

			IsInitialized = false;
		}

		public SameGame(SameGame sameGame)
		{
			_tileMap = new TileMap(sameGame._tileMap);
			_matchRegistry = new MatchRegistry(sameGame._matchRegistry);

			IsInitialized = sameGame.IsInitialized;
		}

		public void Initialize()
		{
			if (IsInitialized) { return; }

			for (int i = 0; i < _tileMap.SizeX; i++) {
				for (int j = 0; j < _tileMap.SizeY; j++) {
					Vector2Int pos = new Vector2Int(i, j);
					_tileMap.RandomizeTileForCell(pos);
					OnTileCreated(pos, _tileMap[pos]);
				}
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

		public Dictionary<Vector2Int, int> GetMatchRepresentatives() => _matchRegistry.GetMatchRepresentatives();

		public int DestroyMatchingTilesFromCell(Vector2Int cellPosition)
		{
			if (!_matchRegistry.TryGetMatchingGroup(cellPosition, out Vector2Int[] matchingCells)) { return 0; }

			DestroyMatchingTiles(matchingCells);

			ApplyGravity();

			CalculateTileDetails();

			return matchingCells.Length;
		}

		private void CalculateTileDetails()
		{
			_matchRegistry.FindMatches(_tileMap);
			TileCount = _tileMap.GetNonEmptyCellsCount();
		}

		private void DestroyMatchingTiles(in Vector2Int[] matchingTilesPositions)
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
				OnTileMoved(originalPositionVertical[i], newPositionVertical[i]);
			}
		}

		private void ApplyGravityVertically(out List<Vector2Int> originalPosition, out List<Vector2Int> newPosition)
		{
			originalPosition = new List<Vector2Int>();
			newPosition = new List<Vector2Int>();

			for (int i = 0; i < _tileMap.SizeX; i++) {
				ApplyGravityVerticallyForColumn(i, _tileMap.SizeY, ref originalPosition, ref newPosition);
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

				for (int i = 0; i < _tileMap.SizeY; i++) {
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

			for (int i = 0; i < _tileMap.SizeX; i++) {
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
