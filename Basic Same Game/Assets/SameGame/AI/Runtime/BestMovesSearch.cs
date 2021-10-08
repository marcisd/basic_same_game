using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	using GameLogic;

	public class BestMovesSearch : MCTS.MonteCarloTreeSearch
	{
		public BestMovesSearch(SameGame sameGame, GameScorer scorer)
			: base(new TreeNode(new SameGame(sameGame), new GameScorer(scorer))) { }

		public new IEnumerable<Vector2Int> PerformSearch(int iterations)
		{
			base.PerformSearch(iterations);

			return (_root as TreeNode).GetPathToBestPlayout();
		}

		protected override bool TrySelection(out MCTS.MCTSTreeNode leaf)
		{
			IEnumerable<MCTS.MCTSTreeNode> nonTerminalLeaves = _root.GetNonTerminalLeaves();
			if (nonTerminalLeaves.Count() == 0) {
				leaf = null;
				return false;
			}
			MCTS.MCTSTreeNode[] leavesArr = nonTerminalLeaves.ToArray();
			int rand = Random.Range(0, leavesArr.Length - 1);
			leaf = leavesArr[rand];
			return true;
		}

	}
}

