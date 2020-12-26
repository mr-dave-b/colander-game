using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace colander_game.Models
{
    public class Player
    {
        public Player() { }

        public Player(UserModel user)
        {
            if (user != null)
            {
                UserId = user.UserId;
                UserName = user.UserName;
                Ready = false;
            }
        }

        [JsonPropertyName("id")]
        public string UserId { get; set; }

        public string UserName { get; set; }

        public bool Ready { get; set; }
    }
}