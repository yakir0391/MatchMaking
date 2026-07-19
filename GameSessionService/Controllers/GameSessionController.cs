using GameSessionService.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameSessionService.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GameSessionController : ControllerBase
    {
        private readonly GameService _gameService;

        public GameSessionController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public IActionResult GetAllGames()
        {
            return Ok(_gameService.GetAll());
        }
    }
}
