using GameSessionService.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameSessionService.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GameSessionController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameSessionController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGames()
        {
            var games = await _gameService.GetAllAsync();
            return Ok(games);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetGameById(Guid id)
        {
            var game = await _gameService.GetByIdAsync(id);

            if (game == null)
            {
                return NotFound();
            }

            return Ok(game);
        }
    }
}
