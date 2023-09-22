using System;
using System.Threading;

public class Bot : Player
{
    public int negativeInfinity = -1000000;
    public int positiveInfinity =  1000000;

    private bool moveFound = false;
    private int chosenMove = 0;

    public override void Update()
    {
        if (moveFound)
        {
            moveFound = false;
            Decided(chosenMove);
        }
    }

    private void ThreadedSearch()
    {
        Thread backgroundThread = new Thread(ChooseMove);
        backgroundThread.Start();
    }


    public override void TurnToMove()
    {
        moveFound = false;
        ThreadedSearch();
    }


    public void ChooseMove()
    {
        int bestEval = negativeInfinity;
        chosenMove = board.legalMoves[0];

        foreach (int move in board.legalMoves)
        {
            board.MakeMove(move);
            int evaluation = Search(3, negativeInfinity, positiveInfinity);
            board.UndoMove();

            if (evaluation >= bestEval)
            {
                bestEval = evaluation;
                chosenMove = move;
            }
        }
        moveFound = true;
    }

    // Implements alpha-beta pruning
    private int Search(int depth, int alpha, int beta)
    {
        if (depth == 0)
        {
            return Evaluator.EvaluateBoard(board);
        }

        if (board.legalMoves.Count == 0)
        {
            return board.inCheck ? negativeInfinity : 0;
        }

        foreach (int move in board.legalMoves)
        {
            board.MakeMove(move);
            int evaluation = -Search(depth - 1, -beta, -alpha);
            board.UndoMove();

            if (evaluation >= beta)
            {
                return beta;
            }

            alpha = Math.Max(alpha, evaluation);
        }

        return alpha;
    }
}