using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	using GameLogic;

	internal class TreeNode
	{
		private readonly SameGame _sameGame;

		private readonly GameScorer _scorer;

		private readonly TreeNode _parent;

		private readonly bool _isTerminalNode;

		private List<TreeNode> _children;

		public bool IsRootNode => _parent == null;

		public bool IsLeafNode => _children == null || _children.Count == 0;

		public bool IsTerminalNode => _isTerminalNode;

		public TreeNode Parent => _parent;

		public IReadOnlyList<TreeNode> Children => _children;

		public Vector2Int? SelectedCell { get; }

		private PlayoutResult? Playout { get; set; }

		private TreeNode BestChild { get; set; }

		public TreeNode(SameGame sameGame, GameScorer scorer)
		{
			_sameGame = sameGame;
			_scorer = scorer;
			_isTerminalNode = !sameGame.HasValidMoves();
		}

		public TreeNode(SameGame sameGame, GameScorer scorer, TreeNode parent, Vector2Int selectedCell)
			: this(sameGame, scorer)
		{
			_parent = parent;
			SelectedCell = selectedCell;
		}

		public void Expand()
		{
			if (IsTerminalNode && !IsLeafNode) {
				throw new InvalidOperationException("Only non-terminal leaves can be expanded!");
			}

			Vector2Int[][] groups = _sameGame.GetMatchingCells();
			_children = new List<TreeNode>();

			foreach (Vector2Int[] group in groups) {
				SameGame gameCopy = new SameGame(_sameGame);
				GameScorer scorerCopy = new GameScorer(_scorer);
				Vector2Int selectedCell = group[0];
				int matchesCount = gameCopy.DestroyMatchingTilesFromCell(selectedCell);
				scorerCopy.RegisterMove(matchesCount);

				TreeNode child = new TreeNode(gameCopy, scorerCopy, this, selectedCell);
				_children.Add(child);

				if (child.IsTerminalNode) {
					Playout = new PlayoutResult(
						child._scorer.TotalMoves,
						child._scorer.TotalScore,
						child._sameGame.TileCount);
				}
			}
		}

		public IEnumerable<TreeNode> GetLeaves()
		{
			return TraverseNodeForLeavesRecursively(this);
		}

		public static void BackPropagate(TreeNode leafNode)
		{
			if (leafNode == null) { throw new ArgumentNullException(nameof(leafNode)); }

			if (!leafNode.IsLeafNode) { throw new ArgumentException("Must be a leaf node!", nameof(leafNode)); }

			if (leafNode.Playout == null) { throw new ArgumentException("Leaf node must have a playout information to backpropagate!"); }

			BackPropagateRecursively(leafNode);
		}

		private static IEnumerable<TreeNode> TraverseNodeForLeavesRecursively(TreeNode node)
		{
			if (node.IsTerminalNode) {
				yield break;
			}

			if (node.IsLeafNode) {
				yield return node;
			} else {
				foreach (TreeNode child in node._children) {
					foreach (var leaf in TraverseNodeForLeavesRecursively(child)) {
						yield return leaf;
					}
				}
			}
		}

		private static void BackPropagateRecursively(TreeNode node)
		{
			if (node.IsRootNode) { return; }

			if (node.Parent.TryUpdateBestChild(node)) {
				BackPropagateRecursively(node.Parent);
			}
		}

		private bool TryUpdateBestChild(TreeNode child)
		{
			if (!_children.Contains(child)) { throw new ArgumentException("Not a child of this node.", nameof(child)); }

			if (BestChild == null || BestChild.Playout.Value.TotalScore > child.Playout.Value.TotalScore) {
				BestChild = child;
				Playout = child.Playout;
				return true;
			}
			return false;
		}
	}
}
