using System;
using System.Collections.Generic;
using System.Linq;

namespace colander_game.Models
{
    public class GameRenderModel
    {
        public UserModel User { get; set; }
        public GameModel Game { get; set ; }

        public Team CurrentTeam()
        {
            return Game?.Teams?.FirstOrDefault(t => t.Players != null && t.Players.Any(p => p.UserId == User?.UserId));
        }
    }
}
