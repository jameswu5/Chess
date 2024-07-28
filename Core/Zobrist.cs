
namespace Chess.Core;

public static class Zobrist
{
    // Hash keys for every piece in each square
    private static readonly ulong[,] pieceKeys = new ulong[15, 64];

    // Hash keys for every possible castling right state
    private static readonly ulong[] castlingKeys = new ulong[16];

    // Hash keys for every possible en passant file (or lack thereof)
    private static readonly ulong[] enPassantKeys = new ulong[9];
    private static int GetTargetFile(int target) => target == -1 ? 0 : Square.GetFile(target);

    // Hash key for the turn
    private static readonly ulong turnKey;

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
            int piece = board.GetPieceAtIndex(i);
            if (piece != Piece.None)
            {
                key ^= pieceKeys[piece, i];
            }
        }

        key ^= castlingKeys[board.castlingRights];

        key ^= enPassantKeys[GetTargetFile(board.enPassantTarget)];

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

    public static void MovePiece(ref ulong key, int movedPiece, int capturedPiece, int startSquare, int endSquare)
    {
        key ^= pieceKeys[movedPiece, startSquare];
        key ^= pieceKeys[movedPiece, endSquare];
        if ((capturedPiece & 0b111) != Piece.None)
        {
            key ^= pieceKeys[capturedPiece, endSquare];
        }
    }

    public static void TogglePiece(ref ulong key, int piece, int square)
    {
        key ^= pieceKeys[piece, square];
    }

    public static void ChangeCastling(ref ulong key, int oldRights, int newRights)
    {
        key ^= castlingKeys[oldRights];
        key ^= castlingKeys[newRights];
    }

    public static void ChangeEnPassantFile(ref ulong key, int oldTarget, int newTarget)
    {
        key ^= enPassantKeys[GetTargetFile(oldTarget)];
        key ^= enPassantKeys[GetTargetFile(newTarget)];
    }

    public static void ChangeTurn(ref ulong key)
    {
        key ^= turnKey;
    }
}