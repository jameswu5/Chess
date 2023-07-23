using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Move
{
    public int moveType;
    public int startIndex;
    public int endIndex;
    public int pieceType;
    public bool isCaptureMove;

    public const int Standard = 1;
    public const int PawnTwoSquares = 2;
    public const int Castling = 3;
    public const int EnPassant = 4;
    public const int PromoteToQueen = 5;
    public const int PromoteToRook = 6;
    public const int PromoteToBishop = 7;
    public const int PromoteToKnight = 8;


    public Move(int moveType, int startIndex, int endIndex, int pieceType, bool isCaptureMove)
    {
        this.moveType = moveType;
        this.startIndex = startIndex;
        this.endIndex = endIndex;
        this.pieceType = pieceType;
        this.isCaptureMove = isCaptureMove;
    }

    public string GetMoveAsString()
    {
        string moveString = "";

        if (moveType == Standard || moveType == PawnTwoSquares || moveType == EnPassant)
        {
            switch (pieceType)
            {
                case Piece.King:
                    moveString += "K";
                    break;
                case Piece.Queen:
                    moveString += "Q";
                    break;
                case Piece.Bishop:
                    moveString += "B";
                    break;
                case Piece.Knight:
                    moveString += "N";
                    break;
                case Piece.Rook:
                    moveString += "R";
                    break;
                case Piece.Pawn:
                    if (isCaptureMove == true)
                    {
                        moveString += GetFileName(startIndex);
                    }
                    break;
                default:
                    Debug.Log("Cannot identify piece.");
                    break;
            }

            if (isCaptureMove) { moveString += "x"; }

            moveString += ConvertIndexToSquareName(endIndex);
        }
        else if (moveType == Castling)
        {
            if (GetFileName(endIndex) == "g") // Kingside
            {
                moveString = "O-O";
            }
            else // Queenside
            {
                moveString = "O-O-O";
            }
        }
        else if (moveType == PromoteToQueen || moveType == PromoteToRook || moveType == PromoteToBishop || moveType == PromoteToKnight)
        {
            if (isCaptureMove == true)
            {
                moveString += GetFileName(startIndex);
                moveString += "x";
            }
            moveString += ConvertIndexToSquareName(endIndex);

            switch (moveType) {
                case PromoteToQueen:
                    moveString += "Q";
                    break;
                case PromoteToRook:
                    moveString += "R";
                    break;
                case PromoteToBishop:
                    moveString += "B";
                    break;
                case PromoteToKnight:
                    moveString += "N";
                    break;
                default:
                    Debug.Log("Cannot find promotion piece");
                    break;
            }

        }


        return moveString;
    }

    public string ConvertIndexToSquareName(int index)
    {
        int rank = (index / 8) + 1;
        return GetFileName(index) + rank.ToString();
    }

    public string GetFileName(int index)
    {
        int file = (index % 8) + 1;
        string[] letters = { "#", "a", "b", "c", "d", "e", "f", "g", "h" };
        return letters[file];
    }

}
