using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SolidCqrsFramework.EventManagement;

namespace SolidCqrsFramework.Replaying;

public class Replayer
{
    private readonly IEventsStore _eventStore;
    private readonly IReplayRepository _replayRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;

    public Replayer(IEventsStore eventStore,
        IReplayRepository replayRepository,
        IEventBus eventBus,
        ILogger logger)
    {
        _eventStore = eventStore;
        _replayRepository = replayRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task ReplayAsync(string replayId)
    {
        var recordType = $"Replay-{DateTime.UtcNow:yyyyMMddTHHmmss}";

        var replayRun = new ReplayRunData
        {
            ReplayId = replayId,
            StartTime = DateTime.UtcNow,
            Status = ReplayStatus.InProgress,
            EventCount = 0,
            RecordType = recordType,
            UpdatedOn = DateTime.UtcNow
        };

        await _replayRepository.CreateReplayRunAsync(replayRun);

        var lastReplayRun = await _replayRepository.GetLastReplayRunAsync();
        _logger.LogInformation($"Last replay run status: {lastReplayRun?.Status ?? "None"}");

        var lastFailedEventTimestamp = lastReplayRun?.FailedEventTimestamp;
        DateTime? lastProcessedTimestamp = null;

        try
        {
            var eventsStream = _eventStore.LoadAllEventsAsync(replayFrom: lastFailedEventTimestamp);
            await foreach (var stream in eventsStream)
            {
                // Validate event order
                if (lastProcessedTimestamp != null && stream.TimeStamp < lastProcessedTimestamp)
                {
                    var message =
                        $"Event {stream.AggregateId} is out of order. Current timestamp: {stream.TimeStamp}, Last timestamp: {lastProcessedTimestamp}";
                    _logger.LogError(message);

                    replayRun.Status = ReplayStatus.Failed;
                    replayRun.ErrorMessage = message;
                    replayRun.FailedPayloadKeyAndType = $"{stream.AggregateId}#{stream.GetType()}";
                    replayRun.FailedEventTimestamp = stream.TimeStamp;
                    replayRun.UpdatedOn = DateTime.UtcNow;

                    await _replayRepository.UpdateReplayRunAsync(replayRun);
                    throw new InvalidOperationException(message);
                }

                lastProcessedTimestamp = stream.TimeStamp;

                try
                {
                    _logger.LogInformation($"Processing event {stream.AggregateId} of type {stream.GetType().Name}");
                    await _eventBus.Publish(new[] { stream });

                    replayRun.EventCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to process event {stream.AggregateId}: {ex.Message}");

                    replayRun.Status = ReplayStatus.Failed;
                    replayRun.ErrorMessage = $"Failed at event {stream.AggregateId}: {ex.Message}";
                    replayRun.FailedPayloadKeyAndType = $"{stream.AggregateId}#{stream.GetType()}";
                    replayRun.FailedEventTimestamp = stream.TimeStamp;
                    replayRun.UpdatedOn = DateTime.UtcNow;

                    await _replayRepository.UpdateReplayRunAsync(replayRun);
                    throw;
                }
            }

            replayRun.Status = ReplayStatus.Success;
            replayRun.EndTime = DateTime.UtcNow;
            replayRun.UpdatedOn = DateTime.UtcNow;

            await _replayRepository.UpdateReplayRunAsync(replayRun);
            _logger.LogInformation($"Replay process completed successfully with {replayRun.EventCount} events.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Replay process stopped due to an error.");
            replayRun.Status = ReplayStatus.Failed;
            replayRun.ErrorMessage = ex.Message;
            replayRun.EndTime = DateTime.UtcNow;
            replayRun.UpdatedOn = DateTime.UtcNow;

            await _replayRepository.UpdateReplayRunAsync(replayRun);
            throw;
        }
    }
}
