using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	using GameLogic;

	internal class TreeNode
	{
		public class MoveProperties
		{
			public Vector2Int SelectedCell { get; }
			public int Depth { get; }

			public MoveProperties(Vector2Int selectedCell, int depth)
			{
				SelectedCell = selectedCell;
				Depth = depth;
			}
		}

		private readonly SameGame _sameGame;

		private readonly TreeNode _parent;

		private readonly bool _isTerminalNode;

		private List<TreeNode> _children;

		public MoveProperties properties { get; }

		public bool IsRootNode => _parent == null;

		public bool IsLeafNode => _children == null || _children.Count == 0;

		public bool IsTerminalNode => _isTerminalNode;

		public TreeNode Parent => _parent;

		public IReadOnlyList<TreeNode> Children => _children;

		private PlayoutInfo? Playout { get; set; }

		private TreeNode BestChild { get; set; }

		public TreeNode(SameGame sameGame)
		{
			_sameGame = sameGame;
			_isTerminalNode = !sameGame.HasValidMoves();
		}

		public TreeNode(SameGame sameGame, TreeNode parent, Vector2Int selectedCell, int depth)
			: this(sameGame)
		{
			_parent = parent;
			properties = new MoveProperties(selectedCell, depth);
		}

		public void Expand()
		{
			if (IsTerminalNode && !IsLeafNode) {
				throw new InvalidOperationException("Only non-terminal leaves can be expanded!");
			}

			Vector2Int[][] groups = _sameGame.GetMatchingCells();
			_children = new List<TreeNode>();
			int childDepth = properties != null ? properties.Depth + 1 : 1;

			foreach (Vector2Int[] group in groups) {
				SameGame copy = new SameGame(_sameGame);
				Vector2Int selectedCell = group[0];
				copy.DestroyMatchingTilesFromCell(selectedCell);

				TreeNode child = new TreeNode(copy, this, selectedCell, childDepth);
				_children.Add(child);

				if (child.IsTerminalNode) {
					Playout = new PlayoutInfo(childDepth, child._sameGame.TileCount);
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

			if (BestChild == null || BestChild.Playout.Value.Score > child.Playout.Value.Score) {
				BestChild = child;
				Playout = child.Playout;
				return true;
			}
			return false;
		}
	}
}
