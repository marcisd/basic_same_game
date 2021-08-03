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

		private void Awake()
		{
			_sameGame = new SameGame(_gridSize, _tilePrefabs.Count, _minimumMatchCount);
		}

		private void Start()
		{
			_sameGame.Initialize();
		}
	}
}
