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

    public Clock clockPrefab;
    public Clock clock;
    public const int allowedTime = 300;
    public const int increment = 0;

    private void Start()
    {
        MoveCamera();
        GetSounds();
        GetTexts();
        CreateBoard();

        clock = Instantiate(clockPrefab);
        clock.Initialise(allowedTime, increment);
        clock.ClockTimedOut += TimedOut;

        StartNewGamePlayerVsPlayer();
    }

    private void Update()
    {
        if (board.gameResult == Judge.Result.Playing)
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

    public void PlayMove(int move)
    {
        board.MakeMove(move, true);
        PlayMoveSound(Move.IsCaptureMove(move));
        Debug.Log($"{board.moveNumber}: {Move.GetMoveAsString(move, board.inCheck)}");
        clock.ToggleClock();
        board.gameResult = Judge.GetResult(board);

        if (board.gameResult != Judge.Result.Playing)
        {
            HandleGameOver();
            return;
        }

        if (board.turn == Piece.White)
        {
            whitePlayer.TurnToMove();
        }
        else
        {
            blackPlayer.TurnToMove();
        }

    }

    public void TimedOut(bool isWhite)
    {
        board.gameResult = isWhite ? Judge.Result.WhiteOutOfTime : Judge.Result.BlackOutOfTime;
        HandleGameOver();
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

    private void MoveCamera() => camera.transform.position = new Vector3(3.5f, 3.5f, -10);

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

    private void CreatePlayer(ref Player player, PlayerType type)
    {
        player = type == PlayerType.Human ? new Human() : new Bot();
        player.PlayChosenMove += PlayMove;
    }

    private void StartNewGame(PlayerType whitePlayerType, PlayerType blackPlayerType)
    {   
        board.ResetBoard();
        clock.NewGame();

        CreatePlayer(ref whitePlayer, whitePlayerType);
        CreatePlayer(ref blackPlayer, blackPlayerType);

        UpdateEndOfGameScreen(board.gameResult);
        whitePlayer.TurnToMove();
    }

    public void StartNewGamePlayerVsPlayer() => StartNewGame(PlayerType.Human, PlayerType.Human);

    public void StartNewGamePlayerVsBot() => StartNewGame(PlayerType.Human, PlayerType.Bot);

    public void StartNewGameBotVsBot() => StartNewGame(PlayerType.Bot, PlayerType.Bot);

    public static void UpdateEndOfGameScreen(Judge.Result gameResult)
    {
        if (gameResult == Judge.Result.Playing)
        {
            endOfGameText.text = "";
            resultText.text = "";
        }
        else if (gameResult == Judge.Result.WhiteIsMated)
        {
            endOfGameText.text = "Checkmate";
            resultText.text = "0 - 1";
        }
        else if (gameResult == Judge.Result.BlackIsMated)
        {
            endOfGameText.text = "Checkmate";
            resultText.text = "1 - 0";
        }
        else if (gameResult == Judge.Result.WhiteOutOfTime)
        {
            endOfGameText.text = "White Flag";
            resultText.text = "0 - 1";
        }
        else if (gameResult == Judge.Result.BlackOutOfTime)
        {
            endOfGameText.text = "Black Flag";
            resultText.text = "1 - 0";
        }
        else
        {
            resultText.text = "1/2 - 1/2";
            switch (gameResult)
            {
                case Judge.Result.FiftyMove:
                    endOfGameText.text = "50 move rule";
                    break;
                case Judge.Result.Insufficient:
                    endOfGameText.text = "Insufficient material";
                    break;
                case Judge.Result.Stalemate:
                    endOfGameText.text = "Stalemate";
                    break;
                case Judge.Result.Threefold:
                    endOfGameText.text = "Threefold repetition";
                    break;
                default:
                    endOfGameText.text = "Unidentified";
                    break;
            }
        }
    }

    public void HandleGameOver()
    {
        UpdateEndOfGameScreen(board.gameResult);
        clock.StopClocks();
    }

}
