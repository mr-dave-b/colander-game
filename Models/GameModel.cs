using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace colander_game.Models
{
    public class GameModel
    {
        public GameModel()
        {
        }

        public GameModel(string gameId, string creatorId)
        {
            GameId = gameId.FormatGameId();
            CreatorId = creatorId.FormatGameId();
            ActivePlayerId = "";

            Teams = new List<Team>();
            ColanderPapers = new List<string>();
            AllPapers = new List<string>();

            // Players = new List<(string sessionId, string userName)>();
        }

        [JsonPropertyName("id")]
        public string GameId { get; set; }

        public string PartitionId => "Beta";

        public string CreatorId { get; set; }

        public string ActivePlayerId { get; set; }

        public List<string> ColanderPapers { get; set; }

        public List<string> AllPapers { get; set; }

        public List<Team> Teams { get; set; }
    }
}