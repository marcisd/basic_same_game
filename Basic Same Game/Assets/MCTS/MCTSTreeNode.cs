using System.Collections.Generic;
using System.Linq;

namespace MSD.MCTS
{
	public abstract class MCTSTreeNode : TreeNode
	{
		private List<MCTSTreeNode> _nonTerminalLeavesCache;

		public bool IsTerminalNode { get; protected set; }

		protected MCTSTreeNode(MCTSTreeNode parent) : base(parent) { }

		public IEnumerable<MCTSTreeNode> GetNonTerminalLeaves()
		{
			if (_nonTerminalLeavesCache == null) {
				_nonTerminalLeavesCache = TraverseNodeForNonTerminalLeaves(this).ToList();
			} else {
				UpdateNonTerminalLeavesCache(ref _nonTerminalLeavesCache);
			}
			return _nonTerminalLeavesCache;
		}

		public void SetTerminal()
		{
			IsTerminalNode = true;
		}

		public abstract void Simulate();

		public abstract void Backpropagate();

		private static IEnumerable<MCTSTreeNode> TraverseNodeForNonTerminalLeaves(MCTSTreeNode node)
		{
			if (node.IsTerminalNode) {
				yield break;
			}

			Queue<MCTSTreeNode> queue = new Queue<MCTSTreeNode>();
			queue.Enqueue(node);

			while (queue.Count > 0) {

				MCTSTreeNode n = queue.Dequeue();

				if (n.IsLeafNode) {
					yield return n;
				} else {
					foreach (MCTSTreeNode child in n.Children) {
						queue.Enqueue(child);
					}
				}
			}
		}

		private static void UpdateNonTerminalLeavesCache(ref List<MCTSTreeNode> nodes)
		{
			List<MCTSTreeNode> newLeaves = new List<MCTSTreeNode>();
			IEnumerable<MCTSTreeNode> leavesToRemove = nodes.Where(n => !n.IsLeafNode);
			foreach (MCTSTreeNode oldLeaf in leavesToRemove) {
				IEnumerable<MCTSTreeNode> relativeLeaves = TraverseNodeForNonTerminalLeaves(oldLeaf);
				newLeaves.AddRange(relativeLeaves);
			}

			if (newLeaves.Count > 0 || leavesToRemove.Any()) {
				nodes = nodes.Except(leavesToRemove).Union(newLeaves).ToList();
			}
		}
	}
}
