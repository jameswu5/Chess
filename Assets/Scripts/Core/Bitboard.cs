using System;
using System.Collections;
using System.Collections.Generic;

public static class Bitboard
{
    public const ulong FileA = 0b1000000010000000100000001000000010000000100000001000000010000000;
    public const ulong FileH = FileA >> 7;

    public const ulong Rank1 = 0b11111111;
    public const ulong Rank2 = Rank1 << 8;
    public const ulong Rank3 = Rank2 << 8;
    public const ulong Rank4 = Rank3 << 8;
    public const ulong Rank5 = Rank4 << 8;
    public const ulong Rank6 = Rank5 << 8;
    public const ulong Rank7 = Rank6 << 8;
    public const ulong Rank8 = Rank7 << 8;


    public static void SetSquare(ref ulong bitBoard, int index)
    {
        bitBoard |= 1ul << index;
    }

    public static void ClearSquare(ref ulong bitBoard, int index)
    {
        bitBoard &= ~(1ul << index);
    }

    public static void Move(ref ulong bitBoard, int startIndex, int endIndex)
    {
        ClearSquare(ref bitBoard, startIndex);
        SetSquare(ref bitBoard, endIndex);
    }

}
