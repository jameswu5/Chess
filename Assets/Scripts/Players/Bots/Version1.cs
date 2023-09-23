using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Version1 : Bot
{
    private Searcher searcher = new Searcher();

    public override void ChooseMove()
    {
        chosenMove = searcher.FindBestMove(board);
        moveFound = true;
    }
}