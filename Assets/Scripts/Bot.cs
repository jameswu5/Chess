using System;
using System.Collections;
using System.Collections.Generic;

public class Bot : Player
{
    public override void Update()
    {
        
    }

    public Move ChooseMove(Board board)
    {
        HashSet<Move> legalMoves = board.GetAllLegalMoves(board.turn);

        foreach (Move move in legalMoves)
        {
            return move;
        }


        return null; // this should never be called
    

    }
}