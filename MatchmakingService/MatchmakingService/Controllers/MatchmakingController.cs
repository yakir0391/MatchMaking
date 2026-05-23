using MatchmakingService.Models;
using MatchmakingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace MatchmakingService.Controllers
{
    [ApiController]
    [Route("api/matchmaking")]
    public class MatchmakingController : ControllerBase
    {
        private readonly IMatchmakingQueue _queue;
        public MatchmakingController(IMatchmakingQueue queue)
        {
            _queue = queue;
        }

        [HttpPost("join")]
        public IActionResult Join([FromBody] PlayerQueueEntry player)
        {
            var added = _queue.Enqueue(player);

            if(!added)
            {
                return BadRequest("Player is already in the queue.");
            }
            return Ok("Player added to the queue.");
        }

        [HttpPost("leave/{playerId}")]
        public IActionResult Leave(string playerId)
        {
            var removed = _queue.Remove(playerId);

            if(!removed)
            {
                return BadRequest("Player is not in the queue.");
            }
            return Ok("Player removed from the queue.");
        }
    }
}
