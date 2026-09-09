using MindSilence.Domain.Models;

namespace MindSilence.Domain.Repository;

public interface IGameProgressRepository
{
	int RecordSession(int levelReached, int totalSeconds);
	IReadOnlyList<DailyStats> GetDailyStats();
}
