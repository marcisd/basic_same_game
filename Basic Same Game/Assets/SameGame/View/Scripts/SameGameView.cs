using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MSD.BasicSameGame.View
{
	using GameLogic;
	using Modules.ObjectPool;

	public class SameGameView : MonoBehaviour
	{
		[SerializeField]
		private Vector2Int _gridSize;

		[SerializeField]
		private int _minimumMatchCount = 3;

		[SerializeField]
		private bool _isTouchDisabled;

		[SerializeField]
		private List<TileController> _tilePrefabs;

		[Space]
		[SerializeField]
		private ObjectPool _recycler;

		[Space]
		[SerializeField]
		private UnityEvent _onGameStart;

		[Space]
		[SerializeField]
		private UnityEvent _onGameOver;

		private SameGame _sameGame;

		private Dictionary<Vector2Int, TileController> _activeTiles = new Dictionary<Vector2Int, TileController>();

		public Vector2Int GridSize => _gridSize;

		public void GameStart()
		{
			_sameGame.Initialize();
			_onGameStart.Invoke();
		}

		public void GameReset()
		{
			GameReset(null);
		}

		public void GameRestart()
		{
			GameReset(GameStart);
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
			GameStart();
		}

		private void OnValidate()
		{
			_gridSize = new Vector2Int(Mathf.Clamp(_gridSize.x, 1, 20), Mathf.Clamp(_gridSize.y, 1, 20));
		}

		private void GameReset(Action onComplete)
		{
			void FinishedDespawning()
			{
				_activeTiles.Clear();
				_sameGame.Reset();
				onComplete?.Invoke();
			}

			KeyValuePair<Vector2Int, TileController> last = _activeTiles.Last();
			foreach (KeyValuePair<Vector2Int, TileController> tile in _activeTiles) {
				if (last.Equals(tile)) {
					DespawnTile(tile.Value, FinishedDespawning);
				} else {
					DespawnTile(tile.Value, null);
				}
			}
		}

		private void OnTileCreated(Vector2Int cellPosition, int tileIndex)
		{
			TileController tile = SpawnTile(tileIndex);
			tile.SpawnAtPosition(cellPosition);
			_activeTiles.Add(cellPosition, tile);
		}

		private void OnTileDestroyed(Vector2Int cellPosition)
		{
			TileController tile = _activeTiles[cellPosition];
			DespawnTile(tile, null);
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
			if (_isTouchDisabled) { return; }

			int tilesDestroyed = _sameGame.DestroyMatchingTilesFromCell(cellPosition);
			if (tilesDestroyed > 0 && !_sameGame.HasValidMoves()) {
				_onGameOver.Invoke();
			}
		}

		private TileController SpawnTile(int tileIndex)
		{
			TileController tile;
			if (_recycler != null) {
				tile = _recycler.Spawn(_tilePrefabs[tileIndex - 1], transform);
			} else {
				tile = Instantiate(_tilePrefabs[tileIndex - 1]);
				tile.transform.SetParent(transform, false);
				return tile;
			}
			tile.OnTileSelected += OnTileSelected;
			return tile;
		}

		private void DespawnTile(TileController tile, Action onDespawn)
		{
			tile.OnTileSelected -= OnTileSelected;
			if (_recycler != null) {
				_recycler.Despawn(tile, onDespawn);
			} else {
				Destroy(tile.gameObject);
			}
		}
	}
}
