using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MSD.BasicSameGame.View
{
	using GameLogic;
	using Modules.ObjectPool;
	using AI;

	public class SameGameView : MonoBehaviour
	{
		private static readonly int MIN_TILES_COUNT = 3;
		private static readonly int MIN_GRID_SIZE = 5;
		private static readonly int MAX_GRID_SIZE = 20;

		[SerializeField] private Vector2Int _gridSize;
		[SerializeField] private int _tileCount = 3;
		[SerializeField] private int _minimumMatchCount = 3;

		[SerializeField] private ScoreCalculator _scoreCalculator;

		[SerializeField] private List<TileController> _tilePrefabs;

		[Space]
		[SerializeField] private ObjectPool _recycler;

		[Space]
		[SerializeField] private UnityEvent _onGameStart;
		[SerializeField] private UnityEvent _onGameOver;
		[SerializeField] private UnityEvent _onAutoCompleteStart;
		[SerializeField] private UnityEvent<string> _onScoreLabelUpdate;

		[Header("AI")]
		[SerializeField] private int _searchIterations = 500;

		[Header("Runtime")]
		[SerializeField] private bool _isTouchDisabled;

		private SameGame _sameGame;
		private GameScorer _scorer;
		private Dictionary<Vector2Int, TileController> _activeTiles = new Dictionary<Vector2Int, TileController>();

		public Vector2Int GridSize => _gridSize;

		public void GameStart()
		{
			_sameGame.Initialize();
			_onGameStart.Invoke();
			UpdateScore(0);
			_isTouchDisabled = false;
		}

		public void GameReset()
		{
			GameReset(null);
		}

		public void GameRestart()
		{
			GameReset(GameStart);
		}

		public void AutoComplete()
		{
			_onAutoCompleteStart.Invoke();
			_isTouchDisabled = true;
			BestMovesSearch mcts = new BestMovesSearch(_sameGame, _scorer);
			IEnumerable<Vector2Int> moves = mcts.PerformSearch(_searchIterations);
			StartCoroutine(Playout(moves));
		}

		private void Awake()
		{
			if (_tilePrefabs.Count >= MIN_TILES_COUNT) {
				_sameGame = new SameGame(_gridSize, _tileCount, _minimumMatchCount);
				_sameGame.OnTileCreated += OnTileCreated;
				_sameGame.OnTileDestroyed += OnTileDestroyed;
				_sameGame.OnTileMoved += OnTileMoved;

				_scorer = new GameScorer(_scoreCalculator);
			} else {
				Debug.LogError("Failed to instantiate game!");
			}
		}

		private void Start()
		{
			GameStart();
		}

		private void OnValidate()
		{
			_gridSize = new Vector2Int(Mathf.Clamp(_gridSize.x, MIN_GRID_SIZE, MAX_GRID_SIZE), Mathf.Clamp(_gridSize.y, MIN_GRID_SIZE, MAX_GRID_SIZE));
			_tileCount = Mathf.Clamp(_tileCount, MIN_TILES_COUNT, _tilePrefabs.Count);
		}

		private void GameReset(Action onComplete)
		{
			void FinishedDespawning()
			{
				_activeTiles.Clear();
				_sameGame.Reset();
				_scorer.Reset();
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

		private IEnumerator Playout(IEnumerable<Vector2Int> moves)
		{
			foreach (Vector2Int move in moves) {
				SelectTile(move);
				yield return new WaitForSeconds(1);
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
			_activeTiles.Remove(cellPosition);
			DespawnTile(tile, null);
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
			if (!_isTouchDisabled) {
				SelectTile(cellPosition);
			}
		}

		private void SelectTile(Vector2Int cellPosition)
		{
			int tilesDestroyed = _sameGame.DestroyMatchingTilesFromCell(cellPosition);
			if (tilesDestroyed > 0) {
				_scorer.RegisterMove(tilesDestroyed);
				UpdateScore(_scorer.TotalScore);

				if (!_sameGame.HasValidMoves()) {
					_onGameOver.Invoke();
				}
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

		private void UpdateScore(int score)
		{
			_onScoreLabelUpdate.Invoke($"Score: {score}");
		}
	}
}
