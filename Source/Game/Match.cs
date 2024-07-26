using System.Collections;
using System.Collections.Generic;

public class Match
{
    const int games = 20;

    public Player.Type player1;
    public Player.Type player2;

    int player1Wins;
    int player2Wins;
    int draws;

    public int gameNumber;
    public bool isActive;

    public enum GameResult { WhiteWins, Draw, BlackWins }

    public event System.Action StartGame;

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

    public void SetBots(Player.Type bot1, Player.Type bot2)
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

        Debug.Log($"{player1Wins} | {draws} | {player2Wins}");

    }

    public void EndOfMatch()
    {
        isActive = false;
    }
}
