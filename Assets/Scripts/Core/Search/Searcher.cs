using System;
using System.Collections.Generic;
using UnityEngine;

public class Searcher
{
    private Board board;

    const int negativeInfinity = -1000000;
    const int positiveInfinity = 1000000;

    int searchDepth = 0;
    int bestMove = 0;


    public int FindBestMove(Board board)
    {
        this.board = board;

        bestMove = 0;
        searchDepth = 3;

        Search(searchDepth, negativeInfinity, positiveInfinity);

        return bestMove;
    }

    private int Search(int depth, int alpha, int beta)
    {

        if (depth == 0)
        {
            return Evaluator.EvaluateBoard(board);
        }


        foreach (int move in board.legalMoves)
        {
            if (alpha >= beta) break;

            board.MakeMove(move);

            int score;

            if (board.legalMoves.Count == 0)
            {
                score = board.inCheck ? positiveInfinity : 0;
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