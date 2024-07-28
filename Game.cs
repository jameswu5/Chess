
namespace Chess;

public class Game
{
    public Core.Board board;
    public UI.UI ui;

    public Player.Player whitePlayer;
    public Player.Player blackPlayer;

    public Game()
    {
        board = new();
        ui = new();
        ui.CreateUI(board.boardState);

        whitePlayer = new Player.Human(this);
        blackPlayer = new Player.Human(this);

        CreatePlayer(ref whitePlayer, Player.Player.Type.Human);
        CreatePlayer(ref blackPlayer, Player.Player.Type.Human);
    }

    public void Update()
    {
        ui.Display();

        // assume the game is still playing
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
        // obviously this isn't the full version
        board.MakeMove(move);
        ui.MakeMove(move);
        Core.Move.DisplayMoveInformation(move);
    }
}