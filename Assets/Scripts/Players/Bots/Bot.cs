using System;
using System.Threading;

public abstract class Bot : Player
{
    protected bool moveFound = false;
    protected int chosenMove = 0;

    public static Bot GetBotFromBotType(Type type)
    {
        switch (type)
        {
            case Type.Random:
                return new RandomBot();
            case Type.Version1:
                return new Version1();
            case Type.Human:
                throw new Exception("Tried to create a bot with Player.Type.Human");
            default:
                throw new Exception("Bot not found");
        }
    }

    public override void Update()
    {
        if (moveFound)
        {
            moveFound = false;
            Decided(chosenMove);
        }
    }

    public override void TurnToMove()
    {
        moveFound = false;
        Thread backgroundThread = new Thread(ChooseMove);
        backgroundThread.Start();
    }

    public abstract void ChooseMove();
}