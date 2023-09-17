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
    public const int increment = 2;

    private void Start()
    {
        MoveCamera();
        GetSounds();
        GetTexts();
        CreateBoard();

        clock = Instantiate(clockPrefab);
        clock.Initialise(allowedTime, increment);

        StartNewGamePlayerVsPlayer();
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

    public void PlayMove(int move)
    {
        board.MakeMove(move, true);
        PlayMoveSound(Move.IsCaptureMove(move));
        Debug.Log($"{board.moveNumber}: {Move.GetMoveAsString(move, board.inCheck)}");
        clock.ToggleClock();
        board.gameResult = board.GetGameResult();
        UpdateEndOfGameScreen(board.gameResult, board.turn);
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
    }

    public void StartNewGamePlayerVsPlayer() => StartNewGame(PlayerType.Human, PlayerType.Human);

    public void StartNewGamePlayerVsBot() => StartNewGame(PlayerType.Human, PlayerType.Bot);

    public void StartNewGameBotVsBot() => StartNewGame(PlayerType.Bot, PlayerType.Bot);

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
