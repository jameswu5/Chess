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

    public Camera camera;
    public static AudioSource captureSound;
    public static AudioSource moveSound;

    public static Text endOfGameText;
    public static Text resultText;
    public static Button resetButton;

    public enum PlayerType { Human, Bot }

    private void Start()
    {
        MoveCamera();
        GetSounds();
        GetTexts();
        CreateBoard();

        NewGame(PlayerType.Human, PlayerType.Bot);
    }


    private void Update()
    {
        if (whitePlayer == null)
        {
            Debug.Log("no white player");
        }
        whitePlayer.Update();
        blackPlayer.Update();
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

    public void MoveCamera()
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

    public void NewGame(PlayerType whitePlayerType, PlayerType blackPlayerType)
    {
        // board.ResetGame();
        whitePlayer = whitePlayerType == PlayerType.Human ? new Human() : new Bot();
        blackPlayer = blackPlayerType == PlayerType.Human ? new Human() : new Bot();
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
