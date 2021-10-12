using System;
using System.Linq;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	using GameLogic;

	internal class TabuExpansionHeuristic : MCTS.IExpansionHeuristic<TreeNode>
	{
		public void Expansion(ref TreeNode nonTerminalLeaf)
		{
			throw new System.NotImplementedException();
		}

		private void Expand(TreeNode leaf)
		{
			if (leaf.IsTerminalNode && !leaf.IsLeafNode) { throw new InvalidOperationException("Only non-terminal leaf nodes can be expanded!"); }

			Vector2Int[][] groups = leaf.sameGame.GetMatchingCells();
			var tabuChoices = groups.Where(group => group.Length < leaf.sameGame.BiggestMatch);
			int biggestMatch = leaf.sameGame.BiggestMatch;

			leaf.RemoveSimulationResult();

			if (tabuChoices.Any()) {
				// TODO
			} else {
				// create child for all groups
				foreach (Vector2Int[] group in groups) {
					CreateChildNode(group, leaf);
				}
			}

			Debug.Log($"Only accepting {leaf.Degree} out of {leaf.sameGame.MatchesCount} matches found.");
		}

		private void CreateChildNode(Vector2Int[] matchGroup, TreeNode treeNode)
		{
			Vector2Int matchMember = matchGroup[0];

			SameGame gameCopy = new SameGame(treeNode.sameGame);
			int matchesCount = gameCopy.DestroyMatchingTilesFromCell(matchMember);

			if (matchesCount == 0) { throw new ArgumentException("Prameter should trigger a valid match!", nameof(matchMember)); }

			GameScorer scorerCopy = new GameScorer(treeNode.scorer);
			scorerCopy.RegisterMove(matchesCount);

			TreeNode child = new TreeNode(gameCopy, scorerCopy, treeNode, matchMember);

			if (child.IsTerminalNode) {
				Debug.Log("Created a terminal leaf child. Backpropagating...");
				child.Backpropagate();
			}
		}
	}
}
