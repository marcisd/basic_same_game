using System;
using System.Collections.Generic;
using System.Linq;

namespace MSD.BasicSameGame.AI
{
	internal class RandomSelectionPolicy : MCTS.ISelectionPolicy
	{
		public TreeNode Selection(IEnumerable<TreeNode> nonTerminalLeaves)
		{
			TreeNode[] leavesArr = nonTerminalLeaves.ToArray();
			Random random = new Random();
			int randomInt = random.Next(leavesArr.Length);
			return leavesArr[randomInt];
		}

		MCTS.MCTSTreeNode MCTS.ISelectionPolicy.Selection(IEnumerable<MCTS.MCTSTreeNode> nonTerminalLeaves)
		{
			return Selection(nonTerminalLeaves.Select(leaf => leaf as TreeNode));
		}
	}
}