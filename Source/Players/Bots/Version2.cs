using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Version2 : Bot
{
    private Searcher searcher = new Searcher();

    public override void ChooseMove(Timer timer)
    {
        searcher.bestMove = 0;
        searcher.nodesSearched = 0;

        float allocatedTime = 1f;

        // Start searching at depth 1
        int searchDepth = 1;

        try
        {
            while (true)
            {
                // Iterative deepening
                chosenMove = searcher.FindBestMove(board, timer, allocatedTime, searchDepth++);
            }
        }
        catch { }

        //Debug.Log($"Nodes searched: {searcher.nodesSearched} | Depth {searchDepth - 1}");

        moveFound = true;
    }

    public override string ToString()
    {
        return "Bot Version 2";
    }
}