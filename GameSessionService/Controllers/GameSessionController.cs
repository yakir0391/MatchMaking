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
    }
}
