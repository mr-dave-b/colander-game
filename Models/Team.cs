using System;
using System.Collections.Generic;

namespace colander_game.Models
{
    public class Team
    {
        public Team(string teamName, Player firstPlayer)
        {
            Name = teamName;
            Score = 0;
            Players = new List<Player>();
            if (firstPlayer != null)
            {
                Players.Add(firstPlayer);
            }
        }
        public Team()
        {
        }

        public string Name { get; set ; }

        public int Score { get; set; }
        
        public List<Player> Players { get; set; }

        public int NextPlayer { get; set; }
    }
}