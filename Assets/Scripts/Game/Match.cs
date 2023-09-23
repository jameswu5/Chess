using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match : MonoBehaviour
{
    const int games = 100;

    public Bot.BotType player1;
    public Bot.BotType player2;

    int player1Wins;
    int player2Wins;
    int draws;

    public bool isActive = false;

    public int gameNumber;

    public enum GameResult { WhiteWins, Draw, BlackWins }

    public event System.Action StartGame;

    public void StartMatch()
    {
        isActive = true;

        player1 = Bot.BotType.Random;
        player2 = Bot.BotType.Version1;

        player1Wins = 0;
        player2Wins = 0;
        draws = 0;
        StartGame.Invoke();
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
