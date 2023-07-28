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
        HashSet<Move> legalMoves = board.GetAllLegalMoves(board.turn);
        List<Move> legalMovesList = new List<Move>(legalMoves);
        Random rng = new Random();
        Move chosenMove = legalMovesList[rng.Next(legalMovesList.Count - 1)];
        return chosenMove;
    }
}