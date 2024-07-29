
namespace Chess.Game;

public class Game
{
    public Core.Board board;
    public UI.UI ui;

    public Player.Player whitePlayer;
    public Player.Player blackPlayer;

    public Clock clock;
    public const int allowedTime = 300;
    public const int increment = 0;

    public Match match;

    public Game()
    {
        board = new();
        ui = new(board);
        ui.CreateUI(board.boardState);

        whitePlayer = new Player.Human(this);
        blackPlayer = new Player.Human(this);

        CreatePlayer(ref whitePlayer, Player.Player.Type.Human);
        CreatePlayer(ref blackPlayer, Player.Player.Type.Human);

        clock = new();
        clock.Initialise(allowedTime, increment);
        clock.ClockTimedOut += TimedOut;

        match = new Match();
        match.StartGame += StartMatchGame;
    }

    public void Update()
    {
        ui.Display();

        if (board.gameResult != Core.Judge.Result.Playing) return;

        clock.Update();

        if (board.turn == Core.Piece.White)
        {
            whitePlayer.Update();
        }
        else
        {
            blackPlayer.Update();
        }
    }

    private void CreatePlayer(ref Player.Player player, Player.Player.Type type)
    {
        // need to change when bots are added
        if (type == Player.Player.Type.Human)
        {
            player = new Player.Human(this);
        }

        player.PlayChosenMove += PlayMove;
    }

    public void PlayMove(int move)
    {
        ui.MakeMove(move);
        board.MakeMove(move);
        // PlayMoveSound(Move.IsCaptureMove(move));

        clock.ToggleClock();
        board.gameResult = Core.Judge.GetResult(board);

        if (board.gameResult != Core.Judge.Result.Playing)
        {
            HandleGameOver();
            return;
        }

        if (board.turn == Core.Piece.White)
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
        board.gameResult = isWhite ? Core.Judge.Result.WhiteOutOfTime : Core.Judge.Result.BlackOutOfTime;
        HandleGameOver();
    }

    public void HandleGameOver()
    {
        if (match.isActive)
        {
            if (board.gameResult == Core.Judge.Result.BlackIsMated || board.gameResult == Core.Judge.Result.BlackOutOfTime)
            {
                match.ReportResult(Match.GameResult.WhiteWins);
            }
            else if (board.gameResult == Core.Judge.Result.WhiteIsMated || board.gameResult == Core.Judge.Result.WhiteOutOfTime)
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

    public static void UpdateEndOfGameScreen(Core.Judge.Result gameResult)
    {
        Console.WriteLine(gameResult);

        // if (gameResult == Core.Judge.Result.Playing)
        // {
        //     endOfGameText.text = "";
        //     resultText.text = "";
        // }
        // else if (gameResult == Core.Judge.Result.WhiteIsMated)
        // {
        //     endOfGameText.text = "Checkmate";
        //     resultText.text = "0 - 1";
        // }
        // else if (gameResult == Core.Judge.Result.BlackIsMated)
        // {
        //     endOfGameText.text = "Checkmate";
        //     resultText.text = "1 - 0";
        // }
        // else if (gameResult == Core.Judge.Result.WhiteOutOfTime)
        // {
        //     endOfGameText.text = "White Flag";
        //     resultText.text = "0 - 1";
        // }
        // else if (gameResult == Core.Judge.Result.BlackOutOfTime)
        // {
        //     endOfGameText.text = "Black Flag";
        //     resultText.text = "1 - 0";
        // }
        // else
        // {
        //     resultText.text = "1/2 - 1/2";
        //     switch (gameResult)
        //     {
        //         case Core.Judge.Result.FiftyMove:
        //             endOfGameText.text = "50 move rule";
        //             break;
        //         case Core.Judge.Result.Insufficient:
        //             endOfGameText.text = "Insufficient material";
        //             break;
        //         case Core.Judge.Result.Stalemate:
        //             endOfGameText.text = "Stalemate";
        //             break;
        //         case Core.Judge.Result.Threefold:
        //             endOfGameText.text = "Threefold repetition";
        //             break;
        //         default:
        //             endOfGameText.text = "Unidentified";
        //             break;
        //     }
        // }
    }


    // Match games

    private void StartNewGame(Player.Player.Type whitePlayerType, Player.Player.Type blackPlayerType)
    {
        board.Initialise();
        ui.Reset();
        ui.CreateUI(board.boardState);

        clock.NewGame();

        CreatePlayer(ref whitePlayer, whitePlayerType);
        CreatePlayer(ref blackPlayer, blackPlayerType);

        UpdateEndOfGameScreen(board.gameResult);

        if (board.turn == Core.Piece.White)
        {
            whitePlayer.TurnToMove(clock.white);
        }
        else
        {
            blackPlayer.TurnToMove(clock.black);
        }
    }

    // Starts a match between two bots
    public void StartMatch(Player.Player.Type bot1, Player.Player.Type bot2)
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
}