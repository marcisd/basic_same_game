using System.Collections.Generic;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	using GameLogic;

	public class BestMovesSearch : MCTS.MonteCarloTreeSearch
	{
		public BestMovesSearch(SameGame sameGame, GameScorer scorer)
			: base(new TreeNode(new SameGame(sameGame), new GameScorer(scorer)),
				  new RandomSelectionPolicy(),
				  new GreedyExpansionHeuristic()) { }

		public new IEnumerable<Vector2Int> PerformSearch(int iterations)
		{
			base.PerformSearch(iterations);

			return (_root as TreeNode).GetPathToBestPlayout();
		}
	}
}
