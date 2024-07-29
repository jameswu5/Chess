
namespace Chess.Search;

public class MoveOrdering : IComparer<int>
{
    public int Compare(int move1, int move2)
    {
        return GetScore(move1).CompareTo(GetScore(move2));
    }

    private static int GetScore(int move)
    {
        int score = 0;

        // Prioritise capturing most valuable pieces with least valuable pieces
        int capturedPieceType = Core.Move.GetCapturedPieceType(move);
        int movedPieceType = Core.Move.GetMovedPieceType(move);

        if (capturedPieceType != Core.Piece.None)
        {
            score += Evaluator.PieceValues[capturedPieceType] - Evaluator.PieceValues[movedPieceType];
        }

        // Try to promote a pawn
        int promotedPieceType = Core.Move.GetPromotedPieceType(move);
        if (promotedPieceType != Core.Piece.None)
        {
            score += Evaluator.PieceValues[promotedPieceType];
        }

        return score;
    }
}