
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

    }

    public void Update()
    {
        ui.Display();

        if (board.gameResult != Core.Judge.Result.Playing) return;

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
            // HandleGameOver();
            Console.WriteLine("Game over");
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
        Console.WriteLine("Game over");
        // HandleGameOver();
    }
}