using System;
using System.Collections;
using System.Collections.Generic;


public static class Direction
{
    public const int NW = 7;
    public const int N = 8;
    public const int NE = 9;
    public const int E = 1;
    public const int SE = -7;
    public const int S = -8;
    public const int SW = -9;
    public const int W = -1;

    /* This is designed so that:
     *   first 4 is orthogonal, last 4 is diagonal
     *   even indices are positive shifts, odd indices are negative shifts
     */ 
    public static int[] directions = { N, S, E, W, NW, SW, NE, SE };

    public static (int, int)[] knightDirections = { (2, 1), (2, -1), (1, 2), (1, -2), (-1, 2), (-1, -2), (-2, 1), (-2, -1) };


    public static int GetIndexFromDirection(int direction)
    {
        Dictionary<int, int> dict = new Dictionary<int, int>()
        {
            { N, 0 },
            { S, 1 },
            { E, 2 },
            { W, 3 },
            { NW, 4 },
            { SW, 5 },
            { NE, 6 },
            { SE, 7 },
        };

        return dict[direction];
    }
}
