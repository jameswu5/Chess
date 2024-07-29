
namespace Chess.Player;

public class RandomBot : Bot
{
    private readonly Random rng;

    public RandomBot(Game.Game game) : base(game)
    {
        rng = new Random();
    }

    public override void ChooseMove(Game.Timer timer)
    {
        chosenMove = board.legalMoves[rng.Next(0, board.legalMoves.Count)];
        moveFound = true;
    }

    public override string ToString()
    {
        return "Random Bot";
    }
}