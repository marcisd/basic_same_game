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
		private List<Tile> _tilePrefabs;

		private SameGame _sameGame;

		private Dictionary<Vector2Int, Tile> _activeTiles = new Dictionary<Vector2Int, Tile>();

		public Vector2Int GridSize => _gridSize;

		public void Restart()
		{
			foreach (KeyValuePair<Vector2Int, Tile> tile in _activeTiles) {
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
			Tile tile = GameObject.Instantiate(_tilePrefabs[tileIndex - 1]);
			Vector3 position = new Vector3(cellPosition.x, cellPosition.y, 0);
			tile.transform.position = position;
			tile.transform.SetParent(transform, true);
			_activeTiles.Add(cellPosition, tile);

			tile.TilePosition = cellPosition;
			tile.OnTileSelected = OnTileSelected;
		}

		private void OnTileDestroyed(Vector2Int cellPosition)
		{
			// TODOL implement recycler
			Tile tile = _activeTiles[cellPosition];
			GameObject.Destroy(tile.gameObject);

			_activeTiles.Remove(cellPosition);
		}

		private void OnTileMoved(Vector2Int originalCellPosition, Vector2Int newCellPosition)
		{
			Tile tile = _activeTiles[originalCellPosition];
			Vector3 position = new Vector3(newCellPosition.x, newCellPosition.y, 0);
			tile.transform.position = position;
			tile.TilePosition = newCellPosition;

			_activeTiles.Remove(originalCellPosition);
			_activeTiles.Add(newCellPosition, tile);
		}

		private void OnTileSelected(Vector2Int cellPosition)
		{
			int tilesDestroyed = _sameGame.DestroyMatchingTilesFromCell(cellPosition);
			Debug.Log(tilesDestroyed);
		}
	}
}
