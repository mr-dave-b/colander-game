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

        public bool UserIsReady()
        {
            var usersTeam = UsersTeam();
            if (usersTeam != null)
            {
                var player = usersTeam.Players.First(p => p.UserId == User.UserId);
                return player.Ready;
            }
            return false;
        }

        public int ReadyPlayersCount()
        {
            return Game?.Teams?.Sum(t => t.Players.Count(p => p.Ready)) ?? 0;
        }
        public int NotReadyPlayersCount()
        {
            return Game?.Teams?.Sum(t => t.Players.Count(p => !p.Ready)) ?? 0;
        }
    }
}
