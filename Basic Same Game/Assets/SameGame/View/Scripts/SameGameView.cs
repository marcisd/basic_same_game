using System.Collections.Generic;
using UnityEngine;

namespace MSD.BasicSameGame.View
{
	using GameLogic;

	public class SameGameView : MonoBehaviour
	{
		[SerializeField]
		private Vector2Int _gridSize;

		[SerializeField]
		private int _minimumMatchCount = 3;

		[SerializeField]
		private List<TileController> _tilePrefabs;

		private SameGame _sameGame;

		private Dictionary<Vector2Int, TileController> _activeTiles = new Dictionary<Vector2Int, TileController>();

		public Vector2Int GridSize => _gridSize;

		public void Restart()
		{
			foreach (KeyValuePair<Vector2Int, TileController> tile in _activeTiles) {
				GameObject.Destroy(tile.Value.gameObject);
			}
			_activeTiles.Clear();
			_sameGame.Reset();
			_sameGame.Initialize();
		}

		private void Awake()
		{
			_sameGame = new SameGame(_gridSize, _tilePrefabs.Count, _minimumMatchCount);
			_sameGame.OnTileCreated += OnTileCreated;
			_sameGame.OnTileDestroyed += OnTileDestroyed;
			_sameGame.OnTileMoved += OnTileMoved;
		}

		private void Start()
		{
			_sameGame.Initialize();
		}

		private void OnTileCreated(Vector2Int cellPosition, int tileIndex)
		{
			// TODO: implement recycler
			TileController tile = GameObject.Instantiate(_tilePrefabs[tileIndex - 1]);
			tile.transform.SetParent(transform, true);
			tile.SpawnAtPosition(cellPosition);
			tile.OnTileSelected += OnTileSelected;

			_activeTiles.Add(cellPosition, tile);
		}

		private void OnTileDestroyed(Vector2Int cellPosition)
		{
			// TODOL implement recycler
			TileController tile = _activeTiles[cellPosition];
			tile.OnTileSelected -= OnTileSelected;
			GameObject.Destroy(tile.gameObject);

			_activeTiles.Remove(cellPosition);
		}

		private void OnTileMoved(Vector2Int originalCellPosition, Vector2Int newCellPosition)
		{
			TileController tile = _activeTiles[originalCellPosition];
			tile.MoveToPosition(newCellPosition);

			_activeTiles.Remove(originalCellPosition);
			_activeTiles.Add(newCellPosition, tile);
		}

		private void OnTileSelected(Vector2Int cellPosition)
		{
			int tilesDestroyed = _sameGame.DestroyMatchingTilesFromCell(cellPosition);
			if (tilesDestroyed > 0 && !_sameGame.HasValidMoves()) {
				Debug.LogError("Game end!");
			}
		}
	}
}
