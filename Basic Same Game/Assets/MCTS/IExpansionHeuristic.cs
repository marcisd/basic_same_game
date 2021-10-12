
namespace MSD.MCTS
{
	/// <summary>
	/// Heuristics is a technique of prioritizing certain computation paths over others when looking for a problem solution.
	/// When an algorithm uses a heuristic, it no longer needs to exhaustively search every possible solution,
	/// so it can find approximate solutions more quickly. A heuristic is a shortcut that sacrifices accuracy and completeness.
	/// </summary>
	public interface IExpansionHeuristic<TMCTSTreeNode> where TMCTSTreeNode : MCTSTreeNode
	{
		void Expansion(ref TMCTSTreeNode nonTerminalLeaf);
	}
}