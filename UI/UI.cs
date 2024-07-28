
namespace Chess.UI;

public class UI
{
    private Square[] squares;
    private Piece[] pieces;

    public UI()
    {
        squares = new Square[64];
        pieces = new Piece[64];

        for (int i = 0; i < 64; i++)
        {
            squares[i] = new Square(i);
        }
    }

    public void CreateUI(int[] boardState)
    {
        pieces = new Piece[64];

        for (int i = 0; i < 64; i++)
        {
            Console.WriteLine($"{i}: {boardState[i]}");
            if (boardState[i] != Core.Piece.None)
            {
                pieces[i] = new Piece(boardState[i], i);
            }
        }
    }

    public void Display()
    {
        for (int i = 0; i < 64; i++)
        {
            squares[i].Display();
            if (pieces[i] != null)
            {
                pieces[i].Draw();
            }
        }
    }
}