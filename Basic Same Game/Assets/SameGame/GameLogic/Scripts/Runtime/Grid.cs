using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSD.BasicSameGame.GameLogic
{
	/// <summary>
	/// A 2D grid with origin <c>(0, 0)</c> at bottom left.
	/// </summary>
	public class Grid
	{
		private readonly Vector2Int _size;

		public Grid(Vector2Int size)
		{
			_size = size;
		}

		public Grid(int horizontal, int vertical)
		{
			_size = new Vector2Int(horizontal, vertical);
		}

		public Vector2Int Size => _size;

		public bool IsValidCell(Vector2Int cellPosition)
		{
			return cellPosition.x >= 0 && cellPosition.x < _size.x && cellPosition.y >= 0 && cellPosition.y < _size.y;
		}

		public void ForEachCell(Action<Vector2Int> action)
		{
			for (int x = 0; x < _size.x; x++) {
				for (int y = 0; y < _size.y; y++) {
					action.Invoke(new Vector2Int(x, y));
				}
			}
		}

		public void ForEachAdjoinedCell(Vector2Int cellPosition, Action<Vector2Int> action)
		{
			if (!IsValidCell(cellPosition)) { return; }

			// Top
			if (cellPosition.y < _size.y) {
				action.Invoke(new Vector2Int(cellPosition.x, cellPosition.y + 1));
			}

			// Right
			if (cellPosition.x < _size.x) {
				action.Invoke(new Vector2Int(cellPosition.x + 1, cellPosition.y));
			}

			// Bottom
			if (cellPosition.y >= 0) {
				action.Invoke(new Vector2Int(cellPosition.x, cellPosition.y - 1));
			}

			// Left
			if (cellPosition.x >= 0) {
				action.Invoke(new Vector2Int(cellPosition.x - 1, cellPosition.y));
			}
		}

		public IEnumerator<Vector2Int> AdjoinedCells(Vector2Int cellPosition)
		{
			if (!IsValidCell(cellPosition)) { yield break; }

			// Top
			if (cellPosition.y < _size.y) {
				yield return new Vector2Int(cellPosition.x, cellPosition.y + 1);
			}

			// Right
			if (cellPosition.x < _size.x) {
				yield return new Vector2Int(cellPosition.x + 1, cellPosition.y);
			}

			// Bottom
			if (cellPosition.y >= 0) {
				yield return new Vector2Int(cellPosition.x, cellPosition.y - 1);
			}

			// Left
			if (cellPosition.x >= 0) {
				yield return new Vector2Int(cellPosition.x - 1, cellPosition.y);
			}
		}
	}
}
