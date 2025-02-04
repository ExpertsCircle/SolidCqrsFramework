using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolidCqrsFramework.Replaying
{
    public interface IReplayRepository
    {
        Task CreateReplayRunAsync(ReplayRunData replayRunData);
        Task<ReplayRunData?> GetLastReplayRunAsync();
        Task<List<ReplayRunData>> GetReplayRunsAsync();
        Task<List<ReplayRunData>> GetFailedReplayRunsAsync();
        Task UpdateReplayRunAsync(ReplayRunData replayRunData);
    }
}
