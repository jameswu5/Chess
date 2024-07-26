using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public Board boardPrefab;
    public Board board;

    public Player whitePlayer;
    public Player blackPlayer;

    public static Text whitePlayerText;
    public static Text blackPlayerText;

    private const Player.Type defaultBot = Player.Type.Version2;

    public new Camera camera;
    public static AudioSource captureSound;
    public static AudioSource moveSound;
    
    public static Text endOfGameText;
    public static Text resultText;

    public Clock clockPrefab;
    public Clock clock;
    public const int allowedTime = 300;
    public const int increment = 0;

    public Match match;

    private void Start()
    {
        MoveCamera();
        GetSounds();
        GetTexts();
        CreateBoard();

        clock = Instantiate(clockPrefab);
        clock.Initialise(allowedTime, increment);
        clock.ClockTimedOut += TimedOut;

        match = new Match();
        match.StartGame += StartMatchGame;

        StartNewGamePlayerVsPlayer();
        //StartMatch(Player.Type.Random, Player.Type.Version1);
        //StartMatch(Player.Type.Version1, Player.Type.Version2);
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
        //Debug.Log($"{board.moveNumber}: {Move.GetMoveAsString(move, board.inCheck)}");
        clock.ToggleClock();
        board.gameResult = Judge.GetResult(board);

        if (board.gameResult != Judge.Result.Playing)
        {
            HandleGameOver();
            return;
        }

        if (board.turn == Piece.White)
        {
            whitePlayer.TurnToMove(clock.white);
        }
        else
        {
            blackPlayer.TurnToMove(clock.black);
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
        whitePlayerText = GameObject.FindGameObjectWithTag("WhitePlayer").GetComponent<Text>();
        blackPlayerText = GameObject.FindGameObjectWithTag("BlackPlayer").GetComponent<Text>();
    }

    private void CreateBoard()
    {
        board = Instantiate(boardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        board.Initialise();
    }

    private void CreatePlayer(ref Player player, Player.Type type)
    {
        if (type == Player.Type.Human)
        {
            player = new Human();
        }
        else
        {
            player = Bot.GetBotFromBotType(type);
        }

        player.PlayChosenMove += PlayMove;
    }

    private void StartNewGame(Player.Type whitePlayerType, Player.Type blackPlayerType)
    {
        board.ResetBoard();
        clock.NewGame();

        CreatePlayer(ref whitePlayer, whitePlayerType);
        CreatePlayer(ref blackPlayer, blackPlayerType);

        whitePlayerText.text = whitePlayer.ToString();
        blackPlayerText.text = blackPlayer.ToString();

        UpdateEndOfGameScreen(board.gameResult);

        if (board.turn == Piece.White)
        {
            whitePlayer.TurnToMove(clock.white);
        }
        else
        {
            blackPlayer.TurnToMove(clock.black);
        }
    }
    public void StartNewGamePlayerVsPlayer() => StartNewGame(Player.Type.Human, Player.Type.Human);

    public void StartNewGamePlayerVsBot() => StartNewGame(Player.Type.Human, defaultBot);

    public void StartNewGameBotVsBot() => StartNewGame(defaultBot, defaultBot);

    // Starts a match between two bots
    public void StartMatch(Player.Type bot1, Player.Type bot2)
    {
        match.SetBots(bot1, bot2);
        match.StartMatch();
    }

    public void StartMatchGame()
    {
        // Player1 is white if gameNumber is even
        if (match.gameNumber % 2 == 0)
        {
            StartNewGame(match.player1, match.player2);
        }
        else
        {
            StartNewGame(match.player2, match.player1);
        }
    }

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
        if (match.isActive)
        {
            if (board.gameResult == Judge.Result.BlackIsMated || board.gameResult == Judge.Result.BlackOutOfTime)
            {
                match.ReportResult(Match.GameResult.WhiteWins);
            }
            else if (board.gameResult == Judge.Result.WhiteIsMated || board.gameResult == Judge.Result.WhiteOutOfTime)
            {
                match.ReportResult(Match.GameResult.BlackWins);
            }
            else
            {
                match.ReportResult(Match.GameResult.Draw);
            }
        }
        else
        {
            UpdateEndOfGameScreen(board.gameResult);
            clock.StopClocks();
        }
    }
}
