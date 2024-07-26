using System;
using System.Collections;
using System.Collections.Generic;

public class MoveOrdering : IComparer<int>
{
    public int Compare(int move1, int move2)
    {
        return GetScore(move1).CompareTo(GetScore(move2));
    }

    private int GetScore(int move)
    {
        int score = 0;

        // Prioritise capturing most valuable pieces with least valuable pieces
        int capturedPieceType = Move.GetCapturedPieceType(move);
        int movedPieceType = Move.GetMovedPieceType(move);

        if (capturedPieceType != Piece.None)
        {
            score += Evaluator.PieceValues[capturedPieceType] - Evaluator.PieceValues[movedPieceType];
        }

        // Try to promote a pawn
        int promotedPieceType = Move.GetPromotedPieceType(move);
        if (promotedPieceType != Piece.None)
        {
            score += Evaluator.PieceValues[promotedPieceType];
        }

        return score;
    }
}
