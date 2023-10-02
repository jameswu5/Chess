using System;
using System.Threading;
using UnityEngine;

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
            case Type.Version2:
                return new Version2();
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

    public override void TurnToMove(Timer timer)
    {
        moveFound = false;
        Thread backgroundThread = new Thread(() => ChooseMove(timer));
        backgroundThread.Start();
    }

    public abstract void ChooseMove(Timer timer);
}