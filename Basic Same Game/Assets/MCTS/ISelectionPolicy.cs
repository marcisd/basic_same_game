using System.Collections.Generic;

namespace MSD.MCTS
{
	/// <summary>
	/// A tree policy is an informed policy used for action (node) selection in the snowcap
	/// (explored part of the game tree as opposed to the vast unexplored bottom part).
	/// One important consideration of such policy is the balance of exploration vs exploitation.
	/// </summary>
	public interface ISelectionPolicy
	{
		MCTSTreeNode Selection(IEnumerable<MCTSTreeNode> nonTerminalLeaves);
	}
}