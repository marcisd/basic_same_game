using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using Random = System.Random;

namespace MSD.MCTS
{
	public abstract class MonteCarloTreeSearch
	{
		private readonly ISelectionPolicy _selectionPolicy;

		private readonly IExpansionHeuristic _expansionHeuristic;

		protected readonly MCTSTreeNode _root;

		protected MonteCarloTreeSearch(MCTSTreeNode root,
			ISelectionPolicy selectionPolicy,
			IExpansionHeuristic expansionHeuristic)
		{
			_root = root;
			_selectionPolicy = selectionPolicy;
			_expansionHeuristic = expansionHeuristic;
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
				if (TryExpansion(leaf, out MCTSTreeNode child)) {
					Simulation(child);
					Backpropagation(child);
				}
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
		/// <param name="leaf">The selected node L.</param>
		/// <returns><c>true</c> if the selection yielded a result; <c>false</c> otherwise.</returns>
		private bool TrySelection(out MCTSTreeNode leaf)
		{
			IEnumerable<MCTSTreeNode> nonTerminalLeaves = _root.GetNonTerminalLeaves();
			if (nonTerminalLeaves.Any()) {
				leaf = _selectionPolicy.Selection(nonTerminalLeaves);
				return true;
			}
			leaf = null;
			return false;
		}

		/// <summary>
		/// Expansion: Unless L ends the game decisively (e.g. win/loss/draw) for either player,
		/// create one (or more) child nodes and choose node C from one of them.
		/// Child nodes are any valid moves from the game position defined by L.
		/// </summary>
		/// <param name="leaf">The selected node L.</param>
		/// <param name="child">A child from the expanded node L.</param>
		/// <returns><c>true</c> if the leaf node L was expanded; <c>false</c> otherwise.</returns>
		private bool TryExpansion(MCTSTreeNode leaf, out MCTSTreeNode child)
		{
			if (leaf.IsTerminalNode && !leaf.IsLeafNode) { throw new InvalidOperationException("Only non-terminal leaf nodes can be expanded!"); }

			_expansionHeuristic.Expansion(leaf);

			if (leaf.Degree > 0) {
				Random random = new Random();
				int randomInt = random.Next(leaf.Degree);
				child = leaf.Children[randomInt] as MCTSTreeNode;
				return true;
			}
			Debug.Log("Aborted expansion on an undesirable candidate.");
			child = null;
			return false;
		}

		/// <summary>
		/// Simulation: Complete one random playout from node C.
		/// This step is sometimes also called playout or rollout.
		/// A playout may be as simple as choosing uniform random moves until the game is decided
		/// (for example in chess, the game is won, lost, or drawn).
		/// </summary>
		/// <param name="child">A child from the expanded node L.</param>
		private void Simulation(MCTSTreeNode child)
		{
			child.Simulate();
		}

		/// <summary>
		/// Backpropagation: Use the result of the playout to update information in the nodes on the path from C to R.
		/// </summary>
		/// <param name="child">A child from the expanded node L.</param>
		private void Backpropagation(MCTSTreeNode child)
		{
			child.Backpropagate();
		}
	}
}
