using System;

namespace colander_game.Models
{
    public class GameModel
    {
        public GameModel(string gameId)
        {
            GameId = gameId.FormatGameId();
        }

        public string GameId { get;}

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}