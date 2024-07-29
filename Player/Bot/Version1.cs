
namespace Chess.Player;

public class Version1 : Bot
{
    private Searcher1 searcher;

    public Version1(Game.Game game) : base(game)
    {
        searcher = new();
    }

    public override void ChooseMove(Game.Timer timer)
    {
        chosenMove = searcher.FindBestMove(board);
        moveFound = true;
    }

    public override string ToString()
    {
        return "Bot Version 1";
    }
}

// This is version 1 of the searcher with simple negamax
public class Searcher1
{
    private Core.Board board;

    const int negativeInfinity = -1000000;
    const int positiveInfinity = 1000000;
    const int checkmateScore = 100000;
    const int searchDepth = 4;

    int nodesSearched;

    int bestMove;
    int sign;

    public int FindBestMove(Core.Board board)
    {
        this.board = board;

        bestMove = 0;
        nodesSearched = 0;

        // I'm not sure why sometimes the engine plays the best move possible and
        // other times the worst move possible so this is a fix by drawing truth table
        sign = (((board.turn == Core.Piece.White ? 1 : 0) ^ (searchDepth & 1)) == 1) ? 1 : -1;

        Search(searchDepth, negativeInfinity, positiveInfinity);
        Console.WriteLine($"Nodes searched (without move ordering): {nodesSearched}");
        return bestMove;
    }

    private int Search(int depth, int alpha, int beta)
    {
        nodesSearched++;

        if (depth == 0)
        {
            return sign * Chess.Search.Evaluator.EvaluateBoard(board);
        }

        foreach (int move in board.legalMoves)
        {
            if (alpha >= beta) break;

            board.MakeMove(move);

            int score;

            if (board.legalMoves.Count == 0)
            {
                score = board.inCheck ? checkmateScore : 0;
            }
            else
            {
                score = -Search(depth - 1, -beta, -alpha);
            }

            if (score > alpha)
            {
                alpha = score;
                if (depth == searchDepth)
                {
                    bestMove = move;
                }
            }

            board.UndoMove();
        }

        return alpha;
    }
}