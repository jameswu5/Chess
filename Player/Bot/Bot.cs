using System.Threading;

namespace Chess.Player;

public abstract class Bot : Player
{
    protected bool moveFound = false;
    protected int chosenMove = 0;

    public Bot(Game.Game game)
    {
        isActive = false;
        board = game.board;
    }

    public static Bot GetBotFromBotType(Type type, Game.Game game)
    {
        return type switch
        {
            Type.Random => new RandomBot(game),
            Type.Version1 => new Version1(game),
            Type.Version2 => new Version2(game),
            Type.Human => throw new Exception("Tried to create a bot with Player.Type.Human"),
            _ => throw new Exception("Bot not found"),
        };
    }

    public override void Update()
    {
        if (!isActive) return;

        if (moveFound)
        {
            moveFound = false;
            Decided(chosenMove);
        }
    }

    public override void TurnToMove(Game.Timer timer)
    {
        isActive = true;
        timer.secondsRemainingAtStart = timer.secondsRemaining;
        moveFound = false;
        Thread backgroundThread = new(() => ChooseMove(timer));
        backgroundThread.Start();
    }

    public abstract void ChooseMove(Game.Timer timer);
}