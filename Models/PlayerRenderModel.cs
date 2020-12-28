namespace colander_game.Models
{
    public class PlayerRenderModel
    {
        public PlayerRenderModel(Player player, string currentUserId, string nextUpPlayer)
        {
            Player = player;
            CurrentUserId = currentUserId;
            NextUpPlayer = nextUpPlayer;
        }

        public Player Player { get; set; }
        public string CurrentUserId { get; set ; }
        public string NextUpPlayer { get; set; }
    }
}
