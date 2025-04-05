using Microsoft.AspNetCore.Mvc;
using PlayTimeManager.Attributes;
using PlayTimeManager.Models.Database;

namespace PlayTimeManager.Controllers;

[ApiController, Route("api/playtime"), RemoteAuth("rw")]
public class PlayTimeController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public PlayTimeController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlayTime>>> GetPlayTime([FromQuery] Guid playerId) 
    {
        var playtimes = await _dbContext.GetPlayTimeAsync(playerId).ToListAsync();
        return playtimes;
    }

    [HttpPost]
    public async Task<ActionResult> PostPlayTime([FromBody] List<PlayTime> playtimes)
    {
        await _dbContext.SavePlayTimeAsync(playtimes);
        return Ok();
    }
}