using System;
using System.Collections;
using System.Collections.Generic;

public class Bot : Player
{
    public override void Update()
    {
        Move chosenMove = ChooseMove(board);
        board.PlayMove(chosenMove);
        board.ResetSquareColour(chosenMove.startIndex);
    }

    public Move ChooseMove(Board board)
    {
        int bestEval = -100000;

        HashSet<Move> legalMoves = board.GetAllLegalMoves(board.turn);
        List<Move> legalMovesAsList = new(legalMoves);
        Move chosenMove = legalMovesAsList[0];

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            int eval = Search(board, 1);
            if (eval >= bestEval)
            {
                bestEval = eval;
                chosenMove = move;
            }
            board.UndoMove();
        }

        return chosenMove;
    }


    private int Search(Board board, int depth)
    {
        if (depth == 0)
        {
            return Evaluator.EvaluateBoard(board);
        }

        HashSet<Move> legalMoves = board.GetAllLegalMoves(board.turn);

        if (legalMoves.Count == 0)
        {
            return board.inCheck ? 100000 : 0; // checkmate : stalemate
        }

        int best = -100000;

        foreach (Move move in legalMoves)
        {
            int eval;
            board.MakeMove(move);
            Board.Result result = board.GetGameResult();
            if (result != Board.Result.Checkmate && result != Board.Result.Playing)
            {
                eval = 0;
            }
            else
            {
                eval = -Search(board, depth - 1);
            }
            best = Math.Max(best, eval);
            board.UndoMove();
        }

        return best;
    }

}