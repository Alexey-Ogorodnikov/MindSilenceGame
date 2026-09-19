using System.Collections.Generic;

namespace MindSilence.Domain
{
    public interface IGameProgressRepository
    {
        int RecordSession(int levelReached, int totalSeconds);

        IReadOnlyList<DailyStats> GetDailyStats();
    }
}
