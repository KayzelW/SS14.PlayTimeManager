using Npgsql;
using PlayTimeManager.Models.Database;

namespace PlayTimeManager;

public class AppDbContext
{
    private readonly IConfiguration _configuration;
    private NpgsqlDataSource DataSource;

    public AppDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
        var connectionString = configuration.GetConnectionString("postgres")!;
        DataSource = NpgsqlDataSource.Create(connectionString);
    }
    
    public async IAsyncEnumerable<PlayTime> GetPlayTimeAsync(Guid playerId)
    {
        await using var cmd =
            DataSource.CreateCommand(
                @"SELECT 'player_id', 'tracker', 'time_spent' FROM playtime WHERE player_id = @player_id");
        cmd.Parameters.Add(new NpgsqlParameter("@player_id", playerId));
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            yield return new PlayTime()
            {
                PlayerId = reader.GetGuid(0),
                Tracker = reader.GetString(1),
                TimeSpent = reader.GetTimeSpan(2)
            };
        }
    }

    public async Task SavePlayTimeAsync(IEnumerable<PlayTime> playTimes)
    {
        await using var cmd = DataSource.CreateCommand(
            @"INSERT INTO playtime('player_id', 'tracker', 'time_spent') VALUES (@player_id, @tracker, @time_spent)
                ON CONFLICT('player_id', 'tracker') DO UPDATE SET 'time_spent' = @time_spent");

        await cmd.PrepareAsync();
        var playerIdParam = new NpgsqlParameter() { ParameterName = "@player_id" };
        cmd.Parameters.Add(playerIdParam);

        var trackerParam = new NpgsqlParameter() { ParameterName = "@tracker" };
        cmd.Parameters.Add(trackerParam);

        var timeSpentParam = new NpgsqlParameter() { ParameterName = "@time_spent" };
        cmd.Parameters.Add(timeSpentParam);

        foreach (var playTime in playTimes)
        {
            playerIdParam.Value = playTime.PlayerId;
            trackerParam.Value = playTime.Tracker;
            timeSpentParam.Value = playTime.TimeSpent;
            await cmd.ExecuteNonQueryAsync();
        }
    }
}