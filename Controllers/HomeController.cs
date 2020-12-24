using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using colander_game.Models;
using System.Text.RegularExpressions;
using colander_game.Services;

namespace colander_game.Controllers
{
    public class HomeController : Controller
    {
        private ISessionService _sessionService;

        public HomeController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _sessionService.GetUserId(Request, Response);
            var userModel = await _sessionService.GetUserData(userId);
            
            return View(new GameRenderModel
            {
                    User = userModel
            });
        }

        [HttpPost]
        public async Task<IActionResult> Index(
            [FromForm] string gameId,
            [FromForm] string yourName)
        {

            if (!string.IsNullOrEmpty(gameId))
            {
                gameId = gameId.FormatGameId();
                return Redirect($"/game/{gameId}");
            }

            var userId = _sessionService.GetUserId(Request, Response);
                        
            if (!string.IsNullOrEmpty(yourName))
            {
                var model = new UserModel
                {
                    UserId = userId,
                    UserName = yourName,
                    SessionId = userId
                };

                var task = _sessionService.SaveUserData(model);
                
                return Redirect("/");
            }

            return await Index();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
