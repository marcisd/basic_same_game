using System.Collections.Generic;
using MSD.BasicSameGame.GameLogic;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	internal static class TreeNodeOperations
	{
		public static IEnumerable<Vector2Int> PlayoutRandomSimulation(SameGame sameGame, GameScorer scorer)
		{
			while (sameGame.HasValidMoves()) {
				Vector2Int[][] matches = sameGame.GetMatchingCells();
				int randomMatch = Random.Range(0, matches.Length - 1);
				Vector2Int randomCell = matches[randomMatch][0];
				yield return randomCell;
				int matchesCount = sameGame.DestroyMatchingTilesFromCell(matches[randomMatch][0]);
				scorer.RegisterMove(matchesCount);
			}
		}

		public static IEnumerable<TreeNode> TraverseNodeForNonTerminalLeavesRecursively(TreeNode node)
		{
			if (node.IsTerminalNode) {
				yield break;
			}

			if (node.IsLeafNode) {
				yield return node;
			} else {
				foreach (TreeNode child in node.Children) {
					foreach (TreeNode leaf in TraverseNodeForNonTerminalLeavesRecursively(child)) {
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

		public static IEnumerable<Vector2Int> TraverseBestChild(TreeNode node)
		{
			while (node.BestChild != null && node.BestChild.SelectedCell.HasValue) {
				yield return node.BestChild.SelectedCell.Value;
				node = node.BestChild;
			}

			foreach (var sim in node.SimulationResult) {
				yield return sim;
			}
		}
	}
}
