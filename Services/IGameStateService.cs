using System.Threading.Tasks;
using colander_game.Models;

namespace colander_game.Services
{
    public interface IGameStateService
    {
        Task<GameModel> GetGameAsync(string gameId, string userId);

        Task<GameModel> JoinTeamAsync(string gameId, string teamName, UserModel user);

        Task<GameModel> DeleteTeamAsync(string gameId, string teamName, string userId);
        
        Task<GameModel> AddNewPaperAsync(string gameId, string paperWords, string userId);

        Task<GameModel> DrawAPaper(string gameId, UserModel user);

        Task<GameModel> EndPlayerTurn(string gameId, string userId);

        Task<GameModel> SetPlayerReady(string gameId, string userId, bool ready = true);     
    }
}