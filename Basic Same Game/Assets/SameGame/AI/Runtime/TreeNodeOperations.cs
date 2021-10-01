using System.Collections.Generic;
using MSD.BasicSameGame.GameLogic;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	internal static class TreeNodeOperations
	{
		public static PlayoutResult PlayoutSimulation(SameGame sameGame, GameScorer scorer)
		{
			while (sameGame.HasValidMoves()) {
				Vector2Int[][] matches = sameGame.GetMatchingCells();
				int randomMatch = Random.Range(0, matches.Length - 1);
				int matchesCount = sameGame.DestroyMatchingTilesFromCell(matches[randomMatch][0]);
				scorer.RegisterMove(matchesCount);
			}

			return new PlayoutResult(
				scorer.TotalMoves,
				scorer.TotalScore,
				sameGame.TileCount);
		}

		public static IEnumerable<TreeNode> TraverseNodeForLeavesRecursively(TreeNode node)
		{
			if (node.IsTerminalNode) {
				yield break;
			}

			if (node.IsLeafNode) {
				yield return node;
			} else {
				foreach (TreeNode child in node.Children) {
					foreach (TreeNode leaf in TraverseNodeForLeavesRecursively(child)) {
						yield return leaf;
					}
				}
			}
		}

		public static void BackpropagateRecursively(TreeNode node)
		{
			if (node.IsRootNode) { return; }

			if (node.Parent.TryUpdateBestChild(node)) {
				BackpropagateRecursively(node.Parent);
			}
		}
	}
}
