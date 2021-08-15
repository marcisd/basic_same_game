using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MSD.BasicSameGame.View
{
	using Modules.ObjectPool;

	public class TileController : MonoBehaviour, IPointerDownHandler, IPoolable
	{
		private Vector2Int _tilePosition;

		private bool _isInteractable = true;

		public event Action<Vector2Int> OnTileSelected;

		public void SpawnAtPosition(Vector2Int cellPosition)
		{
			Vector3 position = new Vector3(cellPosition.x, cellPosition.y, 0);
			transform.position = position;
			_tilePosition = cellPosition;
		}

		public void MoveToPosition(Vector2Int cellPosition)
		{
			_isInteractable = false;
			if (_tilePosition != cellPosition) {
				Vector3 position = new Vector3(cellPosition.x, cellPosition.y, 0);
				LeanTween.move(gameObject, position, 0.5f).setEaseInQuart().setOnComplete(() => {
					_isInteractable = true;
				});
				_tilePosition = cellPosition;
			}
		}

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			if (_isInteractable) {
				OnTileSelected?.Invoke(_tilePosition);
			}
		}

		void IPoolable.OnAfterSpawn()
		{
			_isInteractable = false;
			gameObject.SetActive(true);
			transform.localScale = Vector3.zero;
			LeanTween.scale(gameObject, Vector3.one, 0.25f).setOnComplete(() => {
				_isInteractable = true;
			});
		}

		void IPoolable.OnBeforeDespawn(Action onBeforeDespawnComplete)
		{
			_isInteractable = false;
			LeanTween.scale(gameObject, Vector3.zero, 0.1f).setOnComplete(() => {
				gameObject.SetActive(false);
				onBeforeDespawnComplete.Invoke();
			});
		}
	}
}
