using MindSilence.Domain.Models;
using MindSilence.Domain.Repository;

namespace MindSilence.Domain.UseCases;

public sealed class GetDailyStatsUseCase(IGameProgressRepository progress)
{
	public IReadOnlyList<DailyStats> Invoke() => progress.GetDailyStats();
}
