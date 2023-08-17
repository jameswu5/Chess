using System;
using System.Collections;
using System.Collections.Generic;

public class Bot : Player
{
    private UI boardUI;

    public Bot()
    {
        boardUI = board.boardUI;
    }

    public override void Update()
    {
        PlayMove();
    }

    public void PlayMove()
    {
        int chosenMove = ChooseMove(board);
        board.PlayMove(chosenMove);
        boardUI.ResetSquareColour(Move.GetStartIndex(chosenMove));
    }

    public int ChooseMove(Board board)
    {
        int bestEval = -100000;

        int chosenMove = board.allLegalMoves[0];

        foreach (int move in board.allLegalMoves)
        {
            board.MakeMove(move, false);
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


        if (board.allLegalMoves.Count == 0)
        {
            return board.inCheck ? -100000 : 0; // checkmate : stalemate
        }

        int best = -100000;

        foreach (int move in board.allLegalMoves)
        {
            int eval;
            board.MakeMove(move, false);
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