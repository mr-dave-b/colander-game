using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Cosmos;
using colander_game.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace colander_game.Services
{
    public class GameStateService : IGameStateService
    {
        static Random randomGen = new Random();

        private IStorageService _storage;

        public GameStateService(IStorageService storage)
        {
            _storage = storage;
        }

        // Fetches / creates a game
        public async Task<GameModel> GetGameAsync(string gameId, string userId)
        {
            // Load the game from Azure storage
            var model = await LoadFromStorage(gameId);

            if (model == null)
            {
                // Create a new game
                model = new GameModel(gameId, userId);
            }
            else
            {
                // Any time the game is loaded, check if someone's turn has ended
                await CheckForTimeout(model);
            }
            
            return model;
        }

        public async Task<GameModel> JoinTeamAsync(string gameId, string teamName, UserModel user)
        {
            var game = await GetGameAsync(gameId, user.SessionId);

            if (string.IsNullOrWhiteSpace(teamName))
            {
                // Ignore empty team name
                return game;
            }

            teamName = teamName.Trim();

            // TODO: Locking

            var myTeam = game.Teams.FirstOrDefault(t => t.Players.Any(p => p.UserId == user.UserId));
            if (myTeam != null)
            {
                // Player is already in a team
                if (game.RoundNumber > 0)
                {
                    // Can't change team once the game has started
                    return game;
                }

                // Remove player from current team
                myTeam.Players.RemoveAll(x => x.UserId == user.UserId);
            }

            // Add player to the new team
            myTeam = game.Teams.FirstOrDefault(t => t.Name.Trim().ToUpperInvariant() == teamName.ToUpperInvariant());
            var player = new Player(user);
            if (myTeam != null)
            {
                myTeam.Players.Add(player);
            }
            else
            {
                myTeam = new Team(teamName, player);
                game.Teams.Add(myTeam);
            }

            // Wait for the save since we will reload the page immediately
            await SaveToStorage(game);

            // TODO: Unlocking???
            return game;
        }

        public async Task<GameModel> DeleteTeamAsync(string gameId, string teamName, string userId)
        {
            var game = await GetGameAsync(gameId, userId);

            if (string.IsNullOrWhiteSpace(teamName))
            {
                // Ignore empty team name
                return game;
            }

            if (game.RoundNumber > 0)
            {
                // Can't delete team when game has started
                return game;
            }

            teamName = teamName.Trim();

            // TODO: Locking

            // Find team and delete if empty
            var myTeam = game.Teams.FirstOrDefault(t => t.Name.Trim().ToUpperInvariant() == teamName.ToUpperInvariant());
            if (myTeam != null)
            {
                // TODO: Don't delete a team with platers?
                game.Teams.Remove(myTeam);
            }

            // Wait for the save since we will reload the page immediately
            await SaveToStorage(game);

            // TODO: Unlocking???
            return game;
        }

        public async Task<GameModel> AddNewPaperAsync(string gameId, string paperWords, string userId)
        {
            var game = await GetGameAsync(gameId, userId);

            if (game.RoundNumber > 0)
            {
                // Can't add a new paper when the game has started
                return game;
            }

            if (!string.IsNullOrEmpty(paperWords))
            {
                // TODO: Locking
                if (game.ColanderPapers == null)
                {
                    game.ColanderPapers = new List<PaperModel>();
                }

                if (!game.ColanderPapers.Any(p => p.AuthorUserId == userId && p.Words == paperWords))
                {
                    var newPaper = new PaperModel
                    {
                        Words = paperWords,
                        AuthorUserId = userId
                    };

                    game.ColanderPapers.Add(newPaper);
                }

                // Wait for the save since we will reload the page immediately
                await SaveToStorage(game);

                // TODO: Unlocking???
            }
            return game;
        }

        // Takes a new paper from the colander.
        // Starts the round if it wasn't already.
        // Scores a point if a paper was already drawn
        public async Task<GameModel> DrawAPaper(string gameId, UserModel user)
        {
            var game = await GetGameAsync(gameId, user.UserId);
            DateTime nowUtc = DateTime.UtcNow;

            if (game.ActivePlayer != null && game.ActivePlayer.UserId != user.UserId)
            {
                // Someone else is presenting! Don't allow this
                return null;
            }

            if (!game.GameCanStart || game.IsGameOver)
            {
                // Can't start yet or game over
                return null;
            }

            // TODO: Locking

            if (game.RoundNumber == 0)
            {
                // Any player on any team can start the game
                game.StartTheGame(game.GetPlayersTeam(user.UserId)?.Name);
            }
            else
            {
                if (game.GetPlayersTeam(user.UserId)?.Name == game.NextTeamToPlay?.Name)
                {
                    // Player is on the correct team to play
                }
                else
                {
                    // Player is on the wrong team
                    return null;
                }
            }

            if (game.ActivePlayer == null)
            {
                // New player - start the timer
                game.RoundStartTime = nowUtc;
            }

            game.ActivePlayer = user;

            if (game.ColanderPapers.Count == 0)
            {
                // The colander is empty - start a round by moving all the papers into the colander
                game.ColanderPapers.AddRange(game.PlayedPapers);
                game.PlayedPapers = new List<PaperModel>();
                game.ActivePaper = null;
                game.RoundNumber++;
            }
            
            if (game.RoundStartTime.HasValue && game.RoundStartTime.Value.AddSeconds(70).CompareTo(nowUtc) < 0)
            {
                // Outside 70 seconds - can't score a point or draw a paper
                game.EndPlayersGo();
            }
            else
            {
                if (game.ActivePaper != null)
                {
                    // There is already a paper drawn, remove from colander and score 1 point
                    var found = game.ColanderPapers.RemoveAll(p => p.Words == game.ActivePaper.Words);
                    if (found > 0)
                    {
                        if (game.PlayedPapers == null)
                        {
                            game.PlayedPapers = new List<PaperModel>();
                        }
                        game.PlayedPapers.Add(game.ActivePaper);
                    }
                    var team = game.Teams.FirstOrDefault(t => t.Players.Any(p => p.UserId == user.UserId));
                    if (team != null)
                    {
                        team.Score += 1;
                    }
                }

                if (game.ColanderPapers.Count > 0)
                {
                    if (game.RoundStartTime.HasValue && game.RoundStartTime.Value.AddSeconds(60).CompareTo(DateTime.UtcNow) > 0)
                    {
                        // Draw new paper at random
                        int r = randomGen.Next(game.ColanderPapers.Count);
                        game.ActivePaper = game.ColanderPapers[r];
                    }
                    else
                    {
                        // Time is up - end users go
                        game.EndPlayersGo();
                    }
                }
                else
                {
                    // Round is finished - end users go and end teams go
                    game.EndPlayersGo(endOfRound: true);
                }
            }

            // Wait for the save since we will reload the page immediately
            await SaveToStorage(game);

            // TODO: Unlocking???

            return game;
        }

        public async Task<GameModel> EndPlayerTurn(string gameId, string userId)
        {
            var game = await GetGameAsync(gameId, userId);

            if (!game.GameCanStart || game.IsGameOver)
            {
                // Can't start yet or game over
                return null;
            }

            if (game.ActivePlayer == null || game.ActivePlayer.UserId != userId)
            {
                // Someone else is presenting! Don't allow this
                return null;
            }

            // TODO: Locking

            // Remove the active player and the active paper
            game.EndPlayersGo();

            // Save the updated game state to DB
            Task task = SaveToStorage(game);

            // TODO: Unlocking???

            return game;
        }

        public async Task<GameModel> SetPlayerReady(string gameId, string userId, bool ready = true)
        {
            var game = await GetGameAsync(gameId, userId);

            if (game.RoundNumber == 0)
            {
                // TODO: Locking

                var team = game.GetPlayersTeam(userId);
                if (team != null)
                {
                    var player = team.Players.First(p => p.UserId == userId);
                    player.Ready = ready;

                    // Wait for the save since we will reload the page immediately
                    await SaveToStorage(game);
                    // TODO: Unlocking???
                }
            }
            return game;
        }
        
        private async Task CheckForTimeout(GameModel game)
        {
            // if timer is at more than 70 seconds
            if (game.RoundStartTime.HasValue && game.RoundStartTime.Value.AddSeconds(70).CompareTo(DateTime.UtcNow) < 0)
            {
                // End users turn
                game.EndPlayersGo();

                // Save to database
                await SaveToStorage(game);
            }
        }

        private async Task<GameModel> LoadFromStorage(string gameId)
        {
            var container = await _storage.GetGameContainer();

            try
            {
                // Read the game if it exists.  
                GameModel model = await container.ReadItemAsync<GameModel>(gameId, new PartitionKey("Beta"));
                return model;
            }
            catch(CosmosException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                // Game doesn't exist
                return null;
            }
        }

        private async Task SaveToStorage(GameModel model)
        {
            var container = await _storage.GetGameContainer();

            var response = await container.UpsertItemAsync<GameModel>(model, new PartitionKey(model.PartitionId));
        }
    }
}