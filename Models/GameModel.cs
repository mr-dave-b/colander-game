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

            Teams = new List<Team>();
            ColanderPapers = new List<PaperModel>();
            PlayedPapers = new List<PaperModel>();
            Teams.Add(new Team("Synergy", null));
        }

        public int RoundNumber { get; set; }

        [JsonPropertyName("id")]
        public string GameId { get; set; }

        public string PartitionId => "Beta";

        public string CreatorId { get; set; }

        public UserModel ActivePlayer { get; set; }

        public PaperModel ActivePaper { get; set; }

        public List<PaperModel> ColanderPapers { get; set; }

        public List<PaperModel> PlayedPapers { get; set; }

        public List<Team> Teams { get; set; }

        public bool GameCanStart()
        {
            if (RoundNumber > 0)
            {
                return true;
            }
            if (Teams == null || Teams.Count < 2)
            {
                return false;
            }
            if (ColanderPapers == null || ColanderPapers.Count < Teams.Count*2)
            {
                return false;
            }
            return true;
        }
    }
}