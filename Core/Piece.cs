
namespace Chess.Core;

public static class Piece
{
    public const int None = 0;
    public const int King = 1;
    public const int Queen = 2;
    public const int Bishop = 3;
    public const int Knight = 4;
    public const int Rook = 5;
    public const int Pawn = 6;

    public const int White = 0;
    public const int Black = 8;

    public static readonly Dictionary<int, string> pieceDictionary = new()
    {
        { White + King   , "K" },
        { White + Queen  , "Q" },
        { White + Bishop , "B" },
        { White + Knight , "N" },
        { White + Rook   , "R" },
        { White + Pawn   , "P" },
        { Black + King   , "k" },
        { Black + Queen  , "q" },
        { Black + Bishop , "b" },
        { Black + Knight , "n" },
        { Black + Rook   , "r" },
        { Black + Pawn   , "p" }
    };

    public static readonly Dictionary<char, int> pieceTypes = new()
    {
        {'K', King},
        {'Q', Queen},
        {'B', Bishop},
        {'N', Knight},
        {'R', Rook},
        {'P', Pawn},
    };

    public static readonly int[] pieces = { White + King, White + Queen, White + Bishop, White + Knight, White + Rook, White + Pawn, Black + King, Black + Queen, Black + Bishop, Black + Knight, Black + Rook, Black + Pawn };

    public static int GetPieceType(int pieceID) => pieceID & 0b111;

    public static bool IsColour(int pieceID, int colour) => (pieceID < 8 && colour == White) || (pieceID >= 8 && colour == Black);

    public static string GetCharacterFromPieceType(int pieceID) => pieceDictionary[pieceID];
}