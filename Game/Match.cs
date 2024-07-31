
namespace Chess.Game;

public class Match
{
    const int games = 20;

    public Player.Player.Type player1;
    public Player.Player.Type player2;

    private int player1Wins;
    private int player2Wins;
    private int draws;

    public int gameNumber;
    public bool isActive;

    public enum GameResult { WhiteWins, Draw, BlackWins }

    public event Action StartGame;

    public Match()
    {
        isActive = false;
    }

    public void StartMatch()
    {
        isActive = true;

        player1Wins = 0;
        player2Wins = 0;
        draws = 0;
        gameNumber = 0;

        StartGame.Invoke();
    }

    public void SetBots(Player.Player.Type bot1, Player.Player.Type bot2)
    {
        player1 = bot1;
        player2 = bot2;
    }

    public void ReportResult(GameResult result)
    {
        // Player1 plays white in even indices of gameNumber

        if (result == GameResult.WhiteWins)
        {
            if ((gameNumber & 1) == 0) 
            {
                player1Wins++;
            }
            else
            {
                player2Wins++;
            }
        }
        else if (result == GameResult.Draw)
        {
            draws++;
        }
        else
        {
            if ((gameNumber & 1) == 0)
            {
                player2Wins++;
            }
            else
            {
                player1Wins++;
            }
        }

        gameNumber++;

        if (gameNumber == games)
        {
            EndOfMatch();
        }
        else
        {
            StartGame.Invoke();
        }

        Console.WriteLine($"{player1Wins} | {draws} | {player2Wins}");
    }

    public void EndOfMatch()
    {
        isActive = false;
    }

    public double GetScore(bool isWhite)
    {
        bool isPlayer1 = isWhite ^ (gameNumber % 2 == 1);
        return (isPlayer1 ? player1Wins : player2Wins) + 0.5 * draws;
    }
}