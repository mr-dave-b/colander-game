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
                Task task = SaveToStorage(game);

                // TODO: Unlocking???
            }
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