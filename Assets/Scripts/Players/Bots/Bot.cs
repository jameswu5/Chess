using System;
using System.Threading;

public abstract class Bot : Player
{
    public enum BotType { Random, Version1 };

    protected bool moveFound = false;
    protected int chosenMove = 0;

    public static Bot GetBotFromBotType(BotType type)
    {
        switch (type)
        {
            case BotType.Random:
                return new RandomBot();
            case BotType.Version1:
                return new Version1();
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