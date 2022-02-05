using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MSD.BasicSameGame.AI
{
	using GameLogic;

	internal class TreeNode : MCTS.MCTSTreeNode
	{
		private TreeNode _bestChild;

		private IEnumerable<Vector2Int> _simulationResult;

		private PlayoutResult? _playout;

		public new TreeNode Parent => base.Parent as TreeNode;

		public SameGame sameGame { get; }

		public GameScorer scorer { get; }

		public Vector2Int? SelectedCellFromPatent { get; }
		
		public TreeNode(SameGame sameGame, GameScorer scorer) : this(sameGame, scorer, null, default) { }

		public TreeNode(SameGame sameGame, GameScorer scorer, TreeNode parent, Vector2Int selectedCell)
			: base (parent)
		{
			this.sameGame = sameGame;
			this.scorer = scorer;

			if (parent != null) {
				SelectedCellFromPatent = selectedCell;
			}

			if (!sameGame.HasValidMoves) {
				IsTerminalNode = true;

				_playout = new PlayoutResult(
					scorer.TotalMoves,
					scorer.TotalScore,
					sameGame.TileCount);
			}
		}

		public void RemoveSimulationResult()
		{
			_simulationResult = null;
		}

		public override void Simulate()
		{
			if (IsTerminalNode && !IsLeafNode) { throw new InvalidOperationException("Only non-terminal leaf nodes can be simulated!"); }

			SameGame gameCopy = new SameGame(sameGame);
			GameScorer scorerCopy = new GameScorer(scorer);

			_simulationResult = PlayoutRandomSimulation(gameCopy, scorerCopy);

			_playout = new PlayoutResult(
					scorerCopy.TotalMoves,
					scorerCopy.TotalScore,
					gameCopy.TileCount);
		}

		public override void Backpropagate()
		{
			if (!IsLeafNode) { throw new InvalidOperationException("Backpropagation must start on a leaf node!"); }

			if (_playout == null) { throw new InvalidOperationException("Starting leaf node must have a playout information to backpropagate!"); }

			Backpropagate(this);
		}

		public IEnumerable<Vector2Int> GetPathToBestPlayout()
		{
			if (!IsRootNode) { throw new InvalidOperationException("Operation must start from the root node!"); }

			List<Vector2Int> path = GetPathToBestPlayout(this);

			return path;
		}

		private bool TryUpdateBestChild(TreeNode child)
		{
			if (!Children.Contains(child)) { throw new ArgumentException("Not a child of this node.", nameof(child)); }

			if (_bestChild == null || _bestChild._playout.Value.TotalScore > child._playout.Value.TotalScore) {
				_bestChild = child;
				_playout = child._playout;
				return true;
			}
			return false;
		}

		private static IEnumerable<Vector2Int> PlayoutRandomSimulation(SameGame sameGame, GameScorer scorer)
		{
			while (sameGame.HasValidMoves) {
				Vector2Int[] matches = sameGame.GetMatchRepresentatives().Keys.ToArray();
				int randomMatch = Random.Range(0, matches.Length - 1);
				Vector2Int randomCell = matches[randomMatch];
				yield return randomCell;
				int matchesCount = sameGame.DestroyMatchingTilesFromCell(matches[randomMatch]);
				scorer.RegisterMove(matchesCount);
			}
		}

		private static void Backpropagate(TreeNode node)
		{
			while (!node.IsRootNode) {

				if (!node.Parent.TryUpdateBestChild(node)) {
					break;
				}

				node = node.Parent;
			}
		}

		private static List<Vector2Int> GetPathToBestPlayout(TreeNode node)
		{
			List<Vector2Int> path = new List<Vector2Int>();

			while (!node.IsTerminalNode) {

				if (node._simulationResult != null) {
					path.AddRange(node._simulationResult);
					break;
				}

				path.Add(node._bestChild.SelectedCellFromPatent.Value);

				node = node._bestChild;
			}

			return path;
		}
	}
}
