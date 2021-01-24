using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using colander_game.Models;
using colander_game.Services;

namespace colander_game.Controllers
{
    public class GameStatusController : Controller
    {
        private ISessionService _sessionService;
        private IGameStateService _gameService;

        public GameStatusController(ISessionService sessionService, IGameStateService gameService)
        {
            _sessionService = sessionService;
            _gameService = gameService;
        }

        [Route("status/{gameId}")]
        public async Task<IActionResult> Index(string gameId)
        {
            gameId = gameId.FormatGameId();

            var userId = _sessionService.GetUserId(Request, Response);
            var user = await _sessionService.GetUserData(userId);
            var game = await _gameService.GetGameAsync(gameId, userId);

            return View(new GameRenderModel
            {
                User = user ?? new UserModel(),
                Game = game
            });
        }
    }
}