using UnityEngine;

namespace MSD.BasicSameGame.View
{
	[CreateAssetMenu(menuName = "Score Calculator/Simple Count Score Calculator")]
	public class SimpleCountScoreCalculator : ScoreCalculator
	{
		public override int CalculateScore(int matchCount)
		{
			return matchCount;
		}
	}
}
