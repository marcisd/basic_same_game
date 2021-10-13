using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	internal class RandomSelectionPolicy : MCTS.ISelectionPolicy
	{
		public TreeNode Selection(IEnumerable<TreeNode> nonTerminalLeaves)
		{
			TreeNode[] leavesArr = nonTerminalLeaves.ToArray();
			int rand = Random.Range(0, leavesArr.Length - 1);
			return leavesArr[rand];
		}

		MCTS.MCTSTreeNode MCTS.ISelectionPolicy.Selection(IEnumerable<MCTS.MCTSTreeNode> nonTerminalLeaves)
		{
			return Selection(nonTerminalLeaves.Select(leaf => leaf as TreeNode));
		}
	}
}