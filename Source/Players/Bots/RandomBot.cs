using System.Collections;
using System.Collections.Generic;

public class RandomBot : Bot
{
    System.Random rng = new();

    public override void ChooseMove(Timer timer)
    {
        chosenMove = board.legalMoves[rng.Next(0, board.legalMoves.Count)];
        moveFound = true;
    }

    public override string ToString()
    {
        return "Random Bot";
    }
}
