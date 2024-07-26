using System;
using System.Collections;
using System.Collections.Generic;

public static class Perft
{
    public static int Search(Board board, int depth)
    {
        if (depth == 1) return board.legalMoves.Count;

        int numOfPositions = 0;

        foreach (int move in board.legalMoves)
        {
            board.MakeMove(move);
            numOfPositions += Search(board, depth - 1);
            board.UndoMove();
        }

        return numOfPositions;
    }

    public static void SearchWithBreakdown(Board board, int depth)
    {
        Diagnostics.Stopwatch stopwatch = new();
        stopwatch.Start();

        int numOfPositions = 0;
        foreach (int move in board.legalMoves)
        {
            board.MakeMove(move);
            int positions = Search(board, depth - 1);
            numOfPositions += positions;
            Console.WriteLine($"{Square.ConvertIndexToSquareName(Move.GetStartIndex(move))}{Square.ConvertIndexToSquareName(Move.GetEndIndex(move))}: {positions}");
            board.UndoMove();
        }

        Console.WriteLine($"Total: {numOfPositions} positions");

        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}", ts.Minutes, ts.Seconds, ts.Milliseconds);
        Console.WriteLine($"Runtime: {elapsedTime}");
    }

    public static void Test(Board board, int depth)
    {
        Diagnostics.Stopwatch stopwatch = new();
        stopwatch.Start();
        Console.WriteLine(Search(board, depth));
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}", ts.Minutes, ts.Seconds, ts.Milliseconds);
        Console.WriteLine($"Runtime: {elapsedTime}");
    }
}
