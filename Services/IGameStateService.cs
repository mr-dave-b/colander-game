using System.Threading.Tasks;
using colander_game.Models;

namespace colander_game.Services
{
    public interface IGameStateService
    {
        Task<GameModel> GetGameAsync(string gameId, string userId);

        Task<GameModel> JoinTeamAsync(string gameId, string teamName, UserModel user);
        
        Task<GameModel> AddNewPaperAsync(string gameId, string paperWords, string userId);
    }
}