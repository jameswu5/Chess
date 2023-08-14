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
    public int GetColour(bool isWhite) => isWhite ? Piece.White : Piece.Black;
    public int GetOpponentColour(bool isWhite) => isWhite ? Piece.Black : Piece.White;
    public int GetColourIndex(int c) => Piece.IsWhite(c) ? 0 : 1; // this works for colours (Piece.White) and pieceIDs.
    public int GetColourIndex(bool isWhite) => isWhite ? 0 : 1;
    public int GetOpponentColourIndex(int c) => Piece.IsWhite(c) ? 1 : 0;
    public int GetOpponentColourIndex(bool isWhite) => isWhite ? 1 : 0;


    public bool[] castlingRights; // W kingside, W queenside, B kingside, B queenside

    public int[] kingIndices;
    public bool inCheck;

    public int fiftyMoveCounter;
    public Stack<int> fiftyMoveCounters;
    public int moveNumber;

    public int inPromotionScreen;
    public int enPassantTarget;
    public Stack<int> enPassantTargets;

    public Stack<int> gameMoves;
    public Stack<string> boardPositions;
    Dictionary<string, int> boardStrings;


    // Bitboards

    public ulong[] pieceBitboards;  // 0: king | 1: queen | 2: bishop | 3: knight | 4: rook | 5: pawn (white +0, black +6)
    public ulong[] colourBitboards; // 0: white | 1: black


    public enum Result { Playing, Checkmate, Stalemate, Insufficient, Threefold, FiftyMove };

    public Result gameResult;

    public void Initialise()
    {
        boardState = new int[64];

        inPromotionScreen = -1;
        enPassantTarget = -1;
        enPassantTargets = new Stack<int>();

        kingIndices = new int[2];
        boardPositions = new Stack<string>();
        boardStrings = new Dictionary<string, int>();
        fiftyMoveCounters = new Stack<int>();
        gameMoves = new Stack<int>();
        name = "Board";

        pieceBitboards = new ulong[12];
        colourBitboards = new ulong[2];

        GenerateBoardStateFromFEN();
        boardUI.CreateUI(boardState);
        gameResult = GetGameResult();
        Game.UpdateEndOfGameScreen(gameResult, turn);
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
                int index = (rank << 3) + file;

                int pieceID = pieceColour + pieceType;
                boardState[index] = pieceID;

                // update the bitboards
                AddPieceToBitboard(pieceID, index);

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

        enPassantTarget = sections[3] == "-" ? -1 : Square.GetIndexFromSquareName(sections[3]);
        enPassantTargets.Push(enPassantTarget);

        // halfmove clock
        fiftyMoveCounter = Convert.ToInt16(sections[4]);
        fiftyMoveCounters.Push(fiftyMoveCounter);

        // fullmove clock
        moveNumber = Convert.ToInt16(sections[5]) - 1;
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
                int index = (r << 3) + c;
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

        string enPassantTargetString = enPassantTarget == -1 ? "-" : Square.ConvertIndexToSquareName(enPassantTarget);
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
            boardUI.MovePieceToSquare(index, index);
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
        HashSet<int> legalMoves = null!;

        bool isWhite = CheckPieceIsWhite(index);
        ulong hero = colourBitboards[GetColourIndex(isWhite)];
        ulong opponent = colourBitboards[GetOpponentColourIndex(isWhite)];

        switch (Piece.GetPieceType(boardState[index]))
        {
            case Piece.King:
                legalMoves = MoveGenerator.GetKingMoves(index, hero, castlingRights, boardState);
                break;
            case Piece.Queen:
                legalMoves = MoveGenerator.GetSlideMoves(index, Piece.Queen, hero, opponent, boardState);
                break;
            case Piece.Bishop:
                legalMoves = MoveGenerator.GetSlideMoves(index, Piece.Bishop, hero, opponent, boardState);
                break;
            case Piece.Knight:
                legalMoves = MoveGenerator.GetKnightMoves(index, hero, boardState);
                break;
            case Piece.Rook:
                legalMoves = MoveGenerator.GetSlideMoves(index, Piece.Rook, hero, opponent, boardState);
                break;
            case Piece.Pawn:
                legalMoves = MoveGenerator.GetPawnMoves(index, GetColourIndex(isWhite), hero, opponent, boardState, enPassantTarget);
                break;
            default:
                Debug.Log($"Piece at square index {index} cannot be found!");
                break;
        }

        return legalMoves;
    }

    public void MakeMove(int move, bool changeUI = false)
    {
        int startIndex = Move.GetStartIndex(move);
        int endIndex = Move.GetEndIndex(move);
        int moveType = Move.GetMoveType(move);
        int movedPieceType = Move.GetMovedPieceType(move);
        int capturedPieceType = Move.GetCapturedPieceType(move);

        bool isWhite = CheckPieceIsWhite(startIndex);
        int colour = GetColour(isWhite);
        int opponentColour = GetOpponentColour(isWhite);

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
            if (startIndex == Square.a1 && GetPieceTypeAtIndex(startIndex) == Piece.Rook && CheckPieceIsWhite(startIndex))
            {
                disabledCastlingRights[1] = castlingRights[1];
                ChangeCastlingRight(true, false, false);
            }
            if (startIndex == Square.h1 && GetPieceTypeAtIndex(startIndex) == Piece.Rook && CheckPieceIsWhite(startIndex))
            {
                disabledCastlingRights[0] = castlingRights[0];
                ChangeCastlingRight(true, true, false);
            }
            if (startIndex == Square.a8 && GetPieceTypeAtIndex(startIndex) == Piece.Rook && !CheckPieceIsWhite(startIndex))
            {
                disabledCastlingRights[3] = castlingRights[3];
                ChangeCastlingRight(false, false, false);
            }
            if (startIndex == Square.h8 && GetPieceTypeAtIndex(startIndex) == Piece.Rook && !CheckPieceIsWhite(startIndex))
            {
                disabledCastlingRights[2] = castlingRights[2];
                ChangeCastlingRight(false, true, false);
            }

            PlacePiece(startIndex, endIndex, changeUI);
            UpdateBitboardForMove(movedPieceType + colour, capturedPieceType + opponentColour, startIndex, endIndex);
        }

        if (moveType == Move.Castling)
        {
            // move the king
            PlacePiece(startIndex, endIndex, changeUI);
            UpdateBitboardForMove(Piece.King + colour, Piece.None, startIndex, endIndex);

            // move the rook
            if (endIndex > startIndex) // kingside
            {
                PlacePiece(startIndex + 3, startIndex + 1, changeUI);
                UpdateBitboardForMove(Piece.Rook + colour, Piece.None, startIndex + 3, startIndex + 1);

            }
            else // queenside
            {
                PlacePiece(startIndex - 4, startIndex - 1, changeUI);
                UpdateBitboardForMove(Piece.Rook + colour, Piece.None, startIndex - 4, startIndex - 1);
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
            UpdateBitboardForMove(Piece.Pawn + colour, capturedPieceType + opponentColour, startIndex, endIndex);

            // destroy the piece next to it
            if (isWhite)
            {
                DestroyPiece(endIndex - 8, changeUI);
                ClearSquareFromBitboard(Piece.Pawn + opponentColour, endIndex - 8);
            }
            else
            {
                DestroyPiece(endIndex + 8, changeUI);
                ClearSquareFromBitboard(Piece.Pawn + opponentColour, endIndex + 8);
            }
        }

        if (moveType == Move.PromoteToQueen || moveType == Move.PromoteToRook || moveType == Move.PromoteToBishop || moveType == Move.PromoteToKnight)
        {
            PlacePiece(startIndex, endIndex, changeUI);
            DestroyPiece(endIndex, changeUI);

            UpdateBitboardForMove(Piece.Pawn + colour, capturedPieceType + opponentColour, startIndex, endIndex);
            ClearSquareFromBitboard(Piece.Pawn + colour, endIndex);

            int promotePiece = -1;

            switch (moveType)
            {
                case Move.PromoteToQueen:
                    promotePiece = Piece.Queen;
                    boardState[endIndex] = colour + Piece.Queen;
                    break;
                case Move.PromoteToRook:
                    promotePiece = Piece.Rook;
                    boardState[endIndex] = colour + Piece.Rook;
                    break;
                case Move.PromoteToBishop:
                    promotePiece = Piece.Bishop;
                    boardState[endIndex] = colour + Piece.Bishop;
                    break;
                case Move.PromoteToKnight:
                    promotePiece = Piece.Knight;
                    boardState[endIndex] = colour + Piece.Knight;
                    break;
                default:
                    Debug.Log("A problem has occurred with promoting, cannot find promotion piece.");
                    break;
            }

            AddPieceToBitboard(promotePiece + colour, endIndex);

            if (changeUI)
            {
                boardUI.CreatePiece(promotePiece + colour, endIndex);
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

        //int previousFiftyMoveCounter = fiftyMoveCounter;

        // Update en passant target
        if (moveType == Move.PawnTwoSquares)
        {
            enPassantTarget = isWhite ? endIndex - 8 : endIndex + 8;
        }
        else
        {
            enPassantTarget = -1;
        }
        enPassantTargets.Push(enPassantTarget);

        // Update fifty move counter
        if (Move.IsCaptureMove(move) || movedPieceType == Piece.Pawn)
        {
            fiftyMoveCounter = 0;
        }
        fiftyMoveCounter++;
        fiftyMoveCounters.Push(fiftyMoveCounter);

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

        boardPositions.Push(boardString);
        gameMoves.Push(move);

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
        return boardState[index] & 0b111;
        
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
                    if (endIndex == (homeRank << 3) + 5)
                    {
                        kingside = true;
                    }
                    if (endIndex == (homeRank << 3) + 3)
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

            if ((endIndex & 0b111) == 6 && kingside)
            {
                MakeMove(move);
                if (CheckIfInCheck(colour) == false) {
                    legalMoves.Add(move);
                }
                UndoMove();

            }
            if ((endIndex & 0b111) == 2 && queenside)
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

        int lastMove = gameMoves.Peek();
        gameMoves.Pop();

        int heroColour = turn == Piece.White ? Piece.Black : Piece.White;
        int opponentColour = turn;

        int moveType = Move.GetMoveType(lastMove);
        int startIndex = Move.GetStartIndex(lastMove);
        int endIndex = Move.GetEndIndex(lastMove);
        int movedPieceType = Move.GetMovedPieceType(lastMove);
        int capturedPieceType = Move.GetCapturedPieceType(lastMove);


        int capturedPiece = Piece.None;
        int capturedIndex = -1;


        // move the pieces

        if (moveType == Move.Standard || moveType == Move.PawnTwoSquares)
        {
            // move the piece back to its original position
            PlacePiece(endIndex, startIndex, changeUI);
            UpdateBitboardForMove(movedPieceType + heroColour, Piece.None, endIndex, startIndex);

            // replace captured piece if necessary
            if (capturedPieceType != Piece.None)
            {
                capturedPiece = capturedPieceType + opponentColour;
                capturedIndex = endIndex;
            }
            
        }

        else if (moveType == Move.EnPassant)
        {
            // Move the pawn back to its original position
            PlacePiece(endIndex, startIndex, changeUI);
            UpdateBitboardForMove(movedPieceType + heroColour, Piece.None, endIndex, startIndex);

            // replace the captured pawn
            capturedPiece = Piece.Pawn + opponentColour;
            capturedIndex = heroColour == Piece.White ? endIndex - 8 : endIndex + 8;
        }

        else if (moveType == Move.Castling)
        {
            // Move the king back to its original position
            PlacePiece(endIndex, startIndex, changeUI);
            UpdateBitboardForMove(movedPieceType + heroColour, Piece.None, endIndex, startIndex);

            // Move the rook back to its original position
            switch (endIndex)
            {
                case Square.g1:
                    PlacePiece(Square.f1, Square.h1, changeUI);
                    UpdateBitboardForMove(Piece.Rook + heroColour, Piece.None, Square.f1, Square.h1);
                    break;
                case Square.c1:
                    PlacePiece(Square.d1, Square.a1, changeUI);
                    UpdateBitboardForMove(Piece.Rook + heroColour, Piece.None, Square.d1, Square.a1);
                    break;
                case Square.g8:
                    PlacePiece(Square.f8, Square.h8, changeUI);
                    UpdateBitboardForMove(Piece.Rook + heroColour, Piece.None, Square.f8, Square.h8);
                    break;
                case Square.c8:
                    PlacePiece(Square.d8, Square.a8, changeUI);
                    UpdateBitboardForMove(Piece.Rook + heroColour, Piece.None, Square.d8, Square.a8);
                    break;
                default:
                    Debug.Log("There has been a problem undoing the castling move.");
                    break;
            }
        }

        else if (moveType == Move.PromoteToQueen || moveType == Move.PromoteToRook || moveType == Move.PromoteToBishop || moveType == Move.PromoteToKnight)
        {
            // destroy the promoted piece
            int promotedPieceID = boardState[endIndex];
            DestroyPiece(endIndex, changeUI);
            ClearSquareFromBitboard(promotedPieceID, endIndex);

            // create a pawn on the start position
            if (changeUI)
            {
                boardUI.CreatePiece(Piece.Pawn + heroColour, startIndex);
            }
            boardState[startIndex] = Piece.Pawn + heroColour;
            AddPieceToBitboard(Piece.Pawn + heroColour, startIndex);

            // replace captured piece if necessary
            if (capturedPieceType != Piece.None)
            {
                capturedPiece = capturedPieceType + opponentColour;
                capturedIndex = endIndex;
            }
        }

        else
        {
            Debug.Log("Cannot identify the nature of the previous move.");
        }

        // add the captured piece to the bitboard and replace on UI if necessary
        if (capturedPiece != Piece.None)
        {
            boardState[capturedIndex] = capturedPiece;
            AddPieceToBitboard(capturedPiece, endIndex);

            if (changeUI)
            {
                boardUI.CreatePiece(capturedPiece, capturedIndex);
            }
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

        // revert en passant target
        enPassantTargets.Pop();
        enPassantTarget = enPassantTargets.Peek();


        // revert the king index
        if (movedPieceType == Piece.King)
        {
            int index = heroColour == Piece.White ? 0 : 1;
            kingIndices[index] = startIndex;
        }

        // undo end of game (if applicable in the first place);
        gameResult = Result.Playing;

        // revert the fifty move counter
        fiftyMoveCounters.Pop();
        fiftyMoveCounter = fiftyMoveCounters.Peek();

        // change the move number if undoing a move made by black
        if (turn == Piece.Black)
        {
            moveNumber--;
        }

        // remove the move from the list of seen board states
        string boardString = boardPositions.Peek();
        boardStrings[boardString]--;
        if (boardStrings[boardString] == 0)
        {
            boardStrings.Remove(boardString);
        }
        boardPositions.Pop();

        // remove end of game text if necessary
        Game.UpdateEndOfGameScreen(gameResult, turn);


        // change the turn back
        ChangeTurn();
        HandleCheck();
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

    // Returns the pieceID of the piece at location index
    public int GetPieceAtIndex(int index)
    {
        return boardState[index];
    }


    // Bitboards


    // Takes care of captures as well (since capturedPieceID can be Piece.None)
    public void UpdateBitboardForMove(int pieceID, int capturedPieceID, int startIndex, int endIndex)
    {
        int bitboardIndex = Piece.GetBitboardIndex(pieceID);
        Bitboard.Move(ref pieceBitboards[bitboardIndex], startIndex, endIndex);
        Bitboard.Move(ref colourBitboards[GetColourIndex(pieceID)], startIndex, endIndex);

        if (Piece.GetPieceType(capturedPieceID) != Piece.None)
        {
            ClearSquareFromBitboard(capturedPieceID, endIndex);
        }
    }

    public void ClearSquareFromBitboard(int pieceID, int index)
    {
        int bitboardIndex = Piece.GetBitboardIndex(pieceID);
        Bitboard.ClearSquare(ref pieceBitboards[bitboardIndex], index);
        Bitboard.ClearSquare(ref colourBitboards[GetColourIndex(pieceID)], index);
    }

    public void AddPieceToBitboard(int pieceID, int index)
    {
        int bitboardIndex = Piece.GetBitboardIndex(pieceID);
        Bitboard.SetSquare(ref pieceBitboards[bitboardIndex], index);
        Bitboard.SetSquare(ref colourBitboards[GetColourIndex(pieceID)], index);
    }
}