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

    public static HashSet<int> GetKingMoves(int index, ulong hero, bool[] castlingRights, int[] boardState)
    {
        HashSet<int> legalMoves = new();
        ulong kingAttacks = PrecomputedData.KingAttacks[index] & ~hero;
        bool pieceIsWhite = Piece.IsWhite(boardState[index]);

        List<int> targetIndices = Bitboard.GetIndicesFromBitboard(kingAttacks);
        foreach (int target in targetIndices)
        {
            legalMoves.Add(Move.Initialise(Move.Standard, index, target, Piece.King, Piece.GetPieceType(boardState[target])));
        }

        // Castling

        if (pieceIsWhite && index == Square.e1) // king is in original position
        {
            if (castlingRights[0] == true && boardState[Square.h1] == Piece.White + Piece.Rook
                && boardState[Square.f1] == Piece.None && boardState[Square.g1] == Piece.None)
            {
                // can castle kingside
                legalMoves.Add(Move.Initialise(Move.Castling, index, index + 2, Piece.King, Piece.None));
            }
            if (castlingRights[1] == true && boardState[Square.a1] == Piece.White + Piece.Rook
                && boardState[Square.b1] == Piece.None && boardState[Square.c1] == Piece.None && boardState[Square.d1] == Piece.None)
            {
                // can castle queenside
                legalMoves.Add(Move.Initialise(Move.Castling, index, index - 2, Piece.King, Piece.None));

            }
        }
        else if (!pieceIsWhite && index == 60)
        {
            if (castlingRights[2] == true && boardState[Square.h8] == Piece.Black + Piece.Rook
                && boardState[Square.f8] == Piece.None && boardState[Square.g8] == Piece.None)
            {
                legalMoves.Add(Move.Initialise(Move.Castling, index, index + 2, Piece.King, Piece.None));

            }
            if (castlingRights[3] == true && boardState[Square.a8] == Piece.Black + Piece.Rook
                && boardState[Square.b8] == Piece.None && boardState[Square.c8] == Piece.None && boardState[Square.d8] == Piece.None)
            {
                legalMoves.Add(Move.Initialise(Move.Castling, index, index - 2, Piece.King, Piece.None));
            }
        }

        return legalMoves;
    }
}
