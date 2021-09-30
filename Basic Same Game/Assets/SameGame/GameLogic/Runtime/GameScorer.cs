
namespace MSD.BasicSameGame.GameLogic
{
	public class GameScorer
	{
		private readonly IScoreCalculator _scoreCalculator;

		public int TotalScore { get; private set; }

		public int TotalMoves { get; private set; }

		public GameScorer(IScoreCalculator scoreCalculator)
		{
			_scoreCalculator = scoreCalculator;
		}

		public GameScorer(GameScorer scorer)
		{
			_scoreCalculator = scorer._scoreCalculator;
			TotalScore = scorer.TotalScore;
			TotalMoves = scorer.TotalMoves;
		}

		public void RegisterMove(int matchCount)
		{
			TotalScore += _scoreCalculator.CalculateScore(matchCount);
			TotalMoves++;
		}

		public void Reset()
		{
			TotalScore = 0;
			TotalMoves = 0;
		}
	}
}
