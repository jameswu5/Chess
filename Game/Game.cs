
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
        whitePlayer.DisplayName(true);
        blackPlayer.DisplayName(false);

        if (board.gameResult != Core.Judge.Result.Playing) return;

        whitePlayer.Update();
        blackPlayer.Update();

        clock.Update();
    }

    private void CreatePlayer(ref Player.Player player, Player.Player.Type type)
    {
        // need to change when bots are added
        if (type == Player.Player.Type.Human)
        {
            player = new Player.Human(this);
        }
        else
        {
            player = Player.Bot.GetBotFromBotType(type, this);
        }

        player.PlayChosenMove += PlayMove;
    }

    public void PlayMove(int move)
    {
        board.MakeMove(move);
        ui.MakeMove(move);
        Audio.Audio.PlaySound(Core.Move.IsCaptureMove(move));

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
            clock.StopClocks();
        }
    }

    // Match games

    public void StartNewGame(Player.Player.Type whitePlayerType, Player.Player.Type blackPlayerType)
    {
        Console.WriteLine($"{whitePlayerType} vs {blackPlayerType}");
        board.Initialise();
        ui.Reset();
        ui.CreateUI(board.boardState);

        clock.NewGame();

        CreatePlayer(ref whitePlayer, whitePlayerType);
        CreatePlayer(ref blackPlayer, blackPlayerType);

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