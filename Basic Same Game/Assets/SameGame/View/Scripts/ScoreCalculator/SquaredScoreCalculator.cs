using UnityEngine;

namespace MSD.BasicSameGame.View
{
	[CreateAssetMenu(menuName = "Score Calculator/Squared Score Calculator")]
	public class SquaredScoreCalculator : ScoreCalculator
	{
		public override int CalculateScore(int matchCount)
		{
			return matchCount * matchCount;
		}
	}
}
