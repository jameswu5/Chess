using System;
using System.Collections.Generic;

public class MoveGeneration
{
    // N, E, S, W
    static (int, int)[] orthogonalDirections = { (1, 0), (0, 1), (-1, 0), (0, -1) };
    static (int, int)[] diagonalDirections = { (1, 1), (1, -1), (-1, 1), (-1, -1) };
    static (int, int)[] knightDirections = { (2, 1), (2, -1), (1, 2), (1, -2), (-1, 2), (-1, -2), (-2, 1), (-2, -1) };


    public ulong GenerateKnightAttacks(int startIndex)
    {
        ulong moves = 0ul;

        int file = startIndex & 0b111;
        int rank = startIndex >> 3;
        int index;

        foreach ((int x, int y) d in knightDirections)
        {
            if (IsValidSquare(file + d.x, rank + d.y, out index))
            {
                moves |= 1ul << index;
            }
        }

        return moves;
    }

    public bool IsValidSquare(int x, int y, out int index)
    {
        index = x + (y << 3);
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }


}
