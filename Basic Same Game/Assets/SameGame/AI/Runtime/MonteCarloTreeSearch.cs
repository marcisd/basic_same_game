using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace MSD.BasicSameGame.AI
{
	using GameLogic;

	public class MonteCarloTreeSearch
	{
		private readonly TreeNode _root;

		public MonteCarloTreeSearch(SameGame sameGame, GameScorer scorer)
		{
			_root = new TreeNode(new SameGame(sameGame), new GameScorer(scorer));
		}

		public IEnumerable<Vector2Int> PerformSearch(int iterations)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			int i = 0;
			for (; i < iterations; i++) {
				if (!TrySelection(out TreeNode leaf)) {
					Debug.Log("Exhausted all possible outcomes!");
					break;
				}
				TreeNode child = Expansion(leaf);
				Simulation(child);
				Backpropagation(child);
			}

			stopwatch.Stop();
			var elapsed = stopwatch.Elapsed;

			Debug.Log($"Performed search for {iterations} iterations. Finished in {i}.");
			Debug.Log($"Finished search in {elapsed.TotalSeconds} seconds.");
			Debug.Log("Returning best moves...");

			return _root.GetPathToBestPlayout();
		}

		/// <summary>
		/// Selection: Start from root R and select successive child nodes until a leaf node L is reached.
		/// The root is the current game state and a leaf is any node that has
		/// a potential child from which no simulation (playout) has yet been initiated.
		/// The section below says more about a way of biasing choice of child nodes that lets the game tree
		/// expand towards the most promising moves, which is the essence of Monte Carlo tree search.
		/// </summary>
		/// <returns>A leaf node L</returns>
		private bool TrySelection(out TreeNode leaf)
		{
			// TODO: Randomized policy for now. Add selection algorithm later
			// Selection should balance Exploration vs Expansion
			IEnumerable<TreeNode> nonTerminalLeaves = _root.GetNonTerminalLeaves();
			if (nonTerminalLeaves.Count() == 0) {
				leaf = null;
				return false;
			}
			TreeNode[] leavesArr = nonTerminalLeaves.ToArray();
			int rand = Random.Range(0, leavesArr.Length - 1);
			leaf = leavesArr[rand];
			Debug.Log($"Found {leavesArr.Length} leaves...");
			return true;
		}

		/// <summary>
		/// Expansion: Unless L ends the game decisively (e.g. win/loss/draw) for either player,
		/// create one (or more) child nodes and choose node C from one of them.
		/// Child nodes are any valid moves from the game position defined by L.
		/// </summary>
		/// <returns>From the expanded node L, select a child node C.</returns>
		private TreeNode Expansion(TreeNode leaf)
		{
			// TODO: Randomized for now. Add heuristics later
			leaf.Expand();
			int rand = Random.Range(0, leaf.Children.Count - 1);
			return leaf.Children[rand];
		}

		/// <summary>
		/// Simulation: Complete one random playout from node C.
		/// This step is sometimes also called playout or rollout.
		/// A playout may be as simple as choosing uniform random moves until the game is decided
		/// (for example in chess, the game is won, lost, or drawn).
		/// </summary>
		private void Simulation(TreeNode child)
		{
			child.Simulate();
		}

		/// <summary>
		/// Backpropagation: Use the result of the playout to update information in the nodes on the path from C to R.
		/// </summary>
		private void Backpropagation(TreeNode child)
		{
			child.Backpropagate();
		}
	}
}

