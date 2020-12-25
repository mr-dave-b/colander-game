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
                
                // Save it to the DB
                await SaveToStorage(model);
            }
            
            return model;
        }

        public async Task<GameModel> JoinTeamAsync(string gameId, string teamName, UserModel user)
        {
            var game = await GetGameAsync(gameId, user.SessionId);

            // TODO: Locking

            var myTeam = game.Teams.FirstOrDefault(t => t.Players.Any(p => p.UserId == user.UserId));
            if (myTeam != null)
            {
                // Remove player from current team
                myTeam.Players.RemoveAll(x => x.UserId == user.UserId);
            }
            // Add player to the new team

            myTeam = game.Teams.FirstOrDefault(t => t.Name == teamName);
            if (myTeam != null)
            {
                myTeam.Players.Add(user);
            }
            else
            {
                myTeam = new Team(teamName, user);
                game.Teams.Add(myTeam);
            }

            // Save the updated game state to DB
            Task task = SaveToStorage(game);

            // TODO: Unlocking???
            return game;
        }

        public async Task<GameModel> AddNewPaperAsync(string gameId, string paperWords, string userId)
        {
            var game = await GetGameAsync(gameId, userId);

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

                // Save the updated game state to DB
                await SaveToStorage(game);

                // TODO: Unlocking???
            }
            return game;
        }

        public async Task<GameModel> DrawAPaper(string gameId, UserModel user)
        {
            var game = await GetGameAsync(gameId, user.UserId);

            if (game.ActivePlayer != null && game.ActivePlayer.UserId != user.UserId)
            {
                // Someone else is presenting! Don't allow this
                return null;
            }

            // TODO: Locking
            game.ActivePlayer = user;

            if (game.RoundNumber > 0)
            {
                if (game.ColanderPapers.Count == 0)
                {
                    // The colander is empty - start a round by moving all the papers into the colander
                    game.ColanderPapers.AddRange(game.PlayedPapers);
                    game.PlayedPapers = new List<PaperModel>();
                    game.RoundNumber++;
                }
                
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
            }
            else
            {
                // Start the first round
                game.RoundNumber = 1;
            }

            if (game.ColanderPapers.Count > 0)
            {
                // Draw new paper at random
                int r = randomGen.Next(game.ColanderPapers.Count);
                game.ActivePaper = game.ColanderPapers[r];
            }
            else
            {
                // Round is finished - end users go
                game.ActivePlayer = null;
                game.ActivePaper = null;
            }

            // Save the updated game state to DB
            Task task = SaveToStorage(game);

            // TODO: Unlocking???

            return game;
        }

        public async Task<GameModel> EndPlayerTurn(string gameId, string userId)
        {
            var game = await GetGameAsync(gameId, userId);

            if (game.ActivePlayer == null || game.ActivePlayer.UserId != userId)
            {
                // Someone else is presenting! Don't allow this
                return null;
            }

            // TODO: Locking

            // Remove the active player and the active paper
            game.ActivePlayer = null;
            game.ActivePaper = null;

            // Save the updated game state to DB
            Task task = SaveToStorage(game);

            // TODO: Unlocking???

            return game;
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