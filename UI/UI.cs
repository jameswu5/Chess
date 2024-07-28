using Chess.Core;

namespace Chess.UI;

public class UI
{
    private Square[] squares;
    private Piece[] pieces;
    private Board board;

    public UI(Board board)
    {
        squares = new Square[64];
        pieces = new Piece[64];
        this.board = board;

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
                CreatePiece(boardState[i], i);
            }
        }
    }

    private void CreatePiece(int pieceID, int index)
    {
        pieces[index] = new Piece(pieceID, index);
    }

    private void DestroyPiece(int index)
    {
        pieces[index] = null;
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
        int startIndex = Move.GetStartIndex(move);
        int endIndex = Move.GetEndIndex(move);
        int moveType = Move.GetMoveType(move);

        bool isWhite = board.CheckPieceIsWhite(startIndex);
        int colour = board.GetColour(isWhite);
        // int opponentColour = GetOpponentColour(isWhite);

        // Uncolour the checked king if necessary
        // if (inCheck)
        // {
        //     boardUI.ResetSquareColour(kingIndices[GetColourIndex(turn)]);
        // }

        if (moveType == Move.Standard || moveType == Move.PawnTwoSquares)
        {
            MovePieceToSquare(startIndex, endIndex);
        }

        else if (moveType == Move.Castling)
        {
            // move the king
            MovePieceToSquare(startIndex, endIndex);

            // move the rook
            if (endIndex > startIndex) // kingside
            {
                MovePieceToSquare(startIndex + 3, startIndex + 1);
            }
            else // queenside
            {
                MovePieceToSquare(startIndex - 4, startIndex - 1);
            }
        }

        else if (moveType == Move.EnPassant)
        {
            // move the pawn
            MovePieceToSquare(startIndex, endIndex);

            // destroy the piece next to it
            DestroyPiece(isWhite ? endIndex - 8 : endIndex + 8);
        }

        else if (moveType == Move.PromoteToQueen || moveType == Move.PromoteToRook || moveType == Move.PromoteToBishop || moveType == Move.PromoteToKnight)
        {
            MovePieceToSquare(startIndex, endIndex);
            DestroyPiece(endIndex);

            int promotePiece = -1;

            switch (moveType)
            {
                case Move.PromoteToQueen:
                    promotePiece = Core.Piece.Queen;
                    break;
                case Move.PromoteToRook:
                    promotePiece = Core.Piece.Rook;
                    break;
                case Move.PromoteToBishop:
                    promotePiece = Core.Piece.Bishop;
                    break;
                case Move.PromoteToKnight:
                    promotePiece = Core.Piece.Knight;
                    break;
                default:
                    break;
            }

            CreatePiece(promotePiece + colour, endIndex);
        }

        // if (inCheck)
        // {
        //     boardUI.HighlightCheck(kingIndices[GetColourIndex(turn)]);
        // }
    }

}