using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Version2 : Bot
{
    private Searcher searcher = new Searcher();

    public override void ChooseMove(Timer timer)
    {
        float startTime = timer.secondsRemaining;
        float allocatedTime = 0.2f;

        // Iterative deepening
        int searchDepth = 1;

        while (startTime - timer.secondsRemaining < allocatedTime)
        {
            chosenMove = searcher.FindBestMove(board, timer, searchDepth++);
        }

        Debug.Log($"Nodes searched: {searcher.nodesSearched} | Depth {searchDepth - 1}");
        moveFound = true;
    }

    public override string ToString()
    {
        return "Bot Version 2";
    }
}