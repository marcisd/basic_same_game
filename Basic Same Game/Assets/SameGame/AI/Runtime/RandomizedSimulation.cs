using MSD.BasicSameGame.GameLogic;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	internal static class RandomizedSimulation
	{
		public static void PlayoutSimulation(SameGame sameGame, GameScorer scorer, out PlayoutResult playoutInfo)
		{
			while (sameGame.HasValidMoves()) {
				Vector2Int[][] matches = sameGame.GetMatchingCells();
				int randomMatch = Random.Range(0, matches.Length - 1);
				int matchesCount = sameGame.DestroyMatchingTilesFromCell(matches[randomMatch][0]);
				scorer.RegisterMove(matchesCount);
			}

			playoutInfo = new PlayoutResult(
				scorer.TotalMoves,
				scorer.TotalScore,
				sameGame.TileCount);
		}
	}
}
