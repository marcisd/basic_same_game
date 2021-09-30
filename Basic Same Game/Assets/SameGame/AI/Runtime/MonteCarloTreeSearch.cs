using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	using GameLogic;

	public class MonteCarloTreeSearch
	{
		private readonly TreeNode _root;

		public MonteCarloTreeSearch(SameGame sameGame)
		{
			_root = new TreeNode(new SameGame(sameGame));
		}

		public void PerformSearch(int iterations)
		{
			for (int i = 0; i < iterations; i++) {
				var L = Selection();
				var C = Expansion(L);
			}

			Debug.Log("test");
		}

		/// <summary>
		/// Selection: Start from root R and select successive child nodes until a leaf node L is reached.
		/// The root is the current game state and a leaf is any node that has
		/// a potential child from which no simulation (playout) has yet been initiated.
		/// The section below says more about a way of biasing choice of child nodes that lets the game tree
		/// expand towards the most promising moves, which is the essence of Monte Carlo tree search.
		/// </summary>
		/// <returns>A leaf node L</returns>
		private TreeNode Selection()
		{
			// TODO: add heuristics later
			IEnumerable<TreeNode> leaves = _root.GetLeaves();
			var tempLeaves = leaves.ToArray();
			return leaves.First();
		}

		/// <summary>
		/// Expansion: Unless L ends the game decisively (e.g. win/loss/draw) for either player,
		/// create one (or more) child nodes and choose node C from one of them.
		/// Child nodes are any valid moves from the game position defined by L.
		/// </summary>
		/// <returns>From the expanded node L, select a child node C.</returns>
		private TreeNode Expansion(TreeNode L)
		{
			L.Expand();
			// TODO: just return the first child for now
			return L.Children[0];
		}

		/// <summary>
		/// Simulation: Complete one random playout from node C.
		/// This step is sometimes also called playout or rollout.
		/// A playout may be as simple as choosing uniform random moves until the game is decided
		/// (for example in chess, the game is won, lost, or drawn).
		/// </summary>
		private void Simulation()
		{
		}

		/// <summary>
		/// Backpropagation: Use the result of the playout to update information in the nodes on the path from C to R.
		/// </summary>
		private void BackPropagation()
		{
		}
	}
}

