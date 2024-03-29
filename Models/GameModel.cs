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

        public DateTime? RoundStartTime { get; set; }

        public int NextTeamToPlayInt { get; set; }

        [JsonIgnore]
        public Team NextTeamToPlay => Teams[NextTeamToPlayInt];
        
        [JsonPropertyName("id")]
        public string GameId { get; set; }

        public string PartitionId => "Beta";

        public string CreatorId { get; set; }

        public UserModel ActivePlayer { get; set; }

        public PaperModel ActivePaper { get; set; }

        public List<PaperModel> ColanderPapers { get; set; }

        public List<PaperModel> PlayedPapers { get; set; }

        public List<Team> Teams { get; set; }

        public void StartTheGame(string teamName)
        {
            RoundNumber = 1;
            var currentTeamIndex = Teams.FindIndex(t => t.Name == teamName);
            if (currentTeamIndex > -1)
            {
                NextTeamToPlayInt = currentTeamIndex;
            }
        }

        // End turn of the current active player
        public void EndPlayersGo(bool endOfRound = false)
        {
            // Remove the active paper and remove the timer
            ActivePaper = null;
            RoundStartTime = null;

            if (ActivePlayer != null)
            {
                var team = GetPlayersTeam(ActivePlayer.UserId);
                team.NextPlayer = team.Players.FindIndex(p => p.UserId == ActivePlayer.UserId) + 1;
                if (team.NextPlayer >= team.Players.Count)
                {
                    team.NextPlayer = 0;
                }

                if (!endOfRound)
                {
                    // When it's not the end of the round, play moves to the next team
                    var currentTeamIndex = Teams.FindIndex(t => t.Name == team.Name);
                    if (currentTeamIndex > -1)
                    {
                        // Move play to the next team
                        currentTeamIndex++;

                        // TODO: Skip empty teams?
                        if (currentTeamIndex >= Teams.Count)
                        {
                            currentTeamIndex = 0;
                        }
                        NextTeamToPlayInt = currentTeamIndex;
                    }
                }

                ActivePlayer = null;
            }
        }

        [JsonIgnore]
        public bool GameCanStart
        {
            get
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
        }

        [JsonIgnore]
        public bool IsGameOver =>
            (RoundNumber >= 3 && ColanderPapers != null && ColanderPapers.Count == 0);

        public Team GetPlayersTeam(string userId)
        {
            return Teams?.FirstOrDefault(t => t.Players != null && t.Players.Any(p => p.UserId == userId));
        }

        [JsonIgnore]
        public int TimeLeft
        {
            get
            {
                if (RoundStartTime == null)
                {
                    return 0;
                }
                int timeLeft = (int)RoundStartTime.Value.AddSeconds(60).Subtract(DateTime.UtcNow).TotalSeconds;
                if (timeLeft < 1)
                {
                    return 0;
                }
                return timeLeft;
            }
        }
    }
}