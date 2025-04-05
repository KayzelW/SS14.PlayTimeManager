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
                @"SELECT player_id, tracker, time_spent FROM play_time WHERE player_id = @player_id");
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
        await using var conn = DataSource.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"INSERT INTO play_time(player_id, tracker, time_spent) 
          VALUES (@player_id, @tracker, @time_spent)
          ON CONFLICT(player_id, tracker) 
          DO UPDATE SET time_spent = @time_spent",
            conn);

        var playerIdParam = new NpgsqlParameter("@player_id", NpgsqlTypes.NpgsqlDbType.Uuid);
        cmd.Parameters.Add(playerIdParam);

        var trackerParam = new NpgsqlParameter("@tracker", NpgsqlTypes.NpgsqlDbType.Text);
        cmd.Parameters.Add(trackerParam);

        var timeSpentParam = new NpgsqlParameter("@time_spent", NpgsqlTypes.NpgsqlDbType.Interval);
        cmd.Parameters.Add(timeSpentParam);

        await cmd.PrepareAsync();

        foreach (var playTime in playTimes)
        {
            playerIdParam.Value = playTime.PlayerId;
            trackerParam.Value = playTime.Tracker;
            timeSpentParam.Value = playTime.TimeSpent;

            await cmd.ExecuteNonQueryAsync();
        }
    }
}