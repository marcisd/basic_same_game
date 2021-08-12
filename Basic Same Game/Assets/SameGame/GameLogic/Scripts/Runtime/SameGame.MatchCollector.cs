using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSD.BasicSameGame.GameLogic
{
	public partial class SameGame
	{
		private class MatchCollector
		{
			private readonly Dictionary<int, List<Vector2Int>> _groups = new Dictionary<int, List<Vector2Int>>();

			private readonly Dictionary<Vector2Int, int> _groupMemberLookup = new Dictionary<Vector2Int, int>();

			private int _autoIndex = 0;

			public bool HasValidMoves => _groups.Count > 0;

			public Vector2Int[][] Groups => _groups.Select(pair => pair.Value.ToArray()).ToArray();

			public MatchCollector() { }

			public MatchCollector(MatchCollector matchCollector)
			{
				_groups = matchCollector._groups.ToDictionary(
					entry => entry.Key,
					entry => new List<Vector2Int>(entry.Value));
				_groupMemberLookup = new Dictionary<Vector2Int, int>(matchCollector._groupMemberLookup);
				_autoIndex = matchCollector._autoIndex;
			}

			public bool TryExtractMatchingGroup(Vector2Int cellPosition, out List<Vector2Int> matchingGroup)
			{
				if (_groupMemberLookup.TryGetValue(cellPosition, out int groupIndex)) {
					matchingGroup = _groups[groupIndex];
					_groups.Remove(groupIndex);
					return true;
				}
				matchingGroup = null;
				return false;
			}

			public void Reset()
			{
				_groups.Clear();
				_groupMemberLookup.Clear();
				_autoIndex = 0;
			}

			public void RegisterMatch(List<Vector2Int> matchingCells)
			{
				int index = _autoIndex++;
				_groups.Add(index, new List<Vector2Int>(matchingCells));
				foreach (Vector2Int cell in matchingCells) {
					_groupMemberLookup.Add(cell, index);
				}
			}
		}
	}
}
