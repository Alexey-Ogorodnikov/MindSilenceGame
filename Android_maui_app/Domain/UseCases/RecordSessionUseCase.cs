using MindSilence.Domain.Repository;

namespace MindSilence.Domain.UseCases;

public sealed class RecordSessionUseCase(IGameProgressRepository progress)
{
	public int Invoke(int levelReached, int totalSeconds) =>
		progress.RecordSession(levelReached, totalSeconds);
}
