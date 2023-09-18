public static class Judge
{
    public enum Result
    {
        Playing,
        WhiteIsMated,
        BlackIsMated,
        Stalemate,
        Insufficient,
        Threefold,
        FiftyMove,
        WhiteOutOfTime,
        BlackOutOfTime
    }

    public static Result GetResult(Board board)
    {
        if (board.legalMoves.Count == 0)
        {
            if (!board.inCheck)
            {
                return Result.Stalemate;
            }
            return board.turn == Piece.White ? Result.WhiteIsMated : Result.BlackIsMated;
        }

        if (board.fiftyMoveCounter >= 100)
        {
            return Result.FiftyMove;
        }

        if (board.CheckForInsufficientMaterial())
        {
            return Result.Insufficient;
        }

        if (board.table[board.zobristKey] >= 3)
        {
            return Result.Threefold;
        }

        return Result.Playing;
    }
}
