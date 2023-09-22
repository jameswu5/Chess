using System;
using System.Collections.Generic;

public class Searcher
{
    private Board board;

    const int negativeInfinity = -1000000;
    const int positiveInfinity = 1000000;

    public int FindBestMove(Board board)
    {
        this.board = board;

        int bestEval = negativeInfinity;
        int bestMove = board.legalMoves[0];

        foreach (int move in board.legalMoves)
        {
            board.MakeMove(move);
            int evaluation = Search(3, negativeInfinity, positiveInfinity);
            board.UndoMove();

            if (evaluation >= bestEval)
            {
                bestEval = evaluation;
                bestMove = move;
            }
        }
        return bestMove;
    }

    // I'm not sure if this alpha-beta pruning works
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