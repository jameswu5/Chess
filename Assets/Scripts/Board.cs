using System;
using System.Collections;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const string startFENPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string testFENPosition = "8/8/8/8/2n5/8/8/8 w - - 0 1";
    public const string testEnPassantFEN = "rnbqkbnr/ppp1p1pp/8/8/3pPp2/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
    public const string testPromotionFEN = "8/4PP2/8/3k1K2/8/8/3pp3/8 w - - 0 1";
    public const string stalemateFEN = "8/8/8/8/8/8/q5k1/5K3 w - - 0 1";
    public const string moveGenerationTestFEN = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";
    public const string moveGenerationTestFEN2 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/P7/1PP1NnPP/RNBQK2R b KQ - 1 8";
    public const string moveGenerationTestFEN3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 1 1";
    public const string fiftyMoveRuleFEN = "8/8/r2k5/8/8/4K3/8/8 w - - 86 64";
    public const string insufficientMaterialFEN = "8/8/8/8/k7/8/6Kp/8 w - - 0 1";
    public const string checkmateFEN = "rnbqkbnr/pppppppp/8/8/8/8/8/4K3 w kq - 0 1";

    public UI boardUI;
    public int[] boardState;

    public static int[] Directions = { -8, 1, 8, -1, -7, 9, 7, -9 };
    public int turn;
    public bool[] castlingRights; // W kingside, W queenside, B kingside, B queenside

    public int[] kingIndices;
    public bool inCheck;

    public int fiftyMoveCounter;
    public int moveNumber;

    public int inPromotionScreen;

    public List<int> gameMoves;
    public List<string> boardPositions;
    Dictionary<string, int> boardStrings;

    public enum Result { Playing, Checkmate, Stalemate, Insufficient, Threefold, FiftyMove };

    public Result gameResult;

    public void Initialise()
    {

        boardState = new int[64];
        inPromotionScreen = -1;
        kingIndices = new int[2];
        boardPositions = new List<string>();
        boardStrings = new Dictionary<string, int>();
        gameMoves = new List<int>();
        name = "Board";

        GenerateBoardStateFromFEN();

        boardUI.CreateUI(boardState);
    }


    private void GenerateBoardStateFromFEN(string FENPosition = startFENPosition) {
        Dictionary<char, int> pieceTypes = new Dictionary<char, int>() {
            {'K', Piece.King},
            {'Q', Piece.Queen},
            {'B', Piece.Bishop},
            {'N', Piece.Knight},
            {'R', Piece.Rook},
            {'P', Piece.Pawn},
        };

        int rank = 7;
        int file = 0;

        string[] sections = FENPosition.Split(' ');

        // first section is the board state
        string boardInformation = sections[0];
        foreach (char c in boardInformation) {
            if (c == '/') {
                rank--;
                file = 0;
            } else if (c >= '0' && c <= '8') {
                file += c - '0';
            } else {
                int pieceColour = char.IsUpper(c) ? Piece.White : Piece.Black;
                int pieceType = pieceTypes[char.ToUpper(c)];
                int index = rank * 8 + file;

                int pieceID = pieceColour + pieceType;
                boardState[index] = pieceID;

                if (pieceType == Piece.King)
                {
                    UpdateKingIndex(pieceColour, index);
                }

                file++;
            }
        }

        // Second section determines whose turn it is to move
        turn = sections[1] == "w" ? Piece.White : Piece.Black;


        // Castling Rights
        foreach (char c in sections[2]) {
            switch (c) {
                case 'K':
                    ChangeCastlingRight(true, true, true);
                    break;
                case 'Q':
                    ChangeCastlingRight(true, false, true);
                    break;
                case 'k':
                    ChangeCastlingRight(false, true, true);
                    break;
                case 'q':
                    ChangeCastlingRight(false, false, true);
                    break;
                default:
                    break;
            }
        }


        // en passant targets

        if (sections[3] != "-")
        {
            int targetSquareIndex = Square.GetIndexFromSquareName(sections[3]);

            int move;

            if (sections[3][1] == '6')
            {
                move = Move.Initialise(Move.PawnTwoSquares, targetSquareIndex + 8, targetSquareIndex - 8, Piece.Pawn, Piece.Pawn);
            }
            else
            {
                move = Move.Initialise(Move.PawnTwoSquares, targetSquareIndex - 8, targetSquareIndex + 8, Piece.Pawn, Piece.Pawn);

            }

            move = Move.SetCastlingRights(move, new bool[4]);
            move = Move.SetFiftyMoveCounter(move, 0);

            gameMoves.Add(move);
        }

        // halfmove clock
        fiftyMoveCounter = Convert.ToInt16(sections[4]);

        // fullmove clock
        moveNumber = Convert.ToInt16(sections[5]) - 1;


        //gameResult = GetGameResult();
        //Game.UpdateEndOfGameScreen(gameResult, turn);

    }


    private string GetFENStringFromGameState()
    {
        StringBuilder sb = new();

        // board state
        for (int r = 7; r > -1; r--)
        {
            int emptyCounter = 0;
            for (int c = 0; c < 8; c++)
            {
                int index = r * 8 + c;
                if (boardState[index] == Piece.None)
                {
                    emptyCounter++;
                }
                else
                {
                    if (emptyCounter > 0)
                    {
                        sb.Append(emptyCounter);
                    }
                    sb.Append(Piece.GetCharacterFromPieceType(boardState[index]));
                    emptyCounter = 0;
                }
            }

            if (emptyCounter > 0)
            {
                sb.Append(emptyCounter);
            }
            sb.Append('/');
        }

        sb.Remove(sb.Length - 1, 1);

        sb.Append(' ');

        // turn
        string turnString = turn == Piece.White ? "w" : "b";
        sb.Append(turnString);

        sb.Append(' ');

        // castling rights
        StringBuilder castlingStringBuilder = new();
        if (castlingRights[0])
        {
            castlingStringBuilder.Append("K");
        }
        if (castlingRights[1])
        {
            castlingStringBuilder.Append("Q");
        }
        if (castlingRights[2])
        {
            castlingStringBuilder.Append("k");
        }
        if (castlingRights[3])
        {
            castlingStringBuilder.Append("q");
        }

        string castlingString = castlingStringBuilder.ToString();
        castlingString = castlingString.Length == 0 ? "-" : castlingString;
        sb.Append(castlingString);

        sb.Append(' ');

        // en passant targets
        string enPassantTargetString = "-";
        if (gameMoves.Count > 0)
        {
            int lastMove = gameMoves[^1];
            if (Move.GetMoveType(lastMove) == Move.PawnTwoSquares)
            {
                int offset = Square.GetRank(Move.GetStartIndex(lastMove)) == 2 ? 8 : -8;
                enPassantTargetString = Square.ConvertIndexToSquareName(Move.GetStartIndex(lastMove) + offset);
            }
        }
        sb.Append(enPassantTargetString);
        sb.Append(' ');

        // halfmove clock
        sb.Append(fiftyMoveCounter);
        sb.Append(' ');

        // fullmove clock
        sb.Append(moveNumber);

        return sb.ToString();
    }

    private string GetFENStringFromBoardState()
    {
        string FENString = GetFENStringFromGameState();
        string[] FENStringAsArray = FENString.Split(' ');
        return FENStringAsArray[0];
    }

    public void DragPiece(int index, Vector2 mousePos, float dragOffset)
    {
        boardUI.DragPiece(index, mousePos, dragOffset);
    }

    public void ResetPiecePosition(int index)
    {
        // move the sprite to the square it's already at

        boardUI.MovePieceToSquare(index, index);
    }

    public void TryToPlacePiece(int index, int newIndex, int promotionType = -1) // tries to place a piece in a new square
    {
        // promotionType = -1 if the move isn't a promotion, otherwise it is Piece.[promotionPiece]

        int tryMove = TryToGetMove(index, newIndex, promotionType);

        if (tryMove != 0)
        {
            int move = tryMove;
            PlayMove(move);

            int moveType = Move.GetMoveType(move);
            if (moveType == Move.PromoteToQueen || moveType == Move.PromoteToRook || moveType == Move.PromoteToBishop || moveType == Move.PromoteToKnight)
            {
                DisablePromotionScreen();
            }
        }
        else
        {
            ResetPiecePosition(index);
        }
    }

    public int TryToGetMove(int index, int newIndex, int promotionType)
    {
        foreach (int move in GetLegalMoves(index))
        {
            if (Move.GetEndIndex(move) == newIndex)
            {
                int moveType = Move.GetMoveType(move);
                switch (promotionType)
                {
                    case -1:
                        return move;
                    case Piece.Queen:
                        if (moveType == Move.PromoteToQueen)
                            return move;
                        break;
                    case Piece.Rook:
                        if (moveType == Move.PromoteToRook)
                            return move;
                        break;
                    case Piece.Bishop:
                        if (moveType == Move.PromoteToBishop)
                            return move;
                        break;
                    case Piece.Knight:
                        if (moveType == Move.PromoteToKnight)
                            return move;
                        break;
                    default:
                        Debug.Log($"Cannot find move with promotionType {promotionType}");
                        break;
                }
            }
        }

        return 0; // null move
    }

    public void PlacePiece(int index, int newIndex, bool changeUI)
    {
        int selectedPiece = boardState[index];
        boardState[index] = Piece.None;
        
        if (boardState[newIndex] != Piece.None)
        {
            DestroyPiece(newIndex, changeUI);
        }

        boardState[newIndex] = selectedPiece;

        if (changeUI)
        {
            boardUI.MovePieceToSquare(index, newIndex);
        }
    }

    public void DestroyPiece(int index, bool changeUI)
    {
        if (changeUI) boardUI.DestroyPieceSprite(index);
        boardState[index] = Piece.None;
    }

    //////////////////
    // Moving rules //
    //////////////////

    private HashSet<int> GetPseudoLegalMoves(int index) // These are actually only pseudolegal
    {
        int pieceID = boardState[index];
        HashSet<int> legalMoves = new();

        switch (pieceID % 8)
        {
            case Piece.King:
                legalMoves = KingMoves(index);
                break;

            case Piece.Queen:
                legalMoves = SlideMoves(index, Directions, Piece.Queen);
                break;

            case Piece.Bishop:
                legalMoves = SlideMoves(index, Directions[4..], Piece.Bishop);
                break;

            case Piece.Knight:
                legalMoves = KnightMoves(index);
                break;

            case Piece.Rook:
                legalMoves = SlideMoves(index, Directions[0..4], Piece.Rook);
                break;

            case Piece.Pawn:
                legalMoves = PawnMoves(index);
                break;

            default:
                Debug.Log($"Piece at square index {index} cannot be found!");
                break;
        }

        return legalMoves;
    }

    private HashSet<int> SlideMoves(int index, IEnumerable<int> offsets, int pieceNumber)
    {
        HashSet<int> legalMoves = new();

        foreach (int offset in offsets)
        {
            int currentSquareIndex = index;
            while (!CheckIfAtEdge(currentSquareIndex, offset))
            {
                currentSquareIndex += offset;
                if (currentSquareIndex >= 0 && currentSquareIndex < 64)
                {
                    if (boardState[currentSquareIndex] == Piece.None)
                    {
                        legalMoves.Add(Move.Initialise(Move.Standard, index, currentSquareIndex, pieceNumber, Piece.None));
                    }
                    else
                    {
                        if (Piece.IsWhite(boardState[currentSquareIndex]) != Piece.IsWhite(boardState[index])) // different colour so can capture
                        {
                            int capturedPieceType = GetPieceTypeAtIndex(currentSquareIndex);

                            legalMoves.Add(Move.Initialise(Move.Standard, index, currentSquareIndex, pieceNumber, capturedPieceType));
                        }

                        break;
                    }
                }
            }

        }

        return legalMoves;
    }

    private HashSet<int> KingMoves(int index)
    {
        HashSet<int> legalMoves = new();

        bool pieceIsWhite = Piece.IsWhite(boardState[index]);

        foreach (int offset in Directions)
        {
            if (!CheckIfAtEdge(index, offset))
            {
                int newIndex = index + offset;
                if (newIndex >= 0 && newIndex < 64)
                {
                    if (boardState[newIndex] == Piece.None)
                    {
                        legalMoves.Add(Move.Initialise(Move.Standard, index, newIndex, Piece.King, Piece.None));
                    }
                    else if (Piece.IsWhite(boardState[newIndex]) != pieceIsWhite)
                    {
                        int capturedPieceType = GetPieceTypeAtIndex(newIndex);
                        legalMoves.Add(Move.Initialise(Move.Standard, index, newIndex, Piece.King, capturedPieceType));

                    }
                }
            }

            // Castling

            if (pieceIsWhite && index == 4) // king is in original position
            {
                if (castlingRights[0] == true && boardState[7] != Piece.None && boardState[7] == Piece.White + Piece.Rook
                    && boardState[5] == Piece.None && boardState[6] == Piece.None)
                {
                    // can castle kingside
                    legalMoves.Add(Move.Initialise(Move.Castling, index, index + 2, Piece.King, Piece.None));
                }
                if (castlingRights[1] == true && boardState[0] != Piece.None && boardState[0] == Piece.White + Piece.Rook
                    && boardState[1] == Piece.None && boardState[2] == Piece.None && boardState[3] == Piece.None)
                {
                    // can castle queenside
                    legalMoves.Add(Move.Initialise(Move.Castling, index, index - 2, Piece.King, Piece.None));

                }
            }
            else if (!pieceIsWhite && index == 60)
            {
                if (castlingRights[2] == true && boardState[63] != Piece.None && boardState[63] == Piece.Black + Piece.Rook
                    && boardState[61] == Piece.None && boardState[62] == Piece.None)
                {
                    legalMoves.Add(Move.Initialise(Move.Castling, index, index + 2, Piece.King, Piece.None));

                }
                if (castlingRights[3] == true && boardState[56] != Piece.None && boardState[56] == Piece.Black + Piece.Rook
                    && boardState[57] == Piece.None && boardState[58] == Piece.None && boardState[59] == Piece.None)
                {
                    legalMoves.Add(Move.Initialise(Move.Castling, index, index - 2, Piece.King, Piece.None));
                }
            }
        }

        return legalMoves;
    }

    private HashSet<int> KnightMoves(int index)
    {
        int[] offsets = { -15, -6, 10, 17, 15, 6, -10, -17 };
        HashSet<int> legalMoves = new();
        foreach (int offset in offsets)
        {
            if (!CheckIfAtEdgeForKnight(index, offset))
            {
                int newIndex = index + offset;
                if (newIndex >= 0 && newIndex < 64)
                {
                    if (boardState[newIndex] == Piece.None)
                    {
                        legalMoves.Add(Move.Initialise(Move.Standard, index, newIndex, Piece.Knight, Piece.None));
                    }
                    else if (Piece.IsWhite(boardState[newIndex]) != Piece.IsWhite(boardState[index]))
                    {
                        int capturedPieceType = GetPieceTypeAtIndex(newIndex);
                        legalMoves.Add(Move.Initialise(Move.Standard, index, newIndex, Piece.Knight, capturedPieceType));

                    }
                }
            }
        }
        return legalMoves;
    }

    private HashSet<int> PawnMoves(int index)
    {
        HashSet<int> legalMoves = new();
        int curPieceID = boardState[index];

        int[] offsets;
        int newIndex;
        bool pieceIsWhite = Piece.IsWhite(curPieceID);

        if (pieceIsWhite)
        {
            offsets = new int[] { 8, 7, 9 };
        }
        else
        {
            offsets = new int[] { -8, -7, -9 };
        }

        // move forward one square
        newIndex = index + offsets[0];
        if (newIndex >= 0 && newIndex < 64 && boardState[newIndex] == Piece.None)
        {

            // check if newIndex is in the final rank for promotion
            // no need to check for colour because final rank uniquely determines colour

            if (Square.GetRank(newIndex) == 1 || Square.GetRank(newIndex) == 8)
            {
                legalMoves.Add(Move.Initialise(Move.PromoteToQueen, index, newIndex, Piece.Pawn, Piece.None));
                legalMoves.Add(Move.Initialise(Move.PromoteToRook, index, newIndex, Piece.Pawn, Piece.None));
                legalMoves.Add(Move.Initialise(Move.PromoteToBishop, index, newIndex, Piece.Pawn, Piece.None));
                legalMoves.Add(Move.Initialise(Move.PromoteToKnight, index, newIndex, Piece.Pawn, Piece.None));
            }
            else
            {
                legalMoves.Add(Move.Initialise(Move.Standard, index, newIndex, Piece.Pawn, Piece.None));
            }

            // Still in original rank
            if ((pieceIsWhite && Square.GetRank(index) == 2) || (!pieceIsWhite && Square.GetRank(index) == 7))
            {
                // moveforward two squares
                newIndex += offsets[0];
                if (boardState[newIndex] == Piece.None)
                {
                    legalMoves.Add(Move.Initialise(Move.PawnTwoSquares, index, newIndex, Piece.Pawn, Piece.None));
                }
            }
        }

        // captures
        for (int i = 1; i < offsets.Length; i++)
        {
            int offset = offsets[i];
            if (!CheckIfAtEdge(index, offset))
            {
                newIndex = index + offset;
                if (boardState[newIndex] != Piece.None && CheckPieceIsWhite(newIndex) != pieceIsWhite)
                {
                    int capturedPieceType = GetPieceTypeAtIndex(newIndex);

                    // Check for promotion
                    if (Square.GetRank(newIndex) == 1 || Square.GetRank(newIndex) == 8)
                    {
                        legalMoves.Add(Move.Initialise(Move.PromoteToQueen, index, newIndex, Piece.Pawn, capturedPieceType));
                        legalMoves.Add(Move.Initialise(Move.PromoteToRook, index, newIndex, Piece.Pawn, capturedPieceType));
                        legalMoves.Add(Move.Initialise(Move.PromoteToBishop, index, newIndex, Piece.Pawn, capturedPieceType));
                        legalMoves.Add(Move.Initialise(Move.PromoteToKnight, index, newIndex, Piece.Pawn, capturedPieceType));
                    }
                    else
                    {
                        legalMoves.Add(Move.Initialise(Move.Standard, index, newIndex, Piece.Pawn, capturedPieceType));
                    }
                }

                // en passant

                if (gameMoves.Count > 0)
                {
                    int previousMove = gameMoves[^1];

                    if (Move.GetMoveType(previousMove) == Move.PawnTwoSquares)
                    {
                        if (pieceIsWhite && Move.GetEndIndex(previousMove) == newIndex - 8)
                        {
                            legalMoves.Add(Move.Initialise(Move.EnPassant, index, newIndex, Piece.Pawn, Piece.Pawn));
                        }
                        else if (!pieceIsWhite && Move.GetEndIndex(previousMove) == newIndex + 8)
                        {
                            legalMoves.Add(Move.Initialise(Move.EnPassant, index, newIndex, Piece.Pawn, Piece.Pawn));
                        }
                    }
                }
            }
        }


        return legalMoves;
    }

    private bool CheckIfAtEdge(int index, int offset)
    {
        if (offset == -7 || offset == -8 || offset == -9)
        {
            if (index < 8) return true;
        }
        if (offset == -9 || offset == -1 || offset == 7)
        {
            if (index % 8 == 0) return true;
        }
        if (offset == 7 || offset == 8 || offset == 9)
        {
            if (index >= 56) return true;
        }
        if (offset == -7 || offset == 1 || offset == 9)
        {
            if (index % 8 == 7) return true;
        }
        return false;
    }

    private bool CheckIfAtEdgeForKnight(int index, int offset)
    {

        if (offset == -17 || offset == -15)
        {
            if (index < 16) return true;
        }
        if (offset == -6 || offset == 10)
        {
            if (index % 8 == 6 || index % 8 == 7) return true;
        }
        if (offset == 15 || offset == 17)
        {
            if (index >= 48) return true;
        }
        if (offset == -10 || offset == 6)
        {
            if (index % 8 == 0 || index % 8 == 1) return true;
        }
        if (offset == -10 || offset == -6)
        {
            if (index < 8) return true;
        }
        if (offset == -15 || offset == 17)
        {
            if (index % 8 == 7) return true;
        }
        if (offset == 6 || offset == 10)
        {
            if (index >= 56) return true;
        }
        if (offset == -17 || offset == 15)
        {
            if (index % 8 == 0) return true;
        }

        return false;
    }

    public void MakeMove(int move, bool changeUI = false)
    {
        int startIndex = Move.GetStartIndex(move);
        int endIndex = Move.GetEndIndex(move);
        int moveType = Move.GetMoveType(move);
        int movedPieceType = Move.GetMovedPieceType(move);

        bool isWhite = CheckPieceIsWhite(startIndex);
        bool[] disabledCastlingRights = new bool[4];

        if (moveType == Move.Standard || moveType == Move.PawnTwoSquares)
        {

            // if piece is a king, then disable both castling rights
            if (GetPieceTypeAtIndex(startIndex) == Piece.King)
            {
                if (CheckPieceIsWhite(startIndex))
                {
                    disabledCastlingRights[0] = castlingRights[0];
                    disabledCastlingRights[1] = castlingRights[1];

                    ChangeCastlingRight(true, true, false); // isWhite, isKingside, value
                    ChangeCastlingRight(true, false, false);

                }
                else
                {
                    disabledCastlingRights[2] = castlingRights[2];
                    disabledCastlingRights[3] = castlingRights[3];

                    ChangeCastlingRight(false, true, false);
                    ChangeCastlingRight(false, false, false);

                }
            }

            // if piece is rook and in original position, disable castling right
            if (startIndex == 0 && GetPieceTypeAtIndex(startIndex) == Piece.Rook && CheckPieceIsWhite(startIndex))
            {
                disabledCastlingRights[1] = castlingRights[1];
                ChangeCastlingRight(true, false, false);
            }
            if (startIndex == 7 && GetPieceTypeAtIndex(startIndex) == Piece.Rook && CheckPieceIsWhite(startIndex))
            {
                disabledCastlingRights[0] = castlingRights[0];
                ChangeCastlingRight(true, true, false);
            }
            if (startIndex == 56 && GetPieceTypeAtIndex(startIndex) == Piece.Rook && !CheckPieceIsWhite(startIndex))
            {
                disabledCastlingRights[3] = castlingRights[3];
                ChangeCastlingRight(false, false, false);
            }
            if (startIndex == 63 && GetPieceTypeAtIndex(startIndex) == Piece.Rook && !CheckPieceIsWhite(startIndex))
            {
                disabledCastlingRights[2] = castlingRights[2];
                ChangeCastlingRight(false, true, false);
            }

            PlacePiece(startIndex, endIndex, changeUI);
        }

        if (moveType == Move.Castling)
        {
            // move the king
            PlacePiece(startIndex, endIndex, changeUI);

            // move the rook
            if (endIndex > startIndex) // kingside
            {
                PlacePiece(startIndex + 3, startIndex + 1, changeUI);
            }
            else // queenside
            {
                PlacePiece(startIndex - 4, startIndex - 1, changeUI);
            }

            // Disable castling rights
            if (isWhite)
            {
                disabledCastlingRights[0] = castlingRights[0];
                disabledCastlingRights[1] = castlingRights[1];

                ChangeCastlingRight(true, true, false);
                ChangeCastlingRight(true, false, false);
            }
            else
            {
                disabledCastlingRights[2] = castlingRights[2];
                disabledCastlingRights[3] = castlingRights[3];

                ChangeCastlingRight(false, true, false);
                ChangeCastlingRight(false, false, false);

            }
        }

        if (moveType == Move.EnPassant)
        {
            // move the pawn
            PlacePiece(startIndex, endIndex, changeUI);

            // destroy the piece next to it
            if (isWhite)
            {
                DestroyPiece(endIndex - 8, changeUI);
            }
            else
            {
                DestroyPiece(endIndex + 8, changeUI);
            }
        }

        if (moveType == Move.PromoteToQueen || moveType == Move.PromoteToRook || moveType == Move.PromoteToBishop || moveType == Move.PromoteToKnight)
        {
            PlacePiece(startIndex, endIndex, changeUI);
            DestroyPiece(endIndex, changeUI);

            int colourOfPiece = Square.GetRank(endIndex) == 8 ? Piece.White : Piece.Black;

            int promotePiece = -1;

            switch (moveType)
            {
                case Move.PromoteToQueen:
                    promotePiece = Piece.Queen;
                    boardState[endIndex] = colourOfPiece + Piece.Queen;
                    break;
                case Move.PromoteToRook:
                    promotePiece = Piece.Rook;
                    boardState[endIndex] = colourOfPiece + Piece.Rook;
                    break;
                case Move.PromoteToBishop:
                    promotePiece = Piece.Bishop;
                    boardState[endIndex] = colourOfPiece + Piece.Bishop;
                    break;
                case Move.PromoteToKnight:
                    promotePiece = Piece.Knight;
                    boardState[endIndex] = colourOfPiece + Piece.Knight;
                    break;
                default:
                    Debug.Log("A problem has occurred with promoting, cannot find promotion piece.");
                    break;
            }
            if (changeUI)
            {
                boardUI.CreatePiece(colourOfPiece + promotePiece, endIndex);
            }
        }

        // Update position of the king
        if (movedPieceType == Piece.King)
        {
            if (isWhite)
            {
                UpdateKingIndex(Piece.White, endIndex);
            }
            else
            {
                UpdateKingIndex(Piece.Black, endIndex);
            }
        }

        int previousFiftyMoveCounter = fiftyMoveCounter;

        // Update fifty move counter
        if (Move.IsCaptureMove(move) || movedPieceType == Piece.Pawn)
        {
            fiftyMoveCounter = 0;
        }
        fiftyMoveCounter++;

        // Update move number
        if (turn == Piece.White)
        {
            moveNumber++;
        }

        // Get updated game state as FEN
        string boardString = GetFENStringFromBoardState();
        if (boardStrings.ContainsKey(boardString)) {
            boardStrings[boardString]++;
        } else
        {
            boardStrings.Add(boardString, 1);
        }

        move = Move.SetCastlingRights(move, disabledCastlingRights);
        move = Move.SetFiftyMoveCounter(move, previousFiftyMoveCounter);

        boardPositions.Add(boardString);
        gameMoves.Add(move);

        ChangeTurn();
        HandleCheck();
    }

    public void PlayMove(int move)
    {
        MakeMove(move, true);
        Game.PlayMoveSound(Move.IsCaptureMove(move));
        Debug.Log($"{moveNumber}: {Move.GetMoveAsString(move)}");

        gameResult = GetGameResult();
        Game.UpdateEndOfGameScreen(gameResult, turn);

        //Debug.Log(Evaluator.EvaluateBoard(this));
    }

    public bool CheckIfPieceIsColour(int index, int colour)
    {
        return (CheckPieceIsWhite(index) && colour == Piece.White) || (!CheckPieceIsWhite(index) && colour == Piece.Black);
    }

    private void ChangeTurn()
    {
        turn = turn == Piece.White ? Piece.Black : Piece.White;
    }

    private void ChangeCastlingRight(bool isWhite, bool isKingside, bool value)
    {
        if (isWhite)
        {
            if (isKingside)
            {
                castlingRights[0] = value;
            }
            else
            {
                castlingRights[1] = value;
            }
        }
        else
        {
            if (isKingside)
            {
                castlingRights[2] = value;
            }
            else
            {
                castlingRights[3] = value;
            }
        }
    }

    public int GetPieceTypeAtIndex(int index)
    {
        return boardState[index] % 8;
        
    }

    public bool CheckPieceIsWhite(int index)
    {
        return Piece.IsWhite(boardState[index]);
    }

    public void EnablePromotionScreen(int index)
    {
        boardUI.EnablePromotionScreen(index);
        inPromotionScreen = index;
    }

    public void DisablePromotionScreen()
    {
        boardUI.DisablePromotionScreen();
        inPromotionScreen = -1;
    }

    public bool CheckNeedForPromotion(int index, int newIndex)
    {
        return (Square.GetRank(newIndex) == 1 || Square.GetRank(newIndex) == 8) && GetPieceTypeAtIndex(index) == Piece.Pawn;
    }

    public bool CheckPieceCanMoveThere(int index, int newIndex)
    {
        HashSet<int> moves = GetPseudoLegalMoves(index);
        foreach (int move in moves)
        {
            if (Move.GetEndIndex(move) == newIndex)
            {
                return true;
            }
        }
        return false;
    }

    // Checks //

    public HashSet<int> GetAllPseudoLegalMoves(int colour)
    {
        HashSet<int> pseudoLegalMoves = new();
        for (int i = 0; i < 64; i++)
        {
            if (boardState[i] != Piece.None && CheckIfPieceIsColour(i, colour))
            {
                HashSet<int> moves = GetPseudoLegalMoves(i);
                pseudoLegalMoves.UnionWith(moves);
            }
        }
        return pseudoLegalMoves;
    }

    private HashSet<int> FindCoverageOfColour(int colour)
    {
        HashSet<int> pseudoLegalMoves = GetAllPseudoLegalMoves(colour);

        HashSet<int> coverage = new();
        foreach (int move in pseudoLegalMoves)
        {
            // maybe watch out for moves like castling
            coverage.Add(Move.GetEndIndex(move));
        }

        return coverage;
    }

    private bool CheckIfPieceIsAttacked(int index)
    {
        HashSet<int> coverageOfOpponent = CheckPieceIsWhite(index) == true ? FindCoverageOfColour(Piece.Black) : FindCoverageOfColour(Piece.White);
        return coverageOfOpponent.Contains(index);
    }

    private bool CheckIfInCheck(int colour)
    {
        if (colour == Piece.White)
        {
            return CheckIfPieceIsAttacked(kingIndices[0]);

        }
        else
        {
            return CheckIfPieceIsAttacked(kingIndices[1]);

        }
    }

    private void HandleCheck()
    {
        inCheck = CheckIfInCheck(turn);
        if (inCheck)
        {
            DisplayCheck(turn);
        }

        if (turn == Piece.White)
        {
            boardUI.ResetSquareColour(kingIndices[1]);
        }
        else
        {
            boardUI.ResetSquareColour(kingIndices[0]);
        }
    }

    private void UpdateKingIndex(int colour, int newIndex)
    {
        if (colour == Piece.White)
        {
            kingIndices[0] = newIndex;
        }
        else
        {
            kingIndices[1] = newIndex;
        }
    }

    private void DisplayCheck(int colour)
    {
        if (colour == Piece.White)
        {
            boardUI.HighlightCheck(kingIndices[0]);
        }
        else
        {
            boardUI.HighlightCheck(kingIndices[1]);

        }

    }

    // Strictly legal moves

    public HashSet<int> GetAllLegalMoves(int colour)
    {
        HashSet<int> pseudoLegalMoves = GetAllPseudoLegalMoves(colour);

        HashSet<int> legalMoves = new();

        HashSet<int> castlingMoves = new();

        bool kingside = false;
        bool queenside = false;

        int homeRank = colour == Piece.White ? 0 : 7;

        foreach (int move in pseudoLegalMoves)
        {
            int endIndex = Move.GetEndIndex(move);

            // doesn't prevent castling into check!

            if (Move.GetMoveType(move) == Move.Castling) // hold the castling moves for later: an optimisation
            {
                if (!inCheck) // don't need to consider if in check: it's not allowed
                {
                    castlingMoves.Add(move);
                }

                continue;
            }

            MakeMove(move);
            if (CheckIfInCheck(colour) == false)
            {
                legalMoves.Add(move);
                if (Move.GetMovedPieceType(move) == Piece.King) // find whether king can move to the square castling would pass through
                {
                    if (endIndex == homeRank * 8 + 5)
                    {
                        kingside = true;
                    }
                    if (endIndex == homeRank * 8 + 3)
                    {
                        queenside = true;
                    }
                }
            }
            UndoMove();
        }

        foreach (int move in castlingMoves)
        {
            int endIndex = Move.GetEndIndex(move);

            if (endIndex % 8 == 6 && kingside)
            {
                MakeMove(move);
                if (CheckIfInCheck(colour) == false) {
                    legalMoves.Add(move);
                }
                UndoMove();

            }
            if (endIndex % 8 == 2 && queenside)
            {
                MakeMove(move);
                if (CheckIfInCheck(colour) == false)
                {
                    legalMoves.Add(move);
                }
                UndoMove();
            }
        }


        return legalMoves;

        // return pseudoLegalMoves;
    }

    public HashSet<int> GetLegalMoves(int index)
    {
        HashSet<int> allLegalMoves = CheckPieceIsWhite(index) ? GetAllLegalMoves(Piece.White) : GetAllLegalMoves(Piece.Black);
        HashSet<int> legalMoves = new();

        foreach (int move in allLegalMoves)
        {

            if (Move.GetStartIndex(move) == index)
            {
                legalMoves.Add(move);
            }
        }

        return legalMoves;

    }

    public void UndoMove(bool changeUI = false)
    {

        if (gameMoves.Count == 0) // no moves to undo so exit the function
        {
            return;
        }

        int lastMove = gameMoves[^1];
        gameMoves.RemoveAt(gameMoves.Count - 1);

        int heroColour = turn == Piece.White ? Piece.Black : Piece.White;
        int opponentColour = turn;

        int moveType = Move.GetMoveType(lastMove);
        int startIndex = Move.GetStartIndex(lastMove);
        int endIndex = Move.GetEndIndex(lastMove);
        int movedPieceType = Move.GetMovedPieceType(lastMove);
        int capturedPieceType = Move.GetCapturedPieceType(lastMove);


        int capturedPiece = -1;
        int capturedIndex = -1;


        // move the pieces

        if (moveType == Move.Standard || moveType == Move.PawnTwoSquares)
        {
            // move the piece back to its original position
            PlacePiece(endIndex, startIndex, changeUI);

            // replace captured piece if necessary
            if (capturedPieceType != Piece.None)
            {
                capturedPiece = capturedPieceType + opponentColour;
                capturedIndex = endIndex;

                boardState[endIndex] = capturedPieceType + opponentColour;
            }
            
        }

        else if (moveType == Move.EnPassant)
        {
            // Move the pawn back to its original position
            PlacePiece(endIndex, startIndex, changeUI);

            // replace the captured pawn
            int capturedPawnIndex = heroColour == Piece.White ? endIndex - 8 : endIndex + 8;

            capturedPiece = Piece.Pawn + opponentColour;
            capturedIndex = capturedPawnIndex;

            boardState[capturedPawnIndex] = Piece.Pawn + opponentColour;
        }

        else if (moveType == Move.Castling)
        {
            // Move the king back to its original position
            PlacePiece(endIndex, startIndex, changeUI);

            // Move the rook back to its original position
            switch (endIndex)
            {
                case 6:
                    PlacePiece(5, 7, changeUI);
                    break;
                case 2:
                    PlacePiece(3, 0, changeUI);
                    break;
                case 62:
                    PlacePiece(61, 63, changeUI);
                    break;
                case 58:
                    PlacePiece(59, 56, changeUI);
                    break;
                default:
                    Debug.Log("There has been a problem undoing the castling move.");
                    break;
            }
        }

        else if (moveType == Move.PromoteToQueen || moveType == Move.PromoteToRook || moveType == Move.PromoteToBishop || moveType == Move.PromoteToKnight)
        {
            // destroy the promoted piece
            DestroyPiece(endIndex, changeUI);

            // create a pawn on the start position
            if (changeUI)
            {
                boardUI.CreatePiece(Piece.Pawn + heroColour, startIndex);
            }
            boardState[startIndex] = Piece.Pawn + heroColour;


            // replace captured piece if necessary
            if (capturedPieceType != Piece.None)
            {
                capturedPiece = capturedPieceType + opponentColour;
                capturedIndex = endIndex;
                boardState[endIndex] = capturedPieceType + opponentColour;
            }
        }

        else
        {
            Debug.Log("Cannot identify the nature of the previous move.");
        }

        // replace the piece on the UI if necessary
        if (changeUI && capturedPiece != -1)
        {
            boardUI.CreatePiece(capturedPiece, capturedIndex);
        }

        // revert the castling rights

        bool[] disabledCastlingRights = Move.GetCastlingRights(lastMove);

        for (int i = 0; i < 4; i++)
        {
            if (disabledCastlingRights[i] == true)
            {
                castlingRights[i] = true;
            }
        }

        // revert the king index
        if (movedPieceType == Piece.King)
        {
            int index = heroColour == Piece.White ? 0 : 1;
            kingIndices[index] = startIndex;
        }

        // undo end of game (if applicable in the first place);
        gameResult = Result.Playing;

        // revert the fifty move counter
        fiftyMoveCounter = Move.GetFiftyMoveCounter(lastMove);

        // change the move number if undoing a move made by black
        if (turn == Piece.Black)
        {
            moveNumber--;
        }

        // remove the move from the list of seen board states
        string boardString = boardPositions[^1];
        boardStrings[boardString]--;
        if (boardStrings[boardString] == 0)
        {
            boardStrings.Remove(boardString);
        }
        boardPositions.RemoveAt(boardPositions.Count - 1);

        // remove end of game text if necessary
        Game.UpdateEndOfGameScreen(gameResult, turn);


        // change the turn back
        ChangeTurn();
        HandleCheck();

        //Debug.Log($"Move undone: {lastMove.GetMoveAsString()}");
    }

    public Result GetGameResult()
    {
        HashSet<int> legalMoves = GetAllLegalMoves(turn);
        if (legalMoves.Count == 0)
        {
            return inCheck ? Result.Checkmate : Result.Stalemate;
        }

        if (fiftyMoveCounter >= 100)
        {
            return Result.FiftyMove;
        }

        if (boardStrings.Count > 0 && boardStrings.Values.Max() >= 3)
        {
            return Result.Threefold;
        }

        if (CheckForInsufficientMaterial())
        {
            return Result.Insufficient;
        }

        return Result.Playing;
    }

    private bool CheckForInsufficientMaterial()
    {
        int numOfPieces = 0;
        for (int i = 0; i < 64; i++)
        {
            if (boardState[i] != Piece.None)
            {
                numOfPieces++;
                int pieceType = GetPieceTypeAtIndex(i);
                if (numOfPieces > 3)
                {
                    return false;
                }
                else if (pieceType == Piece.Pawn || pieceType == Piece.Rook || pieceType == Piece.Queen)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void ResetBoard()
    {
        boardUI.ResetBoardUI();
        Initialise();
    }

    // Testing and searching

    private int MoveGenerationTest(int depth)
    {
        if (depth == 0)
        {
            return 1;
        }

        HashSet<int> legalMoves = GetAllLegalMoves(turn);
        int numOfPositions = 0;

        foreach (int move in legalMoves)
        {
            MakeMove(move);
            numOfPositions += MoveGenerationTest(depth - 1);
            UndoMove();
        }

        return numOfPositions;
    }

    public int GetPieceAtIndex(int index)
    {
        return boardState[index];
    }
}