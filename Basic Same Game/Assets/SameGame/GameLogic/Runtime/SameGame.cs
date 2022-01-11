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
			SnowballVertically(out List<Vector2Int> originalPositionVertical, out List<Vector2Int> newPositionVertical, out bool[] columnEmptiness);

			SnowballHorizontally(in columnEmptiness, out List<Vector2Int> originalPositionHorizontal, out List<Vector2Int> newPositionHorizontal);

			MergeNewPositions(ref originalPositionVertical, ref newPositionVertical, in originalPositionHorizontal, in newPositionHorizontal);

			Debug.Assert(originalPositionVertical.Count == newPositionVertical.Count, "The list `originalPosition` should have the same number of contents as the `newPosition` list.");

			for (int i = 0; i < originalPositionVertical.Count; i++) {
				_tileMap.SwapTile(originalPositionVertical[i], newPositionVertical[i]);
				OnTileMoved(originalPositionVertical[i], newPositionVertical[i]);
			}
		}

		private void SnowballVertically(out List<Vector2Int> originalPosition, out List<Vector2Int> newPosition, out bool[] columnEmptiness)
		{
			originalPosition = new List<Vector2Int>();
			newPosition = new List<Vector2Int>();
			columnEmptiness = new bool[_tileMap.SizeX];

			for (int i = 0; i < _tileMap.SizeX; i++) {
				SnowballVerticallyForColumn(i, ref originalPosition, ref newPosition, out bool isEmptyColumn);
				columnEmptiness[i] = isEmptyColumn;
			}
		}

		private void SnowballVerticallyForColumn(in int column, ref List<Vector2Int> originalPosition, ref List<Vector2Int> newPosition, out bool isEmptyColumn)
		{
			int[] col = _tileMap.CopyColumn(column);
			int fast = 0;
			int slow = 0;

			while (fast < col.Length) {
				if (col[fast] == 0) {
					fast++;
				} else {
					if (fast == slow) {
						fast++;
						slow++;
					} else {
						originalPosition.Add(new Vector2Int(column, fast));
						newPosition.Add(new Vector2Int(column, slow));

						int temp = col[fast];
						col[fast++] = col[slow];
						col[slow++] = temp;
					}
				}
			}

			isEmptyColumn = slow == 0;
		}

		private void SnowballHorizontally(in bool[] columnEmptiness, out List<Vector2Int> originalPosition, out List<Vector2Int> newPosition)
		{
			originalPosition = new List<Vector2Int>();
			newPosition = new List<Vector2Int>();
			int fast = 0;
			int slow = 0;

			while (fast < columnEmptiness.Length) {
				if (columnEmptiness[fast]) {
					fast++;
				} else {
					if (fast == slow) {
						fast++;
						slow++;
					} else {

						for (int i = 0; i < _tileMap.SizeY; i++) {
							Vector2Int origPos = new Vector2Int(fast, i);

							if (_tileMap.IsEmptyCell(origPos)) { continue; }

							Vector2Int newPos = new Vector2Int(slow, i);
							originalPosition.Add(origPos);
							newPosition.Add(newPos);
						}

						bool temp = columnEmptiness[fast];
						columnEmptiness[fast++] = columnEmptiness[slow];
						columnEmptiness[slow++] = temp;
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
