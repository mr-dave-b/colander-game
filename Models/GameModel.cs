using System;
using System.Collections.Generic;
using System.Linq;
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

        public int NextTeamToPlayInt { get; set; }

        public Team NextTeamToPlay => Teams[NextTeamToPlayInt];

        public void EndPlayersGo(string teamName, bool endOfRound = false)
        {
            ActivePlayer = null;
            ActivePaper = null;
            if (!endOfRound)
            {
                var currentTeamIndex = Teams.FindIndex(t => t.Name == teamName);
                if (currentTeamIndex > -1)
                {
                    // Move play to the next team
                    currentTeamIndex++;

                    // TODO: Skip team if it is empty
                    if (currentTeamIndex >= Teams.Count)
                    {
                        currentTeamIndex = 0;
                    }
                    NextTeamToPlayInt = currentTeamIndex;
                }
            }
        }

        /*
        public string WhichTeamIsUp()
        {
            if (Teams == null)
            {
                return "";
            }

            var nextTeamNum = LastTeamToPlay + 1;
            Team team = null;
            int goneRound = 0;
            while (goneRound < 2)
            {
                if (Teams.Count <= nextTeamNum)
                {
                    goneRound++;
                    nextTeamNum = 0;
                    continue;
                }

                team = Teams[nextTeamNum];
                if (team.Players == null || team.Players.Count == 0)
                {
                    nextTeamNum++;
                    continue;
                }
                break;
            }

            return team.Name;
        }
        */
        
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
            if (Teams.Any(t => t.Players == null || t.Players.Count == 0))
            {
                return false;
            }
            if (ColanderPapers == null || ColanderPapers.Count < Teams.Count*2)
            {
                return false;
            }
            return true;
        }

        public Team CurrentTeam(string userId)
        {
            return Teams?.FirstOrDefault(t => t.Players != null && t.Players.Any(p => p.UserId == userId));
        }
    }
}