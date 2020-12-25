using System;
using System.Collections.Generic;

namespace colander_game.Models
{
    public class Team
    {
        public Team(string teamName, UserModel firstPlayer)
        {
            Name = teamName;
            Score = 0;
            Players = new List<UserModel>();
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
        
        public List<UserModel> Players { get; set; }
    }
}