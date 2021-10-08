using System.Collections.Generic;
using System.Linq;

namespace MCTS
{
	public abstract class MCTSTreeNode : TreeNode
	{
		private List<MCTSTreeNode> _nonTerminalLeavesCache;

		public bool IsTerminalNode { get; protected set; }

		protected MCTSTreeNode(MCTSTreeNode parent) : base(parent) { }

		public IEnumerable<MCTSTreeNode> GetNonTerminalLeaves()
		{
			if (_nonTerminalLeavesCache == null) {
				_nonTerminalLeavesCache = TraverseNodeForNonTerminalLeavesRecursively(this).ToList();
			} else {
				UpdateNonTerminalLeavesCache(ref _nonTerminalLeavesCache);
			}
			return _nonTerminalLeavesCache;
		}

		public bool TryExpand()
		{
			Expand();
			IsTerminalNode = Degree == 0;
			return !IsTerminalNode;
		}

		protected abstract void Expand();

		public abstract void Simulate();

		public abstract void Backpropagate();

		private static IEnumerable<MCTSTreeNode> TraverseNodeForNonTerminalLeavesRecursively(MCTSTreeNode node)
		{
			if (node.IsTerminalNode) {
				yield break;
			}

			if (node.IsLeafNode) {
				yield return node;
			} else {
				foreach (MCTSTreeNode child in node.Children) {
					foreach (MCTSTreeNode leaf in TraverseNodeForNonTerminalLeavesRecursively(child)) {
						yield return leaf;
					}
				}
			}
		}

		private static void UpdateNonTerminalLeavesCache(ref List<MCTSTreeNode> nodes)
		{
			List<MCTSTreeNode> newLeaves = new List<MCTSTreeNode>();
			IEnumerable<MCTSTreeNode> leavesToRemove = nodes.Where(n => !n.IsLeafNode);
			foreach (MCTSTreeNode oldLeaf in leavesToRemove) {
				IEnumerable<MCTSTreeNode> relativeLeaves = TraverseNodeForNonTerminalLeavesRecursively(oldLeaf);
				newLeaves.AddRange(relativeLeaves);
			}

			if (newLeaves.Count > 0 || leavesToRemove.Any()) {
				nodes = nodes.Except(leavesToRemove).Union(newLeaves).ToList();
			}
		}
	}
}
