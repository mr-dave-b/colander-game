namespace colander_game.Models
{
    public class PlayerRenderModel
    {
        public PlayerRenderModel(Player player, string currentUserId, string nextUpPlayer, bool isGameOver)
        {
            Player = player;
            CurrentUserId = currentUserId;
            NextUpPlayer = isGameOver ? null : nextUpPlayer;
        }

        public Player Player { get; set; }
        public string CurrentUserId { get; set ; }
        public string NextUpPlayer { get; set; }
    }
}
