using System;
using System.Collections.Generic;
using UnityEngine;

public class Searcher
{
    private Board board;

    const int negativeInfinity = -1000000;
    const int positiveInfinity = 1000000;
    const int checkmateScore = 100000;
    const int searchDepth = 4;

    private MoveOrdering moveOrderer = new MoveOrdering();

    int bestMove;
    int sign;

    int nodesSearched;

    public int FindBestMove(Board board)
    {
        this.board = board;

        bestMove = 0;
        nodesSearched = 0;

        // I'm not sure why sometimes the engine plays the best move possible and
        // other times the worst move possible so this is a fix by drawing truth table
        sign = (((board.turn == Piece.White ? 1 : 0) ^ (searchDepth & 1)) == 1) ? 1 : -1;

        Search(searchDepth, negativeInfinity, positiveInfinity);

        Debug.Log($"Nodes searched (with move ordering): {nodesSearched}");

        return bestMove;
    }

    private int Search(int depth, int alpha, int beta)
    {
        nodesSearched++;

        if (depth == 0)
        {
            return sign * Evaluator.EvaluateBoard(board);
        }

        board.legalMoves.Sort(moveOrderer);

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