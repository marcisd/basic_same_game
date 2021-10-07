using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	using GameLogic;

	internal class TreeNode
	{
		private readonly SameGame _sameGame;

		private readonly GameScorer _scorer;

		private List<TreeNode> _children;

		private List<TreeNode> _nonTerminalLeavesCache;

		public bool IsRootNode => Parent == null;

		public bool IsLeafNode => _children == null || _children.Count == 0;

		public IReadOnlyList<TreeNode> Children => _children;

		public bool IsTerminalNode { get; }

		public TreeNode Parent { get; }

		public Vector2Int? SelectedCellFromPatent { get; }

		private TreeNode BestChild { get; set; }

		private IEnumerable<Vector2Int> SimulationResult { get; set; }

		private PlayoutResult? Playout { get; set; }

		public TreeNode(SameGame sameGame, GameScorer scorer)
		{
			_sameGame = sameGame;
			_scorer = scorer;

			if (!sameGame.HasValidMoves()) {
				IsTerminalNode = true;

				Playout = new PlayoutResult(
					_scorer.TotalMoves,
					_scorer.TotalScore,
					_sameGame.TileCount);
			}
		}

		public TreeNode(SameGame sameGame, GameScorer scorer, TreeNode parent, Vector2Int selectedCell)
			: this(sameGame, scorer)
		{
			Parent = parent;
			SelectedCellFromPatent = selectedCell;
		}

		public IEnumerable<TreeNode> GetNonTerminalLeaves()
		{
			if (_nonTerminalLeavesCache != null) {
				UpdateNonTerminalLeavesCache(ref _nonTerminalLeavesCache);
			} else {
				_nonTerminalLeavesCache = TreeNodeOperations.TraverseNodeForNonTerminalLeavesRecursively(this).ToList();
			}
			return _nonTerminalLeavesCache;
		}

		public void Expand()
		{
			if (IsTerminalNode && !IsLeafNode) { throw new InvalidOperationException("Only non-terminal leaf nodes can be expanded!"); }

			Vector2Int[][] groups = _sameGame.GetMatchingCells();
			_children = new List<TreeNode>();

			SimulationResult = null;

			foreach (Vector2Int[] group in groups) {
				CreateChildNode(group[0]);
			}
		}

		public void Simulate()
		{
			if (IsTerminalNode && !IsLeafNode) { throw new InvalidOperationException("Only non-terminal leaf nodes can be simulated!"); }

			SameGame gameCopy = new SameGame(_sameGame);
			GameScorer scorerCopy = new GameScorer(_scorer);

			SimulationResult = TreeNodeOperations.PlayoutRandomSimulation(gameCopy, scorerCopy);

			Playout = new PlayoutResult(
					scorerCopy.TotalMoves,
					scorerCopy.TotalScore,
					gameCopy.TileCount);
		}

		public void Backpropagate()
		{
			if (!IsLeafNode) { throw new InvalidOperationException("Backpropagation must start on a leaf node!"); }

			if (Playout == null) { throw new InvalidOperationException("Starting leaf node must have a playout information to backpropagate!"); }

			BackpropagateRecursively(this);
		}

		public IEnumerable<Vector2Int> GetPathToBestPlayout()
		{
			if (!IsRootNode) { throw new InvalidOperationException("Operation must start from the root node!"); }

			List<Vector2Int> path = new List<Vector2Int>();
			GetPathToBestPlayoutRecursively(this, ref path);

			return path;
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

		private void CreateChildNode(Vector2Int matchMember)
		{
			SameGame gameCopy = new SameGame(_sameGame);
			int matchesCount = gameCopy.DestroyMatchingTilesFromCell(matchMember);

			if (matchesCount == 0) { throw new ArgumentException("Prameter should trigger a valid match!", nameof(matchMember)); }

			GameScorer scorerCopy = new GameScorer(_scorer);
			scorerCopy.RegisterMove(matchesCount);

			TreeNode child = new TreeNode(gameCopy, scorerCopy, this, matchMember);
			_children.Add(child);

			if (child.IsTerminalNode) {
				Debug.Log("Created terminal child. Backpropagating...");
				BackpropagateRecursively(child);
			}
		}

		private static void BackpropagateRecursively(TreeNode node)
		{
			if (node.IsRootNode) { return; }

			if (node.Parent.TryUpdateBestChild(node)) {
				BackpropagateRecursively(node.Parent);
			}
		}

		private static void GetPathToBestPlayoutRecursively(TreeNode node, ref List<Vector2Int> path)
		{
			if (node.IsTerminalNode) { return; }

			if (node.SimulationResult != null) {
				path.AddRange(node.SimulationResult);
				return;
			}

			path.Add(node.BestChild.SelectedCellFromPatent.Value);

			GetPathToBestPlayoutRecursively(node.BestChild, ref path);
		}

		private static void UpdateNonTerminalLeavesCache(ref List<TreeNode> nodes)
		{
			List<TreeNode> newLeaves = new List<TreeNode>();
			List<TreeNode> leavesToRemove = new List<TreeNode>();
			foreach (TreeNode node in nodes) {
				if (!node.IsLeafNode) {
					leavesToRemove.Add(node);
					IEnumerable<TreeNode> relativeLeaves = TreeNodeOperations.TraverseNodeForNonTerminalLeavesRecursively(node);
					newLeaves.AddRange(relativeLeaves);
				}
			}

			if (newLeaves.Count > 0 || leavesToRemove.Count > 0) {
				nodes = nodes.Except(leavesToRemove).Union(newLeaves).ToList();
			}
		}
	}
}
