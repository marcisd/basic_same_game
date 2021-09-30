using UnityEngine;

namespace MSD.BasicSameGame.View
{
	using GameLogic;

	public abstract class ScoreCalculator : ScriptableObject, IScoreCalculator
	{
		public abstract int CalculateScore(int matchCount);
	}
}
