using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class Move
{
    /*
    Moves are encoded as a 32-bit unsigned integer:

    [PrevFiftyMoveCounter] [ChangedCastlingRights] [MovedPiece] [CapturedPiece] [MoveType] [StartIndex] [DestinationIndex]
    [         7          ] [          4          ] [     3    ] [      3      ] [    3   ] [     6    ] [       6        ]

    */

    public int id = 0;
    public string currentFEN = "";


    // This can fit in 3 bits
    public const int Standard = 0;
    public const int PawnTwoSquares = 1;
    public const int Castling = 2;
    public const int EnPassant = 3;
    public const int PromoteToQueen = 4;
    public const int PromoteToRook = 5;
    public const int PromoteToBishop = 6;
    public const int PromoteToKnight = 7;

    private const int EndIndexShift = 0;
    private const int StartIndexShift = 6;
    private const int MoveTypeShift = 12;
    private const int CapturedPieceShift = 15;
    private const int MovedPieceShift = 18;
    private const int CastlingRightsShift = 21;
    private const int FiftyMoveCounterShift = 25;

    private const int StartIndexMask = 0b111111 << StartIndexShift;
    private const int EndIndexMask = 0b111111 << EndIndexShift;
    private const int MoveTypeMask = 0b111 << MoveTypeShift;
    private const int MovedPieceMask = 0b111 << MovedPieceShift;
    private const int CapturedPieceMask = 0b111 << CapturedPieceShift;
    private const int CastlingRightsMask = 0b1111 << CastlingRightsShift;
    private const int FiftyMoveCounterMask = 0b1111111 << FiftyMoveCounterShift;

    public Move(int moveType, int startIndex, int endIndex, int pieceType, int capturedPieceType)
    {

        id |= endIndex << EndIndexShift;
        id |= startIndex << StartIndexShift;
        id |= moveType << MoveTypeShift;
        id |= capturedPieceType << CapturedPieceShift;
        id |= pieceType << MovedPieceShift;
        // Castling rights
        // previous fifty move counter

    }

    public int GetMoveType()
    {
        return (id & MoveTypeMask) >> MoveTypeShift;
    }

    public int GetMovedPieceType()
    {
        return (id & MovedPieceMask) >> MovedPieceShift;
    }

    public int GetCapturedPieceType()
    {
        return (id & CapturedPieceMask) >> CapturedPieceShift;
    }

    public bool IsCaptureMove()
    {
        return GetCapturedPieceType() != Piece.None;
    }

    public int GetStartIndex()
    {
        return (id & StartIndexMask) >> StartIndexShift;
    }

    public int GetEndIndex()
    {
        return (id & EndIndexMask) >> EndIndexShift;
    }


    public void SetCastlingRights(bool[] change)
    {
        int cur = 0;
        for (int i = 3; i >= 0; i--)
        {
            if (change[i])
            {
                cur += 1;
            }
            cur <<= 1;
        }
        cur >>= 1;
        id |= cur << CastlingRightsShift;
    }

    public bool[] GetCastlingRights() // This needs fixing
    {
        bool[] rights = new bool[4];
        int rightsAsInt = (id & CastlingRightsMask) >> CastlingRightsShift;

        for (int i = 0; i < 4; i++)
        {
            rights[i] = rightsAsInt % 2 != 0;
            rightsAsInt >>= 1;
        }

        return rights;
    }

    public void SetFiftyMoveCounter(int counter)
    {
        id |= counter << FiftyMoveCounterShift;
    }

    public int GetFiftyMoveCounter()
    {
        return (id & FiftyMoveCounterMask) >> FiftyMoveCounterShift;
    }


    public string GetMoveAsString()
    {

        StringBuilder sb = new();

        int startIndex = GetStartIndex();
        int endIndex = GetEndIndex();
        int moveType = GetMoveType();
        int movedPiece = GetMovedPieceType();
        int capturedPiece = GetCapturedPieceType();


        if (moveType == Standard || moveType == PawnTwoSquares || moveType == EnPassant)
        {
            switch (movedPiece)
            {
                case Piece.King:
                    sb.Append("K");
                    break;
                case Piece.Queen:
                    sb.Append("Q");
                    break;
                case Piece.Bishop:
                    sb.Append("B");
                    break;
                case Piece.Knight:
                    sb.Append("N");
                    break;
                case Piece.Rook:
                    sb.Append("R");
                    break;
                case Piece.Pawn:
                    if (capturedPiece != Piece.None)
                    {
                        sb.Append(Square.GetFileName(startIndex));
                    }
                    break;
                default:
                    Debug.Log("Cannot identify piece.");
                    break;
            }

            if (capturedPiece != Piece.None) { sb.Append("x"); }

            sb.Append(Square.ConvertIndexToSquareName(endIndex));
        }
        else if (moveType == Castling)
        {
            if (Square.GetFileName(endIndex) == "g") // Kingside
            {
                return "O-O";
            }
            else // Queenside
            {
                return "O-O-O";
            }
        }
        else if (moveType == PromoteToQueen || moveType == PromoteToRook || moveType == PromoteToBishop || moveType == PromoteToKnight)
        {
            if (capturedPiece != 0)
            {
                sb.Append(Square.GetFileName(startIndex));
                sb.Append("x");
            }
            sb.Append(Square.ConvertIndexToSquareName(endIndex));

            switch (moveType) {
                case PromoteToQueen:
                    sb.Append("Q");
                    break;
                case PromoteToRook:
                    sb.Append("R");
                    break;
                case PromoteToBishop:
                    sb.Append("B");
                    break;
                case PromoteToKnight:
                    sb.Append("N");
                    break;
                default:
                    Debug.Log("Cannot find promotion piece");
                    break;
            }

        }

        return sb.ToString();
    }
}
