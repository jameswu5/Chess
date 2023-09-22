using System;
using System.Threading;

public class Bot : Player
{
    private bool moveFound = false;
    private int chosenMove = 0;

    private Searcher searcher = new Searcher();

    public override void Update()
    {
        if (moveFound)
        {
            moveFound = false;
            Decided(chosenMove);
        }
    }

    private void ThreadedSearch()
    {
        Thread backgroundThread = new Thread(ChooseMove);
        backgroundThread.Start();
    }

    public override void TurnToMove()
    {
        moveFound = false;
        ThreadedSearch();
    }

    public void ChooseMove()
    {
        chosenMove = searcher.FindBestMove(board);
        moveFound = true;
    }
}