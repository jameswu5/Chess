using System;
using System.Collections.Generic;

public static class MoveGenerator
{
    
    // Pseudolegal moves

    // hero -> bitboard of pieces of your own piece
    // opponent -> bitboard of pieces of opponent's piece

    public static HashSet<int> GetSlideMoves(int index, int pieceType, ulong hero, ulong opponent, int[] boardState)
    {
        HashSet<int> legalMoves = new();

        if (pieceType == Piece.Rook || pieceType == Piece.Queen)
        {
            // first 4 are orthogonal directions
            for (int i = 0; i < 4; i++)
            {
                int direction = Direction.directions[i];
                ulong rayAttacks = Bitboard.GetRayAttacks(hero, opponent, direction, index);
                List<int> targetIndices = Bitboard.GetIndicesFromBitboard(rayAttacks);
                foreach (int target in targetIndices)
                {
                    legalMoves.Add(Move.Initialise(Move.Standard, index, target, Piece.Rook, Piece.GetPieceType(boardState[target])));
                }
            }
        }
        if (pieceType == Piece.Bishop || pieceType == Piece.Queen)
        {
            // last 4 are diagonal directions
            for (int i = 4; i < 8; i++)
            {
                int direction = Direction.directions[i];
                ulong rayAttacks = Bitboard.GetRayAttacks(hero, opponent, direction, index);
                List<int> targetIndices = Bitboard.GetIndicesFromBitboard(rayAttacks);
                foreach (int target in targetIndices)
                {
                    legalMoves.Add(Move.Initialise(Move.Standard, index, target, Piece.Rook, Piece.GetPieceType(boardState[target])));
                }
            }

        }
        return legalMoves;
    }

    public static HashSet<int> GetKnightMoves(int index, ulong hero, int[] boardState)
    {
        HashSet<int> legalMoves = new();

        ulong knightAttacks = PrecomputedData.KnightAttacks[index] & ~hero;

        List<int> targetIndices = Bitboard.GetIndicesFromBitboard(knightAttacks);
        foreach (int target in targetIndices)
        {
            legalMoves.Add(Move.Initialise(Move.Standard, index, target, Piece.Knight, Piece.GetPieceType(boardState[target])));
        }

        return legalMoves;
    }
}
