using System.Threading;

namespace Chess.Player;

public abstract class Bot : Player
{
    protected bool moveFound = false;
    protected int chosenMove = 0;

    public static Bot GetBotFromBotType(Type type)
    {
        return type switch
        {
            Type.Random => new RandomBot(),
            Type.Human => throw new Exception("Tried to create a bot with Player.Type.Human"),
            _ => throw new Exception("Bot not found"),
        };
    }

    public override void Update()
    {
        if (moveFound)
        {
            moveFound = false;
            Decided(chosenMove);
        }
    }

    public override void TurnToMove(Game.Timer timer)
    {
        timer.secondsRemainingAtStart = timer.secondsRemaining;
        moveFound = false;
        Thread backgroundThread = new(() => ChooseMove(timer));
        backgroundThread.Start();
    }

    public abstract void ChooseMove(Game.Timer timer);
}