using System;
using System.Collections;
using System.Collections.Generic;

public static class Zobrist
{
    // Hash keys for every piece in each square
    private static ulong[,] pieceKeys = new ulong[15, 64];

    // Hash keys for every possible castling right state
    private static ulong[] castlingKeys = new ulong[16];

    // Hash keys for every possible en passant file (or lack thereof)
    private static ulong[] enPassantKeys = new ulong[9];

    // Hash key for the turn
    private static ulong turnKey;

    static Zobrist()
    {
        // Random number generator with random seed
        Random rng = new Random(Guid.NewGuid().GetHashCode());

        foreach (int piece in Piece.pieces)
        {
            for (int sq = 0; sq < 64; sq++)
            {
                pieceKeys[piece, sq] = GetRandomUlong(rng);
            }
        }

        for (int i = 0; i < 16; i++)
        {
            castlingKeys[i] = GetRandomUlong(rng);
        }

        for (int i = 0; i < 9; i++)
        {
            enPassantKeys[i] = GetRandomUlong(rng);
        }

        turnKey = GetRandomUlong(rng);
    }


    public static ulong CalculateKey(Board board)
    {
        ulong key = 0;

        for (int i = 0; i < 64; i++)
        {
            int piece = board.GetPieceTypeAtIndex(i);
            if (piece != Piece.None)
            {
                key ^= pieceKeys[piece, i];
            }
        }

        key ^= castlingKeys[board.castlingRights];

        int file = board.enPassantTarget == -1 ? 0 : Square.GetFile(board.enPassantTarget);
        key ^= enPassantKeys[file];

        if (board.turn == Piece.White)
        {
            key ^= turnKey;
        }

        return key;
    }


    private static ulong GetRandomUlong(Random rng)
    {
        byte[] bytes = new byte[8];
        rng.NextBytes(bytes);
        ulong res = 0;

        for (int i = 0; i < 7; i++)
        {
            res |= bytes[i];
            res <<= 8;
        }
        res |= bytes[7];

        return res;
    }
}
