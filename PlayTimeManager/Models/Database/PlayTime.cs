namespace PlayTimeManager.Models.Database;

public sealed class PlayTime
{
    public int Id { get; set; }

    public Guid PlayerId { get; set; }

    public string Tracker { get; set; } = null!;

    public TimeSpan TimeSpent { get; set; }
}