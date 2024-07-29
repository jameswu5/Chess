using Chess.Core;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Chess.UI;

public class UI
{
    private Square[] squares;
    private Square[] promotionSquares;

    private Piece[] pieces;
    private Piece[] promotionPieces;

    private Board board;

    public int inPromotionScreen;

    public UI(Board board)
    {
        squares = new Square[64];
        promotionSquares = new Square[4];    

        pieces = new Piece[64];
        promotionPieces = new Piece[4];

        this.board = board;

        inPromotionScreen = -1;

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

        if (inPromotionScreen != -1)
        {
            // make board darker
            DrawRectangle(Settings.Board.HorOffset, Settings.Board.VerOffset, Settings.Board.Size, Settings.Board.Size, Settings.Board.CoverColour);

            for (int i = 0; i < 4; i++)
            {
                promotionSquares[i].Display();
                promotionPieces[i].Draw();
            }
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

    public void ResetPiecePosition(int index, bool promotionScreenRequirement)
    {
        if (promotionScreenRequirement && inPromotionScreen == -1)
        {
            (int x, int y) = Square.GetSquareDisplayCoords(index);
            pieces[index].SetPosition(x, y);
        }
    }

    public void MakeMove(int move)
    {
        int startIndex = Move.GetStartIndex(move);
        int endIndex = Move.GetEndIndex(move);
        int moveType = Move.GetMoveType(move);

        bool isWhite = board.CheckPieceIsWhite(startIndex);
        int colour = board.GetColour(isWhite);

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


    public void EnablePromotionScreen(int index)
    {
        // make the board darker

        // SetBoardCover(true);

        int colourMultiplier = Core.Square.GetRank(index) == 1 ? 1 : -1;
        int pieceColour = Core.Square.GetRank(index) == 1 ? Core.Piece.Black : Core.Piece.White;

        // create the pieces

        promotionPieces[0] = new Piece(Core.Piece.Queen + pieceColour, index);
        promotionPieces[1] = new Piece(Core.Piece.Rook + pieceColour, index + 8 * colourMultiplier);
        promotionPieces[2] = new Piece(Core.Piece.Bishop + pieceColour, index + 16 * colourMultiplier);
        promotionPieces[3] = new Piece(Core.Piece.Knight + pieceColour, index + 24 * colourMultiplier);

        // create the squares
        for (int i = 0; i < 4; i++)
        {
            promotionSquares[i] = new Square(index + 8 * i * colourMultiplier);
        }

        inPromotionScreen = index;
    }

    public void DisablePromotionScreen()
    {
        inPromotionScreen = -1;
    }

    public void Reset()
    {
        Array.Clear(squares, 0, squares.Length);
        Array.Clear(pieces, 0, pieces.Length);

        Array.Clear(promotionSquares, 0, promotionSquares.Length);
        Array.Clear(promotionPieces, 0, promotionPieces.Length);
    }
}