
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
        }

        // Need to do new loop so pieces always display above
        for (int i = 0; i < 64; i++)
        {
            pieces[i]?.Draw();
        }
    }

    public void HighlightSquare(int index) => squares[index].Highlight();

    public void HighlightOptions(IEnumerable<int> moves)
    {
        foreach (int move in moves)
        {
            squares[Core.Move.GetEndIndex(move)].SetOptionHighlight(true);
        }
    }

    public void HighlightHover(int index)
    {
        // We unhighlight every single square because we don't know which square it was on before.
        // Still technically O(1) but I'm not a fan.

        for (int i = 0; i < 64; i++)
        {
            UnHighlightHover(i);
        }

        squares[index].SetHoverHighlight(true);
    }

    public void UnHighlightHover(int index) => squares[index].SetHoverHighlight(false);

    public void ResetSquareColour(int index) => squares[index].InitialiseColor();

    public void UnHighlightOptionsAllSquares()
    {
        foreach (Square square in squares)
        {
            square.SetOptionHighlight(false);
        }
    }

    public void DragPiece(int index, int mouseX, int mouseY)
    {
        // adjust so that the centre of the piece is at the mouse
        mouseX -= Settings.Square.Size / 2;
        mouseY -= Settings.Square.Size / 2;
        
        pieces[index].SetPosition(mouseX, mouseY);
    }

    public void MovePieceToSquare(int startIndex, int newIndex)
    {
        (int x, int y) = Square.GetSquareDisplayCoords(newIndex);

        pieces[startIndex].SetPosition(x, y);

        // update position of the piece
        pieces[newIndex] = pieces[startIndex];

        if (newIndex != startIndex)
        {
            pieces[startIndex] = null;
        }
    }

    public void MakeMove(int move)
    {
        int startIndex = Core.Move.GetStartIndex(move);
        int endIndex = Core.Move.GetEndIndex(move);

        MovePieceToSquare(startIndex, endIndex);
    }
}