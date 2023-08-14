using System;
using System.Collections.Generic;

using UnityEngine;

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


    public static HashSet<int> GetPawnMoves(int index, int colourIndex, ulong hero, ulong opponent, int[] boardState, int enPassantTarget)
    {
        HashSet<int> legalMoves = new();

        // pushes
        ulong pawnPushes = PrecomputedData.PawnPushes[colourIndex][index] & ~hero & ~opponent;

        bool promote = (pawnPushes & (Bitboard.Rank1 | Bitboard.Rank8)) > 0;

        List<int> pushIndices = Bitboard.GetIndicesFromBitboard(pawnPushes);
        foreach (int target in pushIndices)
        {
            if (Math.Abs(target - index) == 16)
            {
                legalMoves.Add(Move.Initialise(Move.PawnTwoSquares, index, target, Piece.Pawn, Piece.None));
            }
            else if (promote)
            {
                legalMoves.Add(Move.Initialise(Move.PromoteToBishop, index, target, Piece.Pawn, Piece.None));
                legalMoves.Add(Move.Initialise(Move.PromoteToKnight, index, target, Piece.Pawn, Piece.None));
                legalMoves.Add(Move.Initialise(Move.PromoteToQueen, index, target, Piece.Pawn, Piece.None));
                legalMoves.Add(Move.Initialise(Move.PromoteToRook, index, target, Piece.Pawn, Piece.None));
            }
            else
            {
                legalMoves.Add(Move.Initialise(Move.Standard, index, target, Piece.Pawn, Piece.None));
            }
        }

        // captures
        ulong pawnAttacks = PrecomputedData.PawnAttacks[colourIndex][index] & opponent;

        List<int> targetIndices = Bitboard.GetIndicesFromBitboard(pawnAttacks);
        foreach (int target in targetIndices)
        {

            // Problem:
            // If I include the en passant code further down, boardState[target] can hold Piece.None and still make it to this
            // part of the code. As a result you can move a pawn diagonally without capturing. I am suspicious this is because I make every move
            // possible to check if the king is attacked.
            
            // Here is a plaster solution where I just enforce that there must be a piece at that square. Hopefully when I rewrite
            // legal move generation this if statement (*) can be removed

            if (boardState[target] != Piece.None) // (*)
            {

                if (promote)
                {
                    legalMoves.Add(Move.Initialise(Move.PromoteToBishop, index, target, Piece.Pawn, Piece.GetPieceType(boardState[target])));
                    legalMoves.Add(Move.Initialise(Move.PromoteToKnight, index, target, Piece.Pawn, Piece.GetPieceType(boardState[target])));
                    legalMoves.Add(Move.Initialise(Move.PromoteToQueen, index, target, Piece.Pawn, Piece.GetPieceType(boardState[target])));
                    legalMoves.Add(Move.Initialise(Move.PromoteToRook, index, target, Piece.Pawn, Piece.GetPieceType(boardState[target])));
                }
                else
                {
                    legalMoves.Add(Move.Initialise(Move.Standard, index, target, Piece.Pawn, Piece.GetPieceType(boardState[target])));

                    if (Piece.GetPieceType(boardState[target]) == Piece.None)
                    {
                        Debug.Log(Square.ConvertIndexToSquareName(target));
                    }
                }

            }

        }

        // en passant

        if (enPassantTarget != -1 && (PrecomputedData.PawnAttacks[colourIndex][index] & (1ul << enPassantTarget)) > 0)
        {
            legalMoves.Add(Move.Initialise(Move.EnPassant, index, enPassantTarget, Piece.Pawn, Piece.Pawn));
        }

        return legalMoves;
    }
}
