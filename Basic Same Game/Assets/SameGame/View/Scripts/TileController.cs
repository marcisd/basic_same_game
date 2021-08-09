using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MSD.BasicSameGame.View
{
	public class TileController : MonoBehaviour, IPointerDownHandler
	{
		private Vector2Int _tilePosition;

		private bool _interactable = true;

		public event Action<Vector2Int> OnTileSelected;

		public void SpawnAtPosition(Vector2Int cellPosition)
		{
			Vector3 position = new Vector3(cellPosition.x, cellPosition.y, 0);
			transform.position = position;
			_tilePosition = cellPosition;
		}

		public void MoveToPosition(Vector2Int cellPosition)
		{
			if (_tilePosition != cellPosition) {
				Vector3 position = new Vector3(cellPosition.x, cellPosition.y, 0);
				LeanTween.move(gameObject, position, 0.5f).setEaseInQuart();
				_tilePosition = cellPosition;
			}
		}

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			if (_interactable) {
				OnTileSelected?.Invoke(_tilePosition);
			}
		}
	}
}
