using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Bitboard
{
    public const ulong FileA = 0b0000000100000001000000010000000100000001000000010000000100000001;
    public const ulong FileH = FileA << 7;

    public const ulong Rank1 = 0b11111111;
    public const ulong Rank2 = Rank1 << 8;
    public const ulong Rank3 = Rank2 << 8;
    public const ulong Rank4 = Rank3 << 8;
    public const ulong Rank5 = Rank4 << 8;
    public const ulong Rank6 = Rank5 << 8;
    public const ulong Rank7 = Rank6 << 8;
    public const ulong Rank8 = Rank7 << 8;

    public const ulong WhiteKingsideMask  = 1ul << Square.f1 | 1ul << Square.g1;
    public const ulong WhiteQueensideMask = 1ul << Square.c1 | 1ul << Square.d1;
    public const ulong BlackKingsideMask  = 1ul << Square.f8 | 1ul << Square.g8;
    public const ulong BlackQueensideMask = 1ul << Square.c8 | 1ul << Square.d8;

    public static void SetSquare(ref ulong bitboard, int index)
    {
        bitboard |= 1ul << index;
    }
    
    public static void ClearSquare(ref ulong bitboard, int index)
    {
        bitboard &= ~(1ul << index);
    }

    public static void Move(ref ulong bitboard, int startIndex, int endIndex)
    {
        ClearSquare(ref bitboard, startIndex);
        SetSquare(ref bitboard, endIndex);
    }

    // left shifts by shift
    public static ulong ShiftLeft(ulong bitboard, int shift)
    {
        if (shift > 0)
        {
            return bitboard << shift;
        }
        else
        {
            return bitboard >> -shift;
        }
    }

    public static ulong GetRayAttacksEmptyBoard(int direction, int index)
    {
        ulong attacks = 0ul;
        ulong current = 1ul << index;

        while (current > 0 && !CheckAtEdgeOfBoard(direction, current))
        {
            current = ShiftLeft(current, direction);
            attacks |= current;
        }

        return attacks;
    }

    public static ulong GenerateKingAttacks(int index)
    {
        ulong position = 1ul << index;
        ulong moves = 0ul;

        foreach (int direction in Direction.directions)
        {
            if (!CheckAtEdgeOfBoard(direction, position))
            {
                moves |= ShiftLeft(position, direction);
            }
        }

        return moves;
    }

    public static ulong GeneratePawnAttacks(int index, int colour)
    {
        int[] directions = colour == Piece.White ? new int[] { Direction.NE, Direction.NW } : new int[] { Direction.SE, Direction.SW };

        ulong position = 1ul << index;
        ulong attacks = 0ul;

        foreach (int direction in directions)
        {
            if (!CheckAtEdgeOfBoard(direction, position))
            {
                attacks |= ShiftLeft(position, direction);
            }
        }

        return attacks;
    }

    public static ulong GeneratePawnPushes(int index, int colour)
    {

        int direction = colour == Piece.White ? Direction.N : Direction.S;
        ulong position = 1ul << index;
        ulong pushes = 0ul;

        if (!CheckAtEdgeOfBoard(direction, position))
        {
            ulong singlePush = ShiftLeft(position, direction);
            pushes |= singlePush;

            if ((colour == Piece.White && (position & Rank2) > 0) || (colour == Piece.Black && (position & Rank7) > 0))
            {
                pushes |= ShiftLeft(singlePush, direction);
            }
        }

        return pushes;
    }

    public static ulong GenerateKnightAttacks(int startIndex)
    {
        ulong moves = 0ul;

        int file = startIndex & 0b111;
        int rank = startIndex >> 3;
        int index;

        foreach ((int x, int y) d in Direction.knightDirections)
        {
            if (IsValidSquare(file + d.x, rank + d.y, out index))
            {
                moves |= 1ul << index;
            }
        }

        return moves;
    }

    public static bool IsValidSquare(int x, int y, out int index)
    {
        index = x + (y << 3);
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    // This is for precomputation and I hardcode the values generated by this algorithm
    private static int GetSquaresFromEdgeOfBoard(int index, int direction)
    {
        int count = 0;
        while (!CheckAtEdgeOfBoard(direction, 1ul << index)) {
            count++;
            index += direction;
        }
        return count;
    }

    private static bool CheckAtEdgeOfBoard(int direction, ulong position)
    {
        int[] north = { Direction.NW, Direction.N, Direction.NE };
        int[] east = { Direction.NE, Direction.E, Direction.SE };
        int[] south = { Direction.SW, Direction.S, Direction.SE };
        int[] west = { Direction.NW, Direction.W, Direction.SW };

        if (north.Contains(direction) && (position & Rank8) > 0)
        {
            return true;
        }
        if (east.Contains(direction) && (position & FileH) > 0)
        {
            return true;
        }
        if (south.Contains(direction) && (position & Rank1) > 0)
        {
            return true;
        }
        if (west.Contains(direction) && (position & FileA) > 0)
        {
            return true;
        }
        return false;
    }

    // This is for precomputation and I hardcode the values generated by this algorithm
    private static ulong[,] PrecomputeRayData()
    {
        int[] directions = { Direction.N, Direction.S, Direction.E, Direction.W, Direction.NW, Direction.SW, Direction.NE, Direction.SE };

        ulong[,] rayAttacks = new ulong[8, 64];

        for (int d = 0; d < 8; d++)
        {
            int direction = directions[d];
            for (int sq = 0; sq < 64; sq++)
            {
                rayAttacks[d, sq] = GetRayAttacksEmptyBoard(direction, sq);
            }
        }

        return rayAttacks;
    }

    // This is for precomputation and I hardcode the values generated by this algorithm
    private static ulong[,] GetRayThroughSquares()
    {
        ulong[,] result = new ulong[64, 64];

        ulong position, ray;
        int cur;
        List<int> indices;

        // bottom and left edges will cover every possible ray
        // bottom edge - N, NW, NE
        for (int i = 0; i < 8; i++)
        {
            int[] dir = { Direction.N, Direction.NW, Direction.NE };
            foreach (int d in dir)
            {
                position = 1ul << i;
                ray = position;
                cur = i;
                indices = new() { i };
                while (!CheckAtEdgeOfBoard(d, position))
                {
                    position = ShiftLeft(position, d);
                    ray |= position;
                    cur += d;
                    indices.Add(cur);
                }
                for (int a = 0; a < indices.Count; a++)
                {
                    for (int b = 0; b < indices.Count; b++)
                    {
                        if (a != b)
                        {
                            result[indices[a], indices[b]] = ray;
                        }
                    }
                }
            }
        }

        // left edge - E, SE and NE
        for (int i = 0; i < 64; i += 8)
        {
            int[] dir = { Direction.E, Direction.SE, Direction.NE };
            foreach (int d in dir)
            {
                position = 1ul << i;
                ray = position;
                cur = i;
                indices = new() { i };
                while (!CheckAtEdgeOfBoard(d, position))
                {
                    position = ShiftLeft(position, d);
                    ray |= position;
                    cur += d;
                    indices.Add(cur);
                }
                for (int a = 0; a < indices.Count; a++)
                {
                    for (int b = 0; b < indices.Count; b++)
                    {
                        if (a != b)
                        {
                            result[indices[a], indices[b]] = ray;
                        }
                    }
                }
            }
        }

        // top edge - SE
        for (int i = 56; i < 64; i++)
        {
            position = 1ul << i;
            ray = position;
            cur = i;
            indices = new() { i };
            while (!CheckAtEdgeOfBoard(Direction.SE, position))
            {
                position = ShiftLeft(position, Direction.SE);
                ray |= position;
                cur += Direction.SE;
                indices.Add(cur);
            }
            for (int a = 0; a < indices.Count; a++)
            {
                for (int b = 0; b < indices.Count; b++)
                {
                    if (a != b)
                    {
                        result[indices[a], indices[b]] = ray;
                    }
                }
            }
        }

        return result;
    }

    // Gets the position of the least significant bit that is a 1
    private static int BitScanForward(ulong data)
    {
        // There is no bit that is equal to 1
        if (data == 0) return -1;

        int n = 0;

        if ((data & 0xFFFFFFFF) == 0) { n += 32; data >>= 32; }
        if ((data & 0x0000FFFF) == 0) { n += 16; data >>= 16; }
        if ((data & 0x000000FF) == 0) { n += 8; data >>= 8; }
        if ((data & 0x0000000F) == 0) { n += 4; data >>= 4; }
        if ((data & 0x00000003) == 0) { n += 2; data >>= 2; }
        if ((data & 0x00000001) == 0) { n += 1; }

        return n;
    }

    // Gets the position of the most significant bit that is a 1
    private static int BitScanReverse(ulong data)
    {
        // There is no bit that is equal to 1
        if (data == 0) return -1;

        int n = 63;

        if ((data & 0xFFFFFFFF00000000) == 0) { n -= 32; data <<= 32; }
        if ((data & 0xFFFF000000000000) == 0) { n -= 16; data <<= 16; }
        if ((data & 0xFF00000000000000) == 0) { n -= 8; data <<= 8; }
        if ((data & 0xF000000000000000) == 0) { n -= 4; data <<= 4; }
        if ((data & 0xC000000000000000) == 0) { n -= 2; data <<= 2; }
        if ((data & 0x8000000000000000) == 0) { n -= 1; }

        return n;
    }

    // Takes blockers into consideration.
    public static ulong GetRayAttacks(ulong hero, ulong opponent, int dirIndex, int squareIndex, ulong ignore = 0) {
        ulong attacks = Data.RayAttacks[dirIndex][squareIndex];
        ulong blockers = attacks & (hero | opponent) & ~ignore;

        if (blockers > 0) {
            int blocker = Direction.directions[dirIndex] > 0 ? BitScanForward(blockers) : BitScanReverse(blockers);
            ulong block = ShiftLeft(1ul, blocker);
            
            attacks ^= Data.RayAttacks[dirIndex][blocker];

            // if the blocker is my own piece then clear that square
            if ((block & hero) > 0) {
                attacks &= ~block;
            }
        }

        return attacks;
    }

    public static IEnumerable<int> GetIndicesFromBitboard(ulong bitboard)
    {
        while (bitboard > 0)
        {
            yield return BitScanForward(bitboard);
            bitboard &= bitboard - 1;
        }
    }

    // For testing only
    public static ulong CreateBitboard(IEnumerable<int> occupiedIndices)
    {
        ulong bitboard = 0;

        foreach (int index in occupiedIndices)
        {
            bitboard |= 1ul << index;
        }
        return bitboard;
    }

    public static void Display(ulong bitboard)
    {
        StringBuilder sb = new();
        sb.Append("\n");
        string bitboardAsString = Convert.ToString((long)bitboard, 2);
        StringBuilder sb2 = new();
        for (int i = 0; i < 64 - bitboardAsString.Length; i++)
        {
            sb2.Append("0");
        }
        sb2.Append(bitboardAsString);
        string b = sb2.ToString();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 7; j >= 0; j--)
            {
                sb.Append(b[(i << 3) + j]);
            }
            sb.Append("\n");
        }
        Console.WriteLine(sb.ToString());
    }
}
