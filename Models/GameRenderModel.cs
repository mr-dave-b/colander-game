using System;
using System.Collections.Generic;
using System.Linq;

namespace colander_game.Models
{
    public class GameRenderModel
    {
        public UserModel User { get; set; }
        public GameModel Game { get; set ; }

        public Team UsersTeam()
        {
            return Game?.CurrentTeam(User?.UserId);
        }
    }
}
