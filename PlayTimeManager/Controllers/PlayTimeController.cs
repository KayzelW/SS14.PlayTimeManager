using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayTimeManager.Attributes;
using PlayTimeManager.Models.Database;

namespace PlayTimeManager.Controllers;

[ApiController, Route("api/playtime")]
public class PlayTimeController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PlayTimeController> _logger;

    public PlayTimeController(AppDbContext dbContext, ILogger<PlayTimeController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet, Authorize(Policy = "ro")]
    public async Task<PlayTime[]> GetPlayTime([FromQuery] Guid playerId)
    {
        _logger.LogInformation($"GetPlayTime with playerId: {playerId}");
        var playtimes = await _dbContext.GetPlayTimeAsync(playerId).ToArrayAsync();

        return playtimes;
    }

    [HttpPost, Authorize(Policy = "rw")]
    public async Task<ActionResult> PostPlayTime([FromBody] PlayTime[] playtimes)
    {
        _logger.LogInformation(
            $"PostPlayTime playerId: {string.Join(", ", playtimes.DistinctBy(x => x.PlayerId)
                .Select(x => $"{x.PlayerId}"))}");
        await _dbContext.SavePlayTimeAsync(playtimes);
        return Ok();
    }
}