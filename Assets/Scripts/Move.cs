using System.Collections;
using System.Collections.Generic;



public struct Move
{
    public int moveType;
    public int startIndex;
    public int endIndex;
    public int pieceID;
    public bool isCaptureMove;


    public const int Standard = 1;
    public const int PawnTwoSquares = 2;
    public const int Castling = 3;
    public const int EnPassant = 4;


    public Move(int flag, int index, int newIndex, int pieceNumber, bool isCapture)
    {
        moveType = flag;
        startIndex = index;
        endIndex = newIndex;
        pieceID = pieceNumber;
        isCaptureMove = isCapture;
    }

    public string GetMoveAsString()
    {
        string moveString = "";

        if (moveType == Standard || moveType == PawnTwoSquares || moveType == EnPassant)
        {
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
