using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace MCTS
{
	public abstract class MonteCarloTreeSearch
	{
		protected readonly MCTSTreeNode _root;

		public MonteCarloTreeSearch(MCTSTreeNode root)
		{
			_root = root;
		}

		public void PerformSearch(int iterations)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			int i = 0;
			for (; i < iterations; i++) {
				if (!TrySelection(out MCTSTreeNode leaf)) {
					Debug.Log("Exhausted all possible outcomes!");
					break;
				}
				MCTSTreeNode child = Expansion(leaf);
				Simulation(child);
				Backpropagation(child);
			}

			stopwatch.Stop();
			var elapsed = stopwatch.Elapsed;

			Debug.Log($"Performed search for {iterations} iterations. Finished in {i}.");
			Debug.Log($"Finished search in {elapsed.TotalSeconds} seconds.");
		}

		/// <summary>
		/// Selection: Start from root R and select successive child nodes until a leaf node L is reached.
		/// The root is the current game state and a leaf is any node that has
		/// a potential child from which no simulation (playout) has yet been initiated.
		/// The section below says more about a way of biasing choice of child nodes that lets the game tree
		/// expand towards the most promising moves, which is the essence of Monte Carlo tree search.
		/// </summary>
		/// <returns>A leaf node L</returns>
		protected abstract bool TrySelection(out MCTSTreeNode leaf);

		/// <summary>
		/// Expansion: Unless L ends the game decisively (e.g. win/loss/draw) for either player,
		/// create one (or more) child nodes and choose node C from one of them.
		/// Child nodes are any valid moves from the game position defined by L.
		/// </summary>
		/// <returns>From the expanded node L, select a child node C.</returns>
		private MCTSTreeNode Expansion(MCTSTreeNode leaf)
		{
			leaf.Expand();
			int rand = Random.Range(0, leaf.Children.Count - 1);
			return leaf.Children[rand] as MCTSTreeNode;
		}

		/// <summary>
		/// Simulation: Complete one random playout from node C.
		/// This step is sometimes also called playout or rollout.
		/// A playout may be as simple as choosing uniform random moves until the game is decided
		/// (for example in chess, the game is won, lost, or drawn).
		/// </summary>
		private void Simulation(MCTSTreeNode child)
		{
			child.Simulate();
		}

		/// <summary>
		/// Backpropagation: Use the result of the playout to update information in the nodes on the path from C to R.
		/// </summary>
		private void Backpropagation(MCTSTreeNode child)
		{
			child.Backpropagate();
		}
	}
}
