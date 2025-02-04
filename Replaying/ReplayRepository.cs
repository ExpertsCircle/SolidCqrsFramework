using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SolidCqrsFramework.Replaying;

public class ReplayRepository : IReplayRepository
{
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly ILogger<ReplayRepository> _logger;
    private readonly string _tableName;

    public ReplayRepository(IDynamoDBContext dynamoDbContext, IConfiguration configuration, ILogger<ReplayRepository> logger)
    {
        _dynamoDbContext = dynamoDbContext;
        _logger = logger;
        _tableName = configuration["replayRunTableName"] ?? throw new InvalidOperationException("DynamoDB table name is missing in configuration.");
    }

    public async Task CreateReplayRunAsync(ReplayRunData replayRunData)
    {
        try
        {
            var config = new DynamoDBOperationConfig { OverrideTableName = _tableName };
            await _dynamoDbContext.SaveAsync(replayRunData, config);
            _logger.LogInformation($"Replay run {replayRunData.ReplayId} created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create replay run.");
            throw;
        }
    }

    public async Task<ReplayRunData?> GetLastReplayRunAsync()
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new(nameof(ReplayRunData.RecordType), ScanOperator.BeginsWith, "Replay-")
            };

            var config = new DynamoDBOperationConfig { OverrideTableName = _tableName };
            var replayRuns = await _dynamoDbContext.ScanAsync<ReplayRunData>(conditions, config).GetRemainingAsync();

            var lastReplay = replayRuns.Where(x => !x.Status.Equals(ReplayStatus.InProgress)).OrderByDescending(run => run.StartTime).FirstOrDefault();

            if (lastReplay == null)
            {
                _logger.LogWarning("No previous replay runs found.");
            }
            return lastReplay;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving last replay run.");
            throw;
        }
    }

    public async Task<List<ReplayRunData>> GetReplayRunsAsync()
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new(nameof(ReplayRunData.RecordType), ScanOperator.BeginsWith, "Replay-")
            };

            var config = new DynamoDBOperationConfig { OverrideTableName = _tableName };
            return await _dynamoDbContext.ScanAsync<ReplayRunData>(conditions, config).GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving replay runs.");
            throw;
        }
    }

    public async Task<List<ReplayRunData>> GetFailedReplayRunsAsync()
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new(nameof(ReplayRunData.Status), ScanOperator.Equal, ReplayStatus.Failed)
            };

            var config = new DynamoDBOperationConfig { OverrideTableName = _tableName };
            return await _dynamoDbContext.ScanAsync<ReplayRunData>(conditions, config).GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving failed replay runs.");
            throw;
        }
    }

    public async Task UpdateReplayRunAsync(ReplayRunData replayRunData)
    {
        try
        {
            var config = new DynamoDBOperationConfig { OverrideTableName = _tableName };

            var existingRecord = await _dynamoDbContext.LoadAsync<ReplayRunData>(replayRunData.ReplayId, replayRunData.RecordType, config);
            if (existingRecord == null)
            {
                _logger.LogWarning($"Replay run {replayRunData.ReplayId} does not exist. Creating a new entry.");
            }

            await _dynamoDbContext.SaveAsync(replayRunData, config);
            _logger.LogInformation($"Replay run {replayRunData.ReplayId} updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to update replay run {replayRunData.ReplayId}.");
            throw;
        }
    }
}
