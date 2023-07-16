using System.Collections;
using System.Collections.Generic;



public struct Move
{
    public int startIndex;
    public int endIndex;
    public int pieceID;
    public bool isCaptureMove;



    public Move(int index, int newIndex, int pieceNumber, bool isCapture)
    {
        startIndex = index;
        endIndex = newIndex;
        pieceID = pieceNumber;
        isCaptureMove = isCapture;
    }

    public string GetMoveAsString()
    {
        string moveString = "";

        switch (pieceID % 8)
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
                break;
        }

        if (isCaptureMove) { moveString += "x"; }

        moveString += ConvertIndexToSquareName(endIndex);

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
