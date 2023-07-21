using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInfo
{
    public Move move;
    public int capturedPiece; // type of captured piece
    public bool[] disabledCastlingRights; // true if the castling right was disabled at that index

    public MoveInfo(Move move, int capturedPiece, bool[] disabledCastlingRights)
    {
        this.move = move;
        this.capturedPiece = capturedPiece;
        this.disabledCastlingRights = disabledCastlingRights;
    }

}
