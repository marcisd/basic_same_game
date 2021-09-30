using System;

namespace MSD.BasicSameGame.AI
{
	internal struct PlayoutResult : IEquatable<PlayoutResult>
	{
		public int TotalMoves { get; }
		public int TotalScore { get; }
		public int RemainingTilesCount { get; }

		public PlayoutResult(int totalMoves, int totalScore, int remainingTilesCount)
		{
			TotalMoves = totalMoves;
			TotalScore = totalScore;
			RemainingTilesCount = remainingTilesCount;
		}

		public bool Equals(PlayoutResult other)
		{
			return TotalMoves == other.TotalMoves && RemainingTilesCount == other.RemainingTilesCount;
		}
	}

}
