using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Azure.Cosmos;
using colander_game.Models;
using Microsoft.AspNetCore.Http;

namespace colander_game.Services
{
    public class SessionService : ISessionService
    {
        private IStorageService _storage;

        public SessionService(IStorageService storage)
        {
            _storage = storage;
        }

        public async Task<UserModel> GetUserData(string sessionId)
        {
            var container = await _storage.GetUserContainer();

            try
            {
                // Read the user if it exists.  
                UserModel model = await container.ReadItemAsync<UserModel>(sessionId, new PartitionKey("Beta"));
                return model;
            }
            catch(CosmosException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                // User doesn't exist
                return null;
            }
        }

        public string GetUserId(HttpRequest request, HttpResponse response)
        {
            if (request.Cookies["sesh"] != null)
            {
                return request.Cookies["sesh"].FormatGameId();
            }

            string sesh = Guid.NewGuid().ToString().FormatGameId();

            response.Cookies.Append("sesh", sesh);
            
            return sesh;
        }

        public async Task SaveUserData(UserModel user)
        {
            var container = await _storage.GetUserContainer();

            await container.UpsertItemAsync<UserModel>(user, new PartitionKey(user.PartitionId));
        }
    }

    public interface ISessionService
    {
        // Gets/created session ID cookie
        string GetUserId(HttpRequest request, HttpResponse response);
        
        Task SaveUserData(UserModel user);

        Task<UserModel> GetUserData(string sessionId);
    }
}
