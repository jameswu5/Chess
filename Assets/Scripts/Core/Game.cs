using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Game : MonoBehaviour
{
    public Board boardPrefab;
    public Board board;

    public Player whitePlayer;
    public Player blackPlayer;

    public new Camera camera;
    public static AudioSource captureSound;
    public static AudioSource moveSound;
    
    public static Text endOfGameText;
    public static Text resultText;

    public enum PlayerType { Human, Bot }

    public Human human;
    public Bot bot;

    private void Start()
    {
        MoveCamera();
        GetSounds();
        GetTexts();
        CreateBoard();

        human = new Human();
        bot = new Bot();

        StartNewGamePlayerVsPlayer();

        Perft.Test(board, 2);
    }


    private void Update()
    {
        if (board.gameResult == Board.Result.Playing)
        {
            if (board.turn == Piece.White)
            {
                whitePlayer.Update();
            }
            else
            {
                blackPlayer.Update();
            }
        }
    }

    public static void PlayMoveSound(bool isCapture)
    {
        if (isCapture)
        {
            captureSound.Play();
        }
        else
        {
            moveSound.Play();
        }
    }

    private void MoveCamera()
    {
        camera.transform.position = new Vector3(3.5f, 3.5f, -10);
    }

    private void GetSounds()
    {
        captureSound = GameObject.FindGameObjectWithTag("CaptureSound").GetComponent<AudioSource>();
        moveSound = GameObject.FindGameObjectWithTag("MoveSound").GetComponent<AudioSource>();

    }

    private void GetTexts()
    {
        endOfGameText = GameObject.FindGameObjectWithTag("EndOfGameText").GetComponent<Text>();
        resultText = GameObject.FindGameObjectWithTag("ResultText").GetComponent<Text>();
    }

    private void CreateBoard()
    {
        board = Instantiate(boardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        board.Initialise();
    }

    private void StartNewGame(PlayerType whitePlayerType, PlayerType blackPlayerType)
    {   
        board.ResetBoard();

        whitePlayer = whitePlayerType == PlayerType.Human ? human : bot;
        blackPlayer = blackPlayerType == PlayerType.Human ? human : bot;
    }


    public void StartNewGamePlayerVsPlayer()
    {
        StartNewGame(PlayerType.Human, PlayerType.Human);
    }

    public void StartNewGamePlayerVsBot()
    {
        StartNewGame(PlayerType.Human, PlayerType.Bot);
    }

    public void StartNewGameBotVsBot()
    {
        StartNewGame(PlayerType.Bot, PlayerType.Bot);
    }

    public static void UpdateEndOfGameScreen(Board.Result gameResult, int turn)
    {
        if (gameResult == Board.Result.Playing)
        {
            endOfGameText.text = "";
            resultText.text = "";
        }
        else if (gameResult == Board.Result.Checkmate)
        {
            endOfGameText.text = "Checkmate";
            resultText.text = turn == Piece.White ? "0 - 1" : "1 - 0";
        }
        else
        {
            resultText.text = "1/2 - 1/2";
            switch (gameResult)
            {
                case Board.Result.FiftyMove:
                    endOfGameText.text = "50 move rule";
                    break;
                case Board.Result.Insufficient:
                    endOfGameText.text = "Insufficient material";
                    break;
                case Board.Result.Stalemate:
                    endOfGameText.text = "Stalemate";
                    break;
                case Board.Result.Threefold:
                    endOfGameText.text = "Threefold repetition";
                    break;
                default:
                    endOfGameText.text = "Unidentified";
                    break;
            }
        }
    }



}
