using System;
using System.Collections.Generic;
using UnityEngine;

public class Searcher
{
    private Board board;

    private const int negativeInfinity = -1000000;
    private const int positiveInfinity = 1000000;
    private const int checkmateScore = 100000;
    public int searchDepth;

    private MoveOrdering moveOrderer = new MoveOrdering();

    private int bestMove;
    private int sign;
    public int nodesSearched;

    public int FindBestMove(Board board, Timer timer, int searchDepth)
    {
        this.board = board;

        bestMove = 0;
        nodesSearched = 0;

        this.searchDepth = searchDepth;

        // I'm not sure why sometimes the engine plays the best move possible and
        // other times the worst move possible so this is a fix by drawing truth table
        sign = (((board.turn == Piece.White ? 1 : 0) ^ (searchDepth & 1)) == 1) ? 1 : -1;

        Search(searchDepth, negativeInfinity, positiveInfinity);

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