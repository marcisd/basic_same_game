using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	using GameLogic;

	/// <summary>
	/// Greedy best-first search algorithm always selects the path which appears best at that moment.
	/// It is the combination of depth-first search and breadth-first search algorithms.
	/// It uses the heuristic function and search. Best-first search allows us to take the advantages of both algorithms.
	/// With the help of best-first search, at each step, we can choose the most promising node.
	/// </summary>
	internal class GreedyExpansionHeuristic : MCTS.IExpansionHeuristic
	{
		private class NextMoveState
		{
			public SameGame gameCopy { get; }
			public Vector2Int selectedCell { get; }
			public int matchesCount { get; }

			public NextMoveState(SameGame gameCopy, Vector2Int selectedCell, int matchesCount)
			{
				this.gameCopy = gameCopy;
				this.selectedCell = selectedCell;
				this.matchesCount = matchesCount;
			}
		}

		public void Expansion(TreeNode nonTerminalLeaf)
		{
			nonTerminalLeaf.RemoveSimulationResult();

			List<NextMoveState> forExpansion = GetStatesForExpansion(nonTerminalLeaf);

			foreach (NextMoveState item in forExpansion) {
				CreateChildNode(item, nonTerminalLeaf);
			}

			if (nonTerminalLeaf.Degree == 0) {
				nonTerminalLeaf.SetTerminal();
			}
		}

		private List<NextMoveState> GetStatesForExpansion(TreeNode leaf)
		{
			Dictionary<Vector2Int, int> representatives = leaf.sameGame.GetMatchRepresentatives();
			IEnumerable<KeyValuePair<Vector2Int, int>> tabuChoices = representatives.Where(rep => rep.Value < leaf.sameGame.BiggestMatch);

			if (!tabuChoices.Any()) {

				return representatives.Select(rep => {
					SameGame gameCopy = new SameGame(leaf.sameGame);
					int matchCount = gameCopy.DestroyMatchingTilesFromCell(rep.Key);
					return new NextMoveState(gameCopy, rep.Key, matchCount);
				}).ToList();
			} else {
				List<NextMoveState> forExpansionGreater = new List<NextMoveState>();
				List<NextMoveState> forExpansionEqual = new List<NextMoveState>();

				foreach (KeyValuePair<Vector2Int, int> rep in tabuChoices) {
					SameGame gameCopy = new SameGame(leaf.sameGame);
					int matchCount = gameCopy.DestroyMatchingTilesFromCell(rep.Key);

					if (gameCopy.BiggestMatch > leaf.sameGame.BiggestMatch) {
						forExpansionGreater.Add(new NextMoveState(gameCopy, rep.Key, matchCount));
					} else if (gameCopy.BiggestMatch == leaf.sameGame.BiggestMatch) {
						forExpansionEqual.Add(new NextMoveState(gameCopy, rep.Key, matchCount));
					}
				}

				return forExpansionGreater.Count > 0 ? forExpansionGreater : forExpansionEqual;
			}
		}

		private void CreateChildNode(NextMoveState nextMoveState, TreeNode leaf)
		{
			GameScorer scorerCopy = new GameScorer(leaf.scorer);
			scorerCopy.RegisterMove(nextMoveState.matchesCount);
			TreeNode child = new TreeNode(nextMoveState.gameCopy, scorerCopy, leaf, nextMoveState.selectedCell);

			if (child.IsTerminalNode) {
				Debug.Log("Created a terminal leaf child. Backpropagating...");
				child.Backpropagate();
			}
		}

		void MCTS.IExpansionHeuristic.Expansion(MCTS.MCTSTreeNode nonTerminalLeaf)
		{
			Expansion(nonTerminalLeaf as TreeNode);
		}
	}
}
