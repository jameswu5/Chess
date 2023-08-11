using System;
using System.Collections;
using System.Collections.Generic;
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


    public static ulong[,] ComputeRayAttacks()
    {

        ulong[,] rayAttacks = new ulong[8, 64];

        for (int i = 0; i < 64; i++)
        {
            // north
            rayAttacks[0, i] = 0x0101010101010100ul << i;

            // east
            rayAttacks[1, i] = ((1ul << (i | 7)) - (1ul << i)) << 1;

            // south
            rayAttacks[2, i] = 0x0080808080808080ul >> (i ^ 63);

            // west
            rayAttacks[3, i] = (1ul << i) - (1ul << (i & 56));


            // need to do diagonals
        }


        return rayAttacks;
    }




    public static void DisplayAsMatrix(ulong bitboard)
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
