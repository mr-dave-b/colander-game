using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using colander_game.Models;
using colander_game.Services;

namespace colander_game.Controllers
{
    public class GameController : Controller
    {
        private ISessionService _sessionService;
        private IGameStateService _gameService;

        public GameController(ISessionService sessionService, IGameStateService gameService)
        {
            _sessionService = sessionService;
            _gameService = gameService;
        }

        [Route("game/{gameId}")]
        public async Task<IActionResult> Index(string gameId)
        {
            gameId = gameId.FormatGameId();
            var userId = _sessionService.GetUserId(Request, Response);

            var game = await _gameService.GetGameAsync(gameId, userId);
            if (game.IsGameOver)
            {
                return Redirect($"/gameover/{gameId}");
            }

            var user = await _sessionService.GetUserData(userId);
            if (string.IsNullOrWhiteSpace(user?.UserName))
            {
                return Redirect("/");
            }

            return View(new GameRenderModel
            {
                User = user,
                Game = game
            });
        }

        [Route("gameover/{gameId}")]
        public async Task<IActionResult> GameOver(string gameId)
        {
            gameId = gameId.FormatGameId();
            var userId = _sessionService.GetUserId(Request, Response);

            var game = await _gameService.GetGameAsync(gameId, userId);

            if (!game.IsGameOver)
            {
                return Redirect($"/game/{gameId}");
            }

            var user = await _sessionService.GetUserData(userId);

            return View(new GameRenderModel
            {
                User = user,
                Game = game
            });
        }

        [Route("game/{gameId}/jointeam")]
        [HttpPost]
        public async Task<IActionResult> JoinTeam(string gameId, [FromForm] string teamName)
        {
            gameId = gameId.FormatGameId();

            var userId = _sessionService.GetUserId(Request, Response);
            var user = await _sessionService.GetUserData(userId);

            var game = await _gameService.JoinTeamAsync(gameId, teamName, user);

            return Redirect($"/game/{gameId}");
        }

        [Route("game/{gameId}/deleteteam")]
        [HttpPost]
        public async Task<IActionResult> DeleteTeam(string gameId, [FromForm] string teamName)
        {
            gameId = gameId.FormatGameId();

            var userId = _sessionService.GetUserId(Request, Response);

            var game = await _gameService.DeleteTeamAsync(gameId, teamName, userId);

            return Redirect($"/game/{gameId}");
        }

        [Route("game/{gameId}/addpaper")]
        [HttpPost]
        public async Task<IActionResult> AddPaper(string gameId, [FromForm] string newPaper)
        {
            gameId = gameId.FormatGameId();

            var userId = _sessionService.GetUserId(Request, Response);

            var game = await _gameService.AddNewPaperAsync(gameId, newPaper, userId);

            return Redirect($"/game/{gameId}");
        }

        [Route("game/{gameId}/getpaper")]
        [HttpPost]
        public async Task<IActionResult> GetPaper(string gameId)
        {
            gameId = gameId.FormatGameId();

            var userId = _sessionService.GetUserId(Request, Response);
            var user = await _sessionService.GetUserData(userId);

            var game = await _gameService.DrawAPaper(gameId, user);

            return Redirect($"/game/{gameId}");
        }

        [Route("game/{gameId}/endturn")]
        [HttpPost]
        public async Task<IActionResult> EndTurn(string gameId)
        {
            gameId = gameId.FormatGameId();

            var userId = _sessionService.GetUserId(Request, Response);
            //var user = await _sessionService.GetUserData(userId);

            var game = await _gameService.EndPlayerTurn(gameId, userId);

            return Redirect($"/game/{gameId}");
        }

        [Route("game/{gameId}/ready")]
        [HttpPost]
        public async Task<IActionResult> Ready(string gameId)
        {
            gameId = gameId.FormatGameId();
            var userId = _sessionService.GetUserId(Request, Response);
            await _gameService.SetPlayerReady(gameId, userId);
            return Redirect($"/game/{gameId}");
        }

        [Route("game/{gameId}/notready")]
        [HttpPost]
        public async Task<IActionResult> NotReady(string gameId)
        {
            gameId = gameId.FormatGameId();
            var userId = _sessionService.GetUserId(Request, Response);
            await _gameService.SetPlayerReady(gameId, userId, ready: false);
            return Redirect($"/game/{gameId}");
        }
    }
}
