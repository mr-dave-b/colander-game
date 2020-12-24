using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using colander_game.Models;
using Microsoft.AspNetCore.Http;
using colander_game.Services;
using Newtonsoft.Json;

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

            return View(new GameRenderModel
            {
                User = await _sessionService.GetUserData(userId),
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
    }
}
