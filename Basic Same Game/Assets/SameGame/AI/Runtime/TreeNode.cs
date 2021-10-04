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

		private List<TreeNode> _children;

		public bool IsRootNode => Parent == null;

		public bool IsLeafNode => _children == null || _children.Count == 0;

		public IReadOnlyList<TreeNode> Children => _children;

		public TreeNode Parent { get; }

		public bool IsTerminalNode { get; }

		public Vector2Int? SelectedCell { get; }

		public TreeNode BestChild { get; private set; }

		public IEnumerable<Vector2Int> SimulationResult { get; private set; }

		private PlayoutResult? Playout { get; set; }

		public TreeNode(SameGame sameGame, GameScorer scorer)
		{
			_sameGame = sameGame;
			_scorer = scorer;

			if (!sameGame.HasValidMoves()) {
				IsTerminalNode = true;

				// TODO: need to be considered during backpropagation
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
			SelectedCell = selectedCell;
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

		public IEnumerable<TreeNode> GetNonTerminalLeaves()
		{
			return TreeNodeOperations.TraverseNodeForNonTerminalLeavesRecursively(this);
		}

		public void Backpropagate()
		{
			if (!IsLeafNode) { throw new InvalidOperationException("Backpropagation must start on a leaf node!"); }

			if (Playout == null) { throw new InvalidOperationException("Starting leaf node must have a playout information to backpropagate!"); }

			TreeNodeOperations.BackpropagateRecursively(this);
		}

		internal bool TryUpdateBestChild(TreeNode child)
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
		}
	}
}
