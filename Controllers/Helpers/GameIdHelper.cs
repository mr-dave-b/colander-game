using System.Text.RegularExpressions;

public static class GameIdHelper
{
    public static string FormatGameId(this string gameId)
    {
            Regex invalidCharRemover = new Regex("[^a-z0-9]");
            gameId = invalidCharRemover.Replace(gameId.ToLowerInvariant(), "-");

            Regex duplicateDashRemover = new Regex("[-]{2,}");
            gameId = duplicateDashRemover.Replace(gameId, "-");

            if (gameId.Length  < 1)
            {
                gameId = "-";
            }
            return gameId;
    }
}