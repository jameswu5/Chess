using System.Collections;
using System.Collections.Generic;

public static class Evaluator
{

    public static Dictionary<int, int> PieceValues = new()
    {
        { Piece.Pawn, 100 },
        { Piece.Knight, 310 },
        { Piece.Bishop, 320 },
        { Piece.Rook, 500 },
        { Piece.Queen, 900 },
        { Piece.King, 20000 }
    };

    public static int[] PawnSquareTable =
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

    public static int[] KnightSquareTable =
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

    public static int[] BishopSquareTable =
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

    public static int[] RookSquareTable =
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

    public static int[] QueenSquareTable =
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

    public static int[] KingMiddlegameSquareTable =
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

    public static int[] KingEndgameSquareTable =
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
        { Piece.Pawn, PawnSquareTable },
        { Piece.Knight, KnightSquareTable },
        { Piece.Bishop, BishopSquareTable },
        { Piece.Rook, RookSquareTable },
        { Piece.Queen, QueenSquareTable },
        { Piece.King, KingMiddlegameSquareTable }
    };


    public static int EvaluateBoard(Board board)
    {
        int whiteMaterial = CountMaterialAndPosition(board, Piece.White);
        int blackMaterial = CountMaterialAndPosition(board, Piece.Black);

        return whiteMaterial - blackMaterial;
    }

    public static int CountMaterialAndPosition(Board board, int colour)
    {
        int totalValue = 0;

        for (int i = 0; i < 64; i++)
        {
            Piece piece = board.GetPieceAtIndex(i);
            if (piece != null && board.CheckIfPieceIsColour(i, colour))
            {
                totalValue += PieceValues[piece.GetPieceType()];
                int index = colour == Piece.White ? i : ConvertBlackIndexToWhite(i);
                totalValue += SquareTables[piece.GetPieceType()][index];
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
