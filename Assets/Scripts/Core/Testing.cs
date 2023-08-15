using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Perft
{
    public static int Search(Board board, int depth)
    {
        if (depth == 0) return 1;

        HashSet<int> legalMoves = board.GetAllLegalMoves(board.turn);
        int numOfPositions = 0;

        foreach (int move in legalMoves)
        {
            board.MakeMove(move);
            numOfPositions += Search(board, depth - 1);
            board.UndoMove();
        }

        return numOfPositions;
    }

    public static void SearchWithBreakdown(Board board, int depth)
    {
        HashSet<int> legalMoves = board.GetAllLegalMoves(board.turn);
        int numOfPositions = 0;
        foreach (int move in legalMoves)
        {
            board.MakeMove(move);
            int positions = Search(board, depth - 1);
            numOfPositions += positions;
            Debug.Log($"{Square.ConvertIndexToSquareName(Move.GetStartIndex(move))}{Square.ConvertIndexToSquareName(Move.GetEndIndex(move))}: {positions}");
            board.UndoMove();
        }

        Debug.Log($"Total: {numOfPositions} positions");

    }


    public static void Test(Board board, int depth)
    {
        System.Diagnostics.Stopwatch stopwatch = new();
        stopwatch.Start();
        Debug.Log(Search(board, depth));
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        string elapsedTime = string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        Debug.Log($"Runtime: {elapsedTime}");
    }
}