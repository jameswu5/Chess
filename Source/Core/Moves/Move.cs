using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public static class Move
{
    /*
    Moves are encoded as a 32-bit integer:

    [MovedPiece] [CapturedPiece] [MoveType] [StartIndex] [DestinationIndex]
    [     3    ] [      3      ] [    3   ] [     6    ] [       6        ]
    */

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

    private const int StartIndexMask = 0b111111 << StartIndexShift;
    private const int EndIndexMask = 0b111111 << EndIndexShift;
    private const int MoveTypeMask = 0b111 << MoveTypeShift;
    private const int MovedPieceMask = 0b111 << MovedPieceShift;
    private const int CapturedPieceMask = 0b111 << CapturedPieceShift;

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

    public static int GetMoveType(int move) => (move & MoveTypeMask) >> MoveTypeShift;

    public static bool IsPromotionMove(int moveType) => (moveType & 0b100) > 0;

    public static int GetMovedPieceType(int move) => (move & MovedPieceMask) >> MovedPieceShift;

    public static int GetCapturedPieceType(int move) => (move & CapturedPieceMask) >> CapturedPieceShift;

    public static bool IsCaptureMove(int move) => GetCapturedPieceType(move) != Piece.None;

    public static int GetStartIndex(int move) => (move & StartIndexMask) >> StartIndexShift;

    public static int GetEndIndex(int move) => (move & EndIndexMask) >> EndIndexShift;

    public static int GetPromotedPieceType(int move)
    {
        switch (GetMoveType(move))
        {
            case PromoteToQueen:
                return Piece.Queen;
            case PromoteToRook:
                return Piece.Rook;
            case PromoteToBishop:
                return Piece.Bishop;
            case PromoteToKnight:
                return Piece.Knight;
            default:
                return Piece.None;
        }
    }

    public static string GetMoveAsString(int move, bool check)
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

        if (check)
        {
            sb.Append("+");
        }

        return sb.ToString();
    }

    // This is for debugging
    public static void DisplayMoveInformation(int move)
    {
        int startIndex = GetStartIndex(move);
        int endIndex = GetEndIndex(move);
        int moveType = GetMoveType(move);
        int movedPiece = GetMovedPieceType(move);
        int capturedPiece = GetCapturedPieceType(move);

        Debug.Log($"Start {Square.ConvertIndexToSquareName(startIndex)} | End {Square.ConvertIndexToSquareName(endIndex)} | MoveType {moveType} | movedPiece {movedPiece} | capturedPiece {capturedPiece}");
    }
}
