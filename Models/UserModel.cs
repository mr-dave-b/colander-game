using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace colander_game.Models
{
    public class UserModel
    {

        [JsonPropertyName("id")]
        public string UserId { get; set; }

        public string SessionId { get; set; }

        public string UserName { get; set; }

        public string PartitionId => "Beta";
    }
}