using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSD.BasicSameGame.AI
{
	using GameLogic;

    public class TreeNode
    {
        private readonly SameGame _sameGame;

        private readonly TreeNode _parent;

        private readonly Vector2Int? _selectedCell;

        private readonly bool _isTerminalNode;

        private List<TreeNode> _children;

        public bool IsRootNode => _parent == null;

        public bool IsLeafNode => _children == null || _children.Count == 0;

        public bool IsTerminalNode => _isTerminalNode;

        public TreeNode Parent => _parent;

        public IReadOnlyList<TreeNode> Children => _children;

        public TreeNode(SameGame sameGame)
        {
            _sameGame = sameGame;
            _parent = null;
            _selectedCell = null;

            _isTerminalNode = !sameGame.HasValidMoves();
        }

        public TreeNode(SameGame sameGame, TreeNode parent, Vector2Int selectedCell)
        {
            _sameGame = sameGame;
            _parent = parent;
            _selectedCell = selectedCell;

            _isTerminalNode = !sameGame.HasValidMoves();
        }

        public void Expand()
        {
            if (IsTerminalNode && !IsLeafNode) throw new InvalidOperationException("Only non-terminal leaves can be expanded!");

			Vector2Int[][] groups = _sameGame.GetMatchingCells();
            _children = new List<TreeNode>();
            foreach (Vector2Int[] group in groups) {
                SameGame copy = new SameGame(_sameGame);
                var selectedCell = group[0];
                copy.DestroyMatchingTilesFromCell(selectedCell);

                TreeNode child = new TreeNode(copy, this, selectedCell);
                _children.Add(child);
            }
        }

        public IEnumerable<TreeNode> GetLeaves()
        {
            return TraverseNodeForLeavesRecursively(this);
        }

        private IEnumerable<TreeNode> TraverseNodeForLeavesRecursively(TreeNode node)
        {
            if (node.IsTerminalNode) {
                yield break;
            }

            if (node.IsLeafNode) {
                yield return node;
            } else {
                foreach (TreeNode child in node._children) {
                    foreach (var leaf in TraverseNodeForLeavesRecursively(child)) {
                        yield return leaf;
                    }
                }
            }
        }
    }
}
