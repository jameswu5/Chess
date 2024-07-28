using static Chess.Core.Piece;

namespace Chess.UI;

public static class Piece
{
    public static readonly Image[] PieceImages;

    static Piece()
    {
        PieceImages = GetPieceImages();
    }

    private static Image[] GetPieceImages()
    {
        Image[] images = new Image[16];

        images[King   | White] = new Image("Images/WhiteKing.png"  , Settings.Square.Size, Settings.Square.Size);
        images[Queen  | White] = new Image("Images/WhiteQueen.png" , Settings.Square.Size, Settings.Square.Size);
        images[Bishop | White] = new Image("Images/WhiteBishop.png", Settings.Square.Size, Settings.Square.Size);
        images[Knight | White] = new Image("Images/WhiteKnight.png", Settings.Square.Size, Settings.Square.Size);
        images[Rook   | White] = new Image("Images/WhiteRook.png"  , Settings.Square.Size, Settings.Square.Size);
        images[Pawn   | White] = new Image("Images/WhitePawn.png"  , Settings.Square.Size, Settings.Square.Size);
        images[King   | Black] = new Image("Images/BlackKing.png"  , Settings.Square.Size, Settings.Square.Size);
        images[Queen  | Black] = new Image("Images/BlackQueen.png" , Settings.Square.Size, Settings.Square.Size);
        images[Bishop | Black] = new Image("Images/BlackBishop.png", Settings.Square.Size, Settings.Square.Size);
        images[Knight | Black] = new Image("Images/BlackKnight.png", Settings.Square.Size, Settings.Square.Size);
        images[Rook   | Black] = new Image("Images/BlackRook.png"  , Settings.Square.Size, Settings.Square.Size);
        images[Pawn   | Black] = new Image("Images/BlackPawn.png"  , Settings.Square.Size, Settings.Square.Size);

        return images;
    }

    public static void DrawOnSquare(int piece, int index)
    {
        (int x, int y) = Square.GetSquareDisplayCoords(index);
        PieceImages[piece].Draw(x, y);
    }

    public static void Draw(int piece, int posX, int posY)
    {
        PieceImages[piece].Draw(posX, posY);
    }
}