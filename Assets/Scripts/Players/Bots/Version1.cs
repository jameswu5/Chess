using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Version1 : Bot
{
    private Searcher1 searcher = new Searcher1();

    public override void ChooseMove()
    {
        chosenMove = searcher.FindBestMove(board);
        moveFound = true;
    }
}

// This is version 1 of the searcher with simple negamax
public class Searcher1
{
    private Board board;

    const int negativeInfinity = -1000000;
    const int positiveInfinity = 1000000;
    const int searchDepth = 3;

    int bestMove;
    int sign;

    public int FindBestMove(Board board)
    {
        this.board = board;

        // For some reason sometimes the best move is never updated so the game
        // crashes, so I set it by default to the first available move
        bestMove = board.legalMoves[0];

        // I'm not sure why sometimes the engine plays the best move possible and
        // other times the worst move possible so this is a fix by drawing truth table
        sign = (((board.turn == Piece.White ? 1 : 0) ^ (searchDepth & 1)) == 1) ? 1 : -1;

        Search(searchDepth, negativeInfinity, positiveInfinity);
        return bestMove;
    }

    private int Search(int depth, int alpha, int beta)
    {
        if (depth == 0)
        {
            return sign * Evaluator.EvaluateBoard(board);
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