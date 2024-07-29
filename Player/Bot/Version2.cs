
namespace Chess.Player;

public class Version2 : Bot
{
    private Search.Searcher searcher = new();

    public Version2(Game.Game game) : base(game)
    { 
        searcher = new();
    }

    public override void ChooseMove(Game.Timer timer)
    {
        searcher.bestMove = 0;
        searcher.nodesSearched = 0;

        double allocatedTime = 0.5;

        // Start searching at depth 1
        int searchDepth = 1;

        try
        {
            while (true)
            {
                // Iterative deepening
                chosenMove = searcher.FindBestMove(board, timer, allocatedTime, searchDepth++);
            }
        }
        catch { }

        Console.WriteLine($"Nodes searched: {searcher.nodesSearched} | Depth {searchDepth - 1}");

        moveFound = true;
    }

    public override string ToString()
    {
        return "Bot Version 2";
    }
}