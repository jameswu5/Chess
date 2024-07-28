
namespace Chess.Core;

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
        System.Diagnostics.Stopwatch stopwatch = new();
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

    public static void Test(string FEN, int depth)
    {
        Board board = new();
        board.Initialise(FEN);
        System.Diagnostics.Stopwatch stopwatch = new();
        stopwatch.Start();
        Console.WriteLine(Search(board, depth));
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}", ts.Minutes, ts.Seconds, ts.Milliseconds);
        Console.WriteLine($"Runtime: {elapsedTime}");
    }

    public static void TestAllCases()
    {
        TestCase(FEN.standard, 5, 4865609, "1");
        TestCase(FEN.PerftTestPos2, 4, 4085603, "2");
        TestCase(FEN.PerftTestPos3, 6, 11030083, "3");
        TestCase(FEN.PerftTestPos4a, 5, 15833292, "4a");
        TestCase(FEN.PerftTestPos4b, 5, 15833292, "4b");
        TestCase(FEN.PerftTestPos5, 4, 2103487, "5");
        TestCase(FEN.PerftTestPos6, 4, 3894594, "6");
    }

    private static void TestCase(string FEN, int depth, int expected, string name = "")
    {
        Board board = new();
        board.Initialise(FEN);
        System.Diagnostics.Stopwatch stopwatch = new();
        stopwatch.Start();
        int result = Search(board, depth);
        stopwatch.Stop();

        Console.WriteLine(result == expected ? $"Test {name} passed" : $"Test {name} failed");

        TimeSpan ts = stopwatch.Elapsed;
        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}", ts.Minutes, ts.Seconds, ts.Milliseconds);
        Console.WriteLine($"Runtime [{name}]: {elapsedTime} | Nodes per second: {Math.Round(result / ts.TotalSeconds, 0)}");
    }
}