using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MSD.BasicSameGame.AI
{
	using GameLogic;

	internal class TreeNode : MCTS.MCTSTreeNode
	{
		private readonly SameGame _sameGame;

		private readonly GameScorer _scorer;

		public new TreeNode Parent => base.Parent as TreeNode;

		public Vector2Int? SelectedCellFromPatent { get; }

		private TreeNode BestChild { get; set; }

		private IEnumerable<Vector2Int> SimulationResult { get; set; }

		private PlayoutResult? Playout { get; set; }

		public TreeNode(SameGame sameGame, GameScorer scorer) : this(sameGame, scorer, null, default) { }

		public TreeNode(SameGame sameGame, GameScorer scorer, TreeNode parent, Vector2Int selectedCell)
			: base (parent)
		{
			_sameGame = sameGame;
			_scorer = scorer;

			if (parent != null) {
				SelectedCellFromPatent = selectedCell;
			}

			if (!sameGame.HasValidMoves()) {
				IsTerminalNode = true;

				Playout = new PlayoutResult(
					_scorer.TotalMoves,
					_scorer.TotalScore,
					_sameGame.TileCount);
			}
		}

		public override void Expand()
		{
			if (IsTerminalNode && !IsLeafNode) { throw new InvalidOperationException("Only non-terminal leaf nodes can be expanded!"); }

			Vector2Int[][] groups = _sameGame.GetMatchingCells();

			SimulationResult = null;

			foreach (Vector2Int[] group in groups) {
				CreateChildNode(group[0]);
			}
		}

		public override void Simulate()
		{
			if (IsTerminalNode && !IsLeafNode) { throw new InvalidOperationException("Only non-terminal leaf nodes can be simulated!"); }

			SameGame gameCopy = new SameGame(_sameGame);
			GameScorer scorerCopy = new GameScorer(_scorer);

			SimulationResult = PlayoutRandomSimulation(gameCopy, scorerCopy);

			Playout = new PlayoutResult(
					scorerCopy.TotalMoves,
					scorerCopy.TotalScore,
					gameCopy.TileCount);
		}

		public override void Backpropagate()
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
			if (!Children.Contains(child)) { throw new ArgumentException("Not a child of this node.", nameof(child)); }

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

			if (child.IsTerminalNode) {
				Debug.Log("Created a terminal leaf child. Backpropagating...");
				BackpropagateRecursively(child);
			}
		}

		private static IEnumerable<Vector2Int> PlayoutRandomSimulation(SameGame sameGame, GameScorer scorer)
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
	}
}
