
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

        whitePlayer = new Player.Human(board);
        blackPlayer = new Player.Human(board);
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
}