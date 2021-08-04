using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MSD.BasicSameGame.View
{
	public class Tile : MonoBehaviour, IPointerDownHandler
	{
		public Vector2Int TilePosition { get; set; }

		public Action<Vector2Int> OnTileSelected;

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			OnTileSelected?.Invoke(TilePosition);
		}
	}
}
