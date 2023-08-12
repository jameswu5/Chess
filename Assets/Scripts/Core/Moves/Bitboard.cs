using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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


    public static ulong GetRayAttacks(int direction, int index)
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


    public static bool CheckAtEdgeOfBoard(int direction, ulong position)
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

    public static ulong[,] PrecomputeRayData()
    {
        int[] directions = { Direction.N, Direction.S, Direction.E, Direction.W, Direction.NW, Direction.SW, Direction.NE, Direction.SE };

        ulong[,] rayAttacks = new ulong[8, 64];

        for (int d = 0; d < 8; d++)
        {
            int direction = directions[d];
            for (int sq = 0; sq < 64; sq++)
            {
                rayAttacks[d, sq] = GetRayAttacks(direction, sq);
            }
        }

        return rayAttacks;
    }


    // For testing only
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
        Debug.Log(sb.ToString());
    }
}
