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
    public class StorageService : IStorageService
    {
        public const string GameContainerId = "GameModels";
        public const string UserContainerId = "PlayerModels";

        private IConfiguration _configuration;

        private CosmosContainer _gameContainer = null;
        private CosmosContainer _userContainer = null;

        public StorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<CosmosContainer> GetGameContainer()
        {
            if (_gameContainer == null)
            {
                var cosmosEndpoint = _configuration["CosmosEndpoint"];
                var cosmosKey = _configuration["CosmosKey"];
                var databaseId = _configuration["DatabaseId"];

                CosmosClient cosmosClient = new CosmosClient(cosmosEndpoint, cosmosKey);
        
                CosmosDatabase database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

                _gameContainer = await database.CreateContainerIfNotExistsAsync(GameContainerId, "/PartitionId");
            }

            return _gameContainer;
        }

        public async Task<CosmosContainer> GetUserContainer()
        {
            if (_userContainer == null)
            {
                var cosmosEndpoint = _configuration["CosmosEndpoint"];
                var cosmosKey = _configuration["CosmosKey"];
                var databaseId = _configuration["DatabaseId"];

                CosmosClient cosmosClient = new CosmosClient(cosmosEndpoint, cosmosKey);
        
                CosmosDatabase database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

                _userContainer = await database.CreateContainerIfNotExistsAsync(UserContainerId, "/PartitionId");
            }

            return _userContainer;
        }
    }

    public interface IStorageService
    {
        Task<CosmosContainer> GetGameContainer();

        Task<CosmosContainer> GetUserContainer();
    }
}