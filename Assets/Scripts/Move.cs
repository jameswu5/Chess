using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public static class Move
{
    /*
    Moves are encoded as an unsigned 32-bit integer:

    [PrevFiftyMoveCounter] [ChangedCastlingRights] [MovedPiece] [CapturedPiece] [MoveType] [StartIndex] [DestinationIndex]
    [         7          ] [          4          ] [     3    ] [      3      ] [    3   ] [     6    ] [       6        ]

    */

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
    //private const int FiftyMoveCounterMask = 0b1111111 << FiftyMoveCounterShift;
    private const int FiftyMoveCounterMask = 0b1111111;

    public static int Initialise(int moveType, int startIndex, int endIndex, int pieceType, int capturedPieceType)
    {
        int move = 0;
        move |= endIndex << EndIndexShift;
        move |= startIndex << StartIndexShift;
        move |= moveType << MoveTypeShift;
        move |= capturedPieceType << CapturedPieceShift;
        move |= pieceType << MovedPieceShift;

        return move;

    }

    public static int GetMoveType(int move)
    {
        return (move & MoveTypeMask) >> MoveTypeShift;
    }

    public static int GetMovedPieceType(int move)
    {
        return (move & MovedPieceMask) >> MovedPieceShift;
    }

    public static int GetCapturedPieceType(int move)
    {
        return (move & CapturedPieceMask) >> CapturedPieceShift;
    }

    public static bool IsCaptureMove(int move)
    {
        return GetCapturedPieceType(move) != Piece.None;
    }

    public static int GetStartIndex(int move)
    {
        return (move & StartIndexMask) >> StartIndexShift;
    }

    public static int GetEndIndex(int move)
    {
        return (move & EndIndexMask) >> EndIndexShift;
    }


    public static int SetCastlingRights(int move, bool[] change)
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
        move |= cur << CastlingRightsShift;

        return move;
    }

    public static bool[] GetCastlingRights(int move)
    {
        bool[] rights = new bool[4];
        int rightsAsInt = (move & CastlingRightsMask) >> CastlingRightsShift;

        for (int i = 0; i < 4; i++)
        {
            rights[i] = rightsAsInt % 2 != 0;
            rightsAsInt >>= 1;
        }

        return rights;
    }

    public static int SetFiftyMoveCounter(int move, int counter)
    {
        move |= counter << FiftyMoveCounterShift;
        return move;
    }

    public static int GetFiftyMoveCounter(int move)
    {
        return move >> FiftyMoveCounterShift & FiftyMoveCounterMask;
    }


    public static string GetMoveAsString(int move)
    {

        StringBuilder sb = new();

        int startIndex = GetStartIndex(move);
        int endIndex = GetEndIndex(move);
        int moveType = GetMoveType(move);
        int movedPiece = GetMovedPieceType(move);
        int capturedPiece = GetCapturedPieceType(move);


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
