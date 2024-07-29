
namespace Chess.Search;

public static class Evaluator
{
    public static Dictionary<int, int> PieceValues = new()
    {
        { Core.Piece.Pawn, 100 },
        { Core.Piece.Knight, 310 },
        { Core.Piece.Bishop, 320 },
        { Core.Piece.Rook, 500 },
        { Core.Piece.Queen, 900 },
        { Core.Piece.King, 20000 }
    };

    public static readonly int[] PawnSquareTable =
    {
        0,  0,  0,  0,  0,  0,  0,  0,
        5, 10, 10,-20,-20, 10, 10,  5,
        5, -5,-10,  0,  0,-10, -5,  5,
        0,  0,  0, 20, 20,  0,  0,  0,
        5,  5, 10, 25, 25, 10,  5,  5,
        10, 10, 20, 30, 30, 20, 10, 10,
        50, 50, 50, 50, 50, 50, 50, 50,
        0,  0,  0,  0,  0,  0,  0,  0
    };

    public static readonly int[] KnightSquareTable =
    {
        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20,  0,  5,  5,  0,-20,-40,
        -30,  5, 10, 15, 15, 10,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5, 15, 20, 20, 15,  5,-30,
        -30,  0, 10, 15, 15, 10,  0,-30,
        -40,-20,  0,  0,  0,  0,-20,-40,
        -50,-40,-30,-30,-30,-30,-40,-50
    };

    public static readonly int[] BishopSquareTable =
    {
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  5,  0,  0,  0,  0,  5,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -20,-10,-10,-10,-10,-10,-10,-20
    };

    public static readonly int[] RookSquareTable =
    {
         0,  0,  0,  5,  5,  0,  0,  0,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
         5, 10, 10, 10, 10, 10, 10,  5,
         0,  0,  0,  0,  0,  0,  0,  0
    };

    public static readonly int[] QueenSquareTable =
    {
        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  5,  0,  0,  0,  0,-10,
        -10,  5,  5,  5,  5,  5,  0,-10,
          0,  0,  5,  5,  5,  5,  0, -5,
         -5,  0,  5,  5,  5,  5,  0, -5,
        -10,  0,  5,  5,  5,  5,  0,-10,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20
    };

    public static readonly int[] KingMiddlegameSquareTable =
    {
         20, 30, 10,  0,  0, 10, 30, 20,
         20, 20,  0,  0,  0,  0, 20, 20,
        -20,-30,-30,-40,-40,-30,-30,-20,
        -10,-20,-20,-20,-20,-20,-20,-10,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30
    };

    public static readonly int[] KingEndgameSquareTable =
    {
        -50,-30,-30,-30,-30,-30,-30,-50,
        -30,-30,  0,  0,  0,  0,-30,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-20,-10,  0,  0,-10,-20,-30,
        -50,-40,-30,-20,-20,-30,-40,-50,
    };

    public static Dictionary<int, int[]> SquareTables = new() // we're ignoring endgames for now
    {
        { Core.Piece.Pawn, PawnSquareTable },
        { Core.Piece.Knight, KnightSquareTable },
        { Core.Piece.Bishop, BishopSquareTable },
        { Core.Piece.Rook, RookSquareTable },
        { Core.Piece.Queen, QueenSquareTable },
        { Core.Piece.King, KingMiddlegameSquareTable }
    };


    public static int EvaluateBoard(Core.Board board)
    {
        int whiteMaterial = CountMaterialAndPosition(board, Core.Piece.White);
        int blackMaterial = CountMaterialAndPosition(board, Core.Piece.Black);

        return whiteMaterial - blackMaterial;
    }

    public static int CountMaterialAndPosition(Core.Board board, int colour)
    {
        int totalValue = 0;

        for (int i = 0; i < 64; i++)
        {
            int piece = board.GetPieceAtIndex(i);
            if (piece != Core.Piece.None && board.CheckIfPieceIsColour(i, colour))
            {
                totalValue += PieceValues[Core.Piece.GetPieceType(piece)];
                int index = colour == Core.Piece.White ? i : ConvertBlackIndexToWhite(i);
                totalValue += SquareTables[Core.Piece.GetPieceType(piece)][index];
            }
        }

        return totalValue;
    }

    public static int ConvertBlackIndexToWhite(int index)
    {
        int rank = index / 8;
        int file = index % 8;
        return (7 - rank) * 8 + file;
    }
}