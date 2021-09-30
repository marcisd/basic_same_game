using UnityEngine;

namespace MSD.BasicSameGame.View
{
	[CreateAssetMenu(menuName = "Score Calculator/Multiplied Score Calculator")]
	public class MultipliedScoreCalculator : ScoreCalculator
	{
		[SerializeField]
		private int _multiplier;

		public override int CalculateScore(int matchCount)
		{
			return matchCount * _multiplier;
		}
	}
}
