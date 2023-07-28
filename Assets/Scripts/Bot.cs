using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bot : Player
{
    public Board board = GameObject.FindGameObjectWithTag("BoardObject").GetComponent<Board>();

    public override void Update()
    {
        Move chosenMove = ChooseMove(board);
        board.PlayMove(chosenMove);
    }

    public Move ChooseMove(Board board)
    {
        HashSet<Move> legalMoves = board.GetAllLegalMoves(board.turn);
        List<Move> legalMovesList = new List<Move>(legalMoves);
        System.Random rng = new System.Random();
        Move chosenMove = legalMovesList[rng.Next(legalMovesList.Count - 1)];
        return chosenMove;
    
    }
}