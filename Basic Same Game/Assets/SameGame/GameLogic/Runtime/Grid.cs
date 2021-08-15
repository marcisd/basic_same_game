using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSD.BasicSameGame.GameLogic
{
	/// <summary>
	/// A 2D grid with origin <c>(0, 0)</c> at bottom left.
	/// </summary>
	internal struct Grid
	{
		public Vector2Int Size { get; }

		public Grid(Vector2Int size)
		{
			Size = size;
		}

		public Grid(int horizontal, int vertical)
		{
			Size = new Vector2Int(horizontal, vertical);
		}

		public bool IsValidCell(Vector2Int cellPosition)
		{
			return cellPosition.x >= 0 && cellPosition.x < Size.x && cellPosition.y >= 0 && cellPosition.y < Size.y;
		}

		public void ForEachCell(Action<Vector2Int> action)
		{
			for (int x = 0; x < Size.x; x++) {
				for (int y = 0; y < Size.y; y++) {
					action.Invoke(new Vector2Int(x, y));
				}
			}
		}

		public void ForEachAdjoinedCell(Vector2Int cellPosition, Action<Vector2Int> action)
		{
			if (!IsValidCell(cellPosition)) { return; }

			// Top
			if (cellPosition.y < Size.y - 1) {
				action.Invoke(new Vector2Int(cellPosition.x, cellPosition.y + 1));
			}

			// Right
			if (cellPosition.x < Size.x - 1) {
				action.Invoke(new Vector2Int(cellPosition.x + 1, cellPosition.y));
			}

			// Bottom
			if (cellPosition.y > 0) {
				action.Invoke(new Vector2Int(cellPosition.x, cellPosition.y - 1));
			}

			// Left
			if (cellPosition.x > 0) {
				action.Invoke(new Vector2Int(cellPosition.x - 1, cellPosition.y));
			}
		}

		public IEnumerator<Vector2Int> GetAdjoinedCellsEnumerator(Vector2Int cellPosition)
		{
			if (!IsValidCell(cellPosition)) { yield break; }

			// Top
			if (cellPosition.y < Size.y - 1) {
				yield return new Vector2Int(cellPosition.x, cellPosition.y + 1);
			}

			// Right
			if (cellPosition.x < Size.x - 1) {
				yield return new Vector2Int(cellPosition.x + 1, cellPosition.y);
			}

			// Bottom
			if (cellPosition.y > 0) {
				yield return new Vector2Int(cellPosition.x, cellPosition.y - 1);
			}

			// Left
			if (cellPosition.x > 0) {
				yield return new Vector2Int(cellPosition.x - 1, cellPosition.y);
			}
		}
	}
}
