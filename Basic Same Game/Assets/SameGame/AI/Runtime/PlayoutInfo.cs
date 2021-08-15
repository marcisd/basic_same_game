using System;

namespace MSD.BasicSameGame.AI
{
	internal struct PlayoutInfo : IEquatable<PlayoutInfo>
	{
		public int TotalMoves { get; }
		public int RemainingTilesCount { get; }

		public int Score => TotalMoves * (RemainingTilesCount + 1);

		public PlayoutInfo(int totalMoves, int remainingTilesCount)
		{
			TotalMoves = totalMoves;
			RemainingTilesCount = remainingTilesCount;
		}

		public bool Equals(PlayoutInfo other)
		{
			return TotalMoves == other.TotalMoves && RemainingTilesCount == other.RemainingTilesCount;
		}
	}

}
