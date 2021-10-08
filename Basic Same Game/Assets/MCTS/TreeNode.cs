using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MCTS
{
	public class TreeNode
	{
		private readonly List<TreeNode> _children = new List<TreeNode>();

		public ReadOnlyCollection<TreeNode> Children => _children.AsReadOnly();

		public TreeNode Parent { get; }

		public int Level { get; }

		public int Degree => _children.Count;

		public bool IsRootNode => Parent == null;

		public bool IsLeafNode => _children == null || _children.Count == 0;

		public TreeNode() : this(null) { }

		public TreeNode(TreeNode parent)
		{
			if (parent != null) {
				Parent = parent;
				parent._children.Add(this);
				Level = parent.Level + 1;
			}
		}
	}
}
