using System;

public class Bot : Player
{
    public int negativeInfinity = -1000000;
    public int positiveInfinity =  1000000;

    public override void Update()
    {
        PlayMove();
    }

    public void PlayMove()
    {
        int chosenMove = ChooseMove();
        Decided(chosenMove);
    }

    public int ChooseMove()
    {
        int bestEval = negativeInfinity;
        int chosenMove = board.legalMoves[0];
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

        return chosenMove;
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