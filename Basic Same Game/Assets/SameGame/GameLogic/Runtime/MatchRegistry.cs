using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSD.BasicSameGame.GameLogic
{
	internal class MatchRegistry
	{
		private readonly Vector2Int _size;

		private readonly int _minimumMatchCount;

		private int[] _map;

		private HashSet<int> _parentsOfEmpty;

		public int BiggestMatch { get; private set; }

		public int Count { get; private set; }

		public bool HasValidMoves => Count > 0;

		public MatchRegistry(Vector2Int size, int minimumMatchCount)
		{
			_size = size;
			_minimumMatchCount = minimumMatchCount;
		}

		public MatchRegistry(MatchRegistry toCopy)
		{
			_size = toCopy._size;
			_minimumMatchCount = toCopy._minimumMatchCount;

			_map = new int[toCopy._map.Length];
			Array.Copy(toCopy._map, _map, toCopy._map.Length);
			_parentsOfEmpty = new HashSet<int>(toCopy._parentsOfEmpty);

			BiggestMatch = toCopy.BiggestMatch;
			Count = toCopy.Count;
		}

		public void FindMatches(TileMap tileMap)
		{
			InitializeMap();
			FindDisjointSets(tileMap);
			CalculateProperties();
		}

		public bool TryGetMatchingGroup(Vector2Int cellPosition, out Vector2Int[] matchingGroup)
		{
			int targetParent = FindParent(cellPosition.x, cellPosition.y);
			int matchCount = -_map[targetParent];
			if (!_parentsOfEmpty.Contains(targetParent) && matchCount >= _minimumMatchCount) {

				List<Vector2Int> matches = new List<Vector2Int>();
				for (int i = 0; i < _map.Length; i++) {
					Vector2Int pos = GetCellPosition(i);
					int parent = FindParent(pos.x, pos.y);
					if (parent == targetParent) {
						matches.Add(pos);
					}
				}
				matchingGroup = matches.ToArray();
				return true;
			}
			matchingGroup = null;
			return false;
		}

		public Dictionary<Vector2Int, int> GetMatchRepresentatives()
		{
			Dictionary<Vector2Int, int> representatives = new Dictionary<Vector2Int, int>();
			BiggestMatch = 0;
			for (int i = 0; i < _map.Length; i++) {
				if (_map[i] < 0 && !_parentsOfEmpty.Contains(i)) {
					int matchCount = -_map[i];
					if (matchCount >= _minimumMatchCount) {
						BiggestMatch = Mathf.Max(BiggestMatch, matchCount);
						representatives.Add(GetCellPosition(i), matchCount);
					}
				}
			}
			return representatives;
		}

		private void InitializeMap()
		{
			_map = new int[_size.x * _size.y];
			for (int i = 0; i < _map.Length; i++) {
				_map[i] = -1;
			}
		}

		private void FindDisjointSets(TileMap tileMap)
		{
			_parentsOfEmpty = new HashSet<int>();
			for (int i = 0; i < _size.x; i++) {
				for (int j = 0; j < _size.y; j++) {

					Vector2Int me = new Vector2Int(i, j);
					int meParent = FindParent(i, j);

					if (j < _size.y - 1) {
						Vector2Int right = new Vector2Int(i, j + 1);
						if (tileMap[me] == tileMap[right]) {
							int rightParent = FindParent(i, j + 1);
							MergeSets(meParent, rightParent);
						}
					}

					if (i < _size.x - 1) {
						Vector2Int down = new Vector2Int(i + 1, j);
						if (tileMap[me] == tileMap[down]) {
							int downParent = FindParent(i + 1, j);
							MergeSets(meParent, downParent);
						}
					}

					if (tileMap[me] == 0) {
						_parentsOfEmpty.Add(meParent);
					}
				}
			}
		}

		private int FindParent(int posX, int posY)
		{
			int parent = (posX * _size.y) + posY;
			while (_map[parent] >= 0) {
				parent = _map[parent];
			}
			return parent;
		}

		private void MergeSets(int parent1, int parent2)
		{
			if (parent1 != parent2) {
				_map[parent1] += _map[parent2];
				_map[parent2] = parent1;
			}
		}

		private void CalculateProperties()
		{
			Count = 0;
			BiggestMatch = 0;
			for (int i = 0; i < _map.Length; i++) {
				if (_map[i] < 0 && !_parentsOfEmpty.Contains(i)) {
					int matchCount = -_map[i];
					if (matchCount >= _minimumMatchCount) {
						BiggestMatch = Mathf.Max(BiggestMatch, matchCount);
						Count++;
					}
				}
			}
		}

		private Vector2Int GetCellPosition(int i)
		{
			int x = i / _size.y;
			int y = i % _size.y;
			return new Vector2Int(x, y);
		}
	}
}
