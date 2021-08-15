
namespace MSD.BasicSameGame.AI
{
	internal class PlayoutInfo
	{
		public int TotalMoves { get; }
		public int RemainingTilesCount { get; }

		public int Score => TotalMoves * (RemainingTilesCount + 1);

		public PlayoutInfo(int totalMoves, int remainingTilesCount)
		{
			TotalMoves = totalMoves;
			RemainingTilesCount = remainingTilesCount;
		}
	}

}
