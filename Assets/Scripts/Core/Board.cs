using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public UI boardUI;
    private int[] boardState;

    private MoveGenerator mg;
    public List<int> legalMoves;
    private Stack<List<int>> moveCache;

    public int turn;
    private int GetColour(bool isWhite) => isWhite ? Piece.White : Piece.Black;
    private int GetOpponentColour(bool isWhite) => isWhite ? Piece.Black : Piece.White;
    public int GetColourIndex(int c) => Piece.IsColour(c, Piece.White) ? 0 : 1; // this works for colours (Piece.White) and pieceIDs.
    public int GetOpponentColourIndex(int c) => Piece.IsColour(c, Piece.White) ? 1 : 0;

    public int castlingRights;
    private const int WhiteKingsideRightMask  = 0b1000;
    private const int WhiteQueensideRightMask = 0b0100;
    private const int BlackKingsideRightMask  = 0b0010;
    private const int BlackQueensideRightMask = 0b0001;

    public int[] kingIndices;
    public bool inCheck;

    public int fiftyMoveCounter;
    public int moveNumber;

    public int inPromotionScreen;
    public int enPassantTarget;

    private Stack<int> gameMoves;

    private Stack<State> states;

    private ulong[] pieceBitboards;  // 0: king | 1: queen | 2: bishop | 3: knight | 4: rook | 5: pawn (white +0, black +6)
    public ulong[] colourBitboards; // 0: white | 1: black
    public ulong AllPiecesBiboard => colourBitboards[0] | colourBitboards[1];

    public ulong zobristKey;
    public Dictionary<ulong, int> table;

    public Judge.Result gameResult;

    public void Initialise()
    {
        boardState = new int[64];
        mg = new MoveGenerator();
        moveCache = new Stack<List<int>>();

        inPromotionScreen = -1;
        enPassantTarget = -1;

        castlingRights = 0;

        kingIndices = new int[2];
        gameMoves = new Stack<int>();
        name = "Board";

        pieceBitboards = new ulong[16];
        colourBitboards = new ulong[2];

        LoadPosition();
        boardUI.CreateUI(boardState);
        legalMoves = GetAllLegalMoves();
        moveCache.Push(new List<int>(legalMoves));

        zobristKey = Zobrist.CalculateKey(this);
        table = new Dictionary<ulong, int>();
        table[zobristKey] = 1;

        states = new Stack<State>();
        states.Push(GetCurrentState());

        gameResult = Judge.GetResult(this);
    }

    private void LoadPosition(string FENPosition = FEN.standard) {
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
                int pieceType = Piece.pieceTypes[char.ToUpper(c)];
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
                    castlingRights |= WhiteKingsideRightMask;
                    break;
                case 'Q':
                    castlingRights |= WhiteQueensideRightMask;
                    break;
                case 'k':
                    castlingRights |= BlackKingsideRightMask;
                    break;
                case 'q':
                    castlingRights |= BlackQueensideRightMask;
                    break;
                default:
                    break;
            }
        }

        // en passant targets
        enPassantTarget = sections[3] == "-" ? -1 : Square.GetIndexFromSquareName(sections[3]);

        // halfmove clock
        fiftyMoveCounter = Convert.ToInt16(sections[4]);

        // fullmove clock
        moveNumber = Convert.ToInt16(sections[5]) - 1;
    }

    private string ExportPosition()
    {
        StringBuilder sb = new();

        // board state
        for (int r = 7; r > -1; r--)
        {
            int emptyCounter = 0;
            for (int c = 0; c < 8; c++)
            {
                int index = (r << 3) + c;
                if (GetPieceAtIndex(index) == Piece.None)
                {
                    emptyCounter++;
                }
                else
                {
                    if (emptyCounter > 0)
                    {
                        sb.Append(emptyCounter);
                    }
                    sb.Append(Piece.GetCharacterFromPieceType(GetPieceAtIndex(index)));
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
        if ((castlingRights & WhiteKingsideRightMask) > 0)
        {
            castlingStringBuilder.Append("K");
        }
        if ((castlingRights & WhiteQueensideRightMask) > 0)
        {
            castlingStringBuilder.Append("Q");
        }
        if ((castlingRights & BlackKingsideRightMask) > 0)
        {
            castlingStringBuilder.Append("k");
        }
        if ((castlingRights & BlackQueensideRightMask) > 0)
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

    public int TryToPlacePiece(int index, int newIndex, int promotionType = Piece.None)
    {
        int move = TryToGetMove(index, newIndex, promotionType);

        if (move != 0)
        {
            int moveType = Move.GetMoveType(move);
            if (moveType == Move.PromoteToQueen || moveType == Move.PromoteToRook || moveType == Move.PromoteToBishop || moveType == Move.PromoteToKnight)
            {
                DisablePromotionScreen();
            }
        }
        else if (inPromotionScreen == -1)
        {
            boardUI.MovePieceToSquare(index, index);
        }

        return move;
    }

    private int TryToGetMove(int index, int newIndex, int promotionType)
    {
        foreach (int move in legalMoves)
        {
            if (Move.GetStartIndex(move) == index && Move.GetEndIndex(move) == newIndex)
            {
                int moveType = Move.GetMoveType(move);

                switch (promotionType)
                {
                    case Piece.None:
                        if (Move.IsPromotionMove(moveType))
                        {
                            EnablePromotionScreen(newIndex);
                            return 0;
                        }
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

        return 0;
    }

    private void PlacePiece(int index, int newIndex, bool changeUI)
    {
        int selectedPiece = GetPieceAtIndex(index);
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

    private void DestroyPiece(int index, bool changeUI)
    {
        if (changeUI)
        {
            boardUI.DestroyPieceSprite(index);
        }
        boardState[index] = Piece.None;
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

        int oldCastlingRights = castlingRights;
        int oldEnPassantTarget = enPassantTarget;

        // Uncolour the checked king if necessary
        if (changeUI && inCheck)
        {
            boardUI.ResetSquareColour(kingIndices[GetColourIndex(turn)]);
        }

        if (moveType == Move.Standard || moveType == Move.PawnTwoSquares)
        {
            // if piece is a king, then disable both castling rights
            if (GetPieceTypeAtIndex(startIndex) == Piece.King)
            {
                if (CheckPieceIsWhite(startIndex))
                {
                    castlingRights &= 0b0011;
                }
                else
                {
                    castlingRights &= 0b1100;
                }
            }

            PlacePiece(startIndex, endIndex, changeUI);
            UpdateBitboardForMove(movedPieceType + colour, capturedPieceType + opponentColour, startIndex, endIndex);
            Zobrist.MovePiece(ref zobristKey, movedPieceType + colour, capturedPieceType + opponentColour, startIndex, endIndex);
        }

        else if (moveType == Move.Castling)
        {
            // move the king
            PlacePiece(startIndex, endIndex, changeUI);
            UpdateBitboardForMove(Piece.King + colour, Piece.None, startIndex, endIndex);
            Zobrist.MovePiece(ref zobristKey, Piece.King + colour, Piece.None, startIndex, endIndex);


            // move the rook
            if (endIndex > startIndex) // kingside
            {
                PlacePiece(startIndex + 3, startIndex + 1, changeUI);
                UpdateBitboardForMove(Piece.Rook + colour, Piece.None, startIndex + 3, startIndex + 1);
                Zobrist.MovePiece(ref zobristKey, Piece.Rook + colour, Piece.None, startIndex + 3, startIndex + 1);
            }
            else // queenside
            {
                PlacePiece(startIndex - 4, startIndex - 1, changeUI);
                UpdateBitboardForMove(Piece.Rook + colour, Piece.None, startIndex - 4, startIndex - 1);
                Zobrist.MovePiece(ref zobristKey, Piece.Rook + colour, Piece.None, startIndex - 4, startIndex - 1);
            }

            // Disable castling rights
            if (isWhite)
            {
                castlingRights &= 0b0011;
            }
            else
            {
                castlingRights &= 0b1100;
            }
        }

        else if (moveType == Move.EnPassant)
        {
            // move the pawn
            PlacePiece(startIndex, endIndex, changeUI);
            UpdateBitboardForMove(Piece.Pawn + colour, Piece.None, startIndex, endIndex);
            Zobrist.MovePiece(ref zobristKey, Piece.Pawn + colour, Piece.None, startIndex, endIndex);

            // destroy the piece next to it
            if (isWhite)
            {
                DestroyPiece(endIndex - 8, changeUI);
                ClearSquareFromBitboard(Piece.Pawn + opponentColour, endIndex - 8);
                Zobrist.TogglePiece(ref zobristKey, Piece.Pawn + opponentColour, endIndex - 8);
            }
            else
            {
                DestroyPiece(endIndex + 8, changeUI);
                ClearSquareFromBitboard(Piece.Pawn + opponentColour, endIndex + 8);
                Zobrist.TogglePiece(ref zobristKey, Piece.Pawn + opponentColour, endIndex + 8);
            }
        }

        else if (moveType == Move.PromoteToQueen || moveType == Move.PromoteToRook || moveType == Move.PromoteToBishop || moveType == Move.PromoteToKnight)
        {
            PlacePiece(startIndex, endIndex, changeUI);
            DestroyPiece(endIndex, changeUI);

            UpdateBitboardForMove(Piece.Pawn + colour, capturedPieceType + opponentColour, startIndex, endIndex);
            ClearSquareFromBitboard(Piece.Pawn + colour, endIndex);

            Zobrist.MovePiece(ref zobristKey, Piece.Pawn + colour, capturedPieceType + opponentColour, startIndex, endIndex);
            Zobrist.TogglePiece(ref zobristKey, Piece.Pawn + colour, endIndex);

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
            Zobrist.TogglePiece(ref zobristKey, promotePiece + colour, endIndex);

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

        // Update castling rights
        if (startIndex == Square.a1 || endIndex == Square.a1)
        {
            castlingRights &= ~WhiteQueensideRightMask;
        }
        if (startIndex == Square.h1 || endIndex == Square.h1)
        {
            castlingRights &= ~WhiteKingsideRightMask;
        }
        if (startIndex == Square.a8 || endIndex == Square.a8)
        {
            castlingRights &= ~BlackQueensideRightMask;
        }
        if (startIndex == Square.h8 || endIndex == Square.h8)
        {
            castlingRights &= ~BlackKingsideRightMask;
        }

        Zobrist.ChangeCastling(ref zobristKey, oldCastlingRights, castlingRights);

        // Update en passant target
        if (moveType == Move.PawnTwoSquares)
        {
            enPassantTarget = isWhite ? endIndex - 8 : endIndex + 8;
        }
        else
        {
            enPassantTarget = -1;
        }

        Zobrist.ChangeEnPassantFile(ref zobristKey, oldEnPassantTarget, enPassantTarget);

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

        gameMoves.Push(move);

        ChangeTurn();
        Zobrist.ChangeTurn(ref zobristKey);

        legalMoves = GetAllLegalMoves();
        moveCache.Push(new List<int>(legalMoves));

        if (table.ContainsKey(zobristKey))
        {
            table[zobristKey]++;
        } else
        {
            table.Add(zobristKey, 1);
        }

        states.Push(GetCurrentState());

        if (changeUI && inCheck)
        {
            boardUI.HighlightCheck(kingIndices[GetColourIndex(turn)]);
        }
    }

    public bool CheckIfPieceIsColour(int index, int colour) => (CheckPieceIsWhite(index) && colour == Piece.White) || (!CheckPieceIsWhite(index) && colour == Piece.Black);

    private void ChangeTurn() => turn = turn == Piece.White ? Piece.Black : Piece.White;

    public bool CanCastleKingside(int colour)
    {
        int mask = colour == Piece.White ? WhiteKingsideRightMask : BlackKingsideRightMask;
        return (castlingRights & mask) > 0;
    }

    public bool CanCastleQueenside(int colour)
    {
        int mask = colour == Piece.White ? WhiteQueensideRightMask : BlackQueensideRightMask;
        return (castlingRights & mask) > 0;
    }

    public int GetPieceTypeAtIndex(int index) => boardState[index] & 0b111;

    public int GetPieceAtIndex(int index) => boardState[index];

    private bool CheckPieceIsWhite(int index) => Piece.IsColour(boardState[index], Piece.White);

    private void EnablePromotionScreen(int index)
    {
        boardUI.EnablePromotionScreen(index);
        inPromotionScreen = index;
    }

    public void DisablePromotionScreen()
    {
        boardUI.DisablePromotionScreen();
        inPromotionScreen = -1;
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

    private List<int> GetAllLegalMoves()
    {
        List<int> moves = mg.GenerateMoves(this);
        inCheck = mg.inCheck;
        return moves;
    }

    // Must be called after GetAllLegalMoves
    public List<int> GetLegalMoves(int index)
    {
        List<int> moves = new();
        foreach (int move in legalMoves)
        {
            if (Move.GetStartIndex(move) == index) moves.Add(move);
        }
        return moves;
    }

    public void UndoMove(bool changeUI = false)
    {
        if (gameMoves.Count == 0) // no moves to undo so exit the function
        {
            return;
        }

        int lastMove = gameMoves.Pop();

        int heroColour = turn == Piece.White ? Piece.Black : Piece.White;
        int opponentColour = turn;

        int moveType = Move.GetMoveType(lastMove);
        int startIndex = Move.GetStartIndex(lastMove);
        int endIndex = Move.GetEndIndex(lastMove);
        int movedPieceType = Move.GetMovedPieceType(lastMove);
        int capturedPieceType = Move.GetCapturedPieceType(lastMove);

        int capturedPiece = Piece.None;
        int capturedIndex = -1;

        if (changeUI && inCheck)
        {
            boardUI.ResetSquareColour(kingIndices[GetColourIndex(turn)]);
        }

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
            int promotedPieceID = GetPieceAtIndex(endIndex);
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
            AddPieceToBitboard(capturedPiece, capturedIndex);

            if (changeUI)
            {
                boardUI.CreatePiece(capturedPiece, capturedIndex);
            }
        }

        // revert the king index
        if (movedPieceType == Piece.King)
        {
            kingIndices[GetColourIndex(heroColour)] = startIndex;
        }

        // undo end of game (if applicable in the first place);
        gameResult = Judge.Result.Playing;

        // change the move number if undoing a move made by black
        if (turn == Piece.Black) moveNumber--;

        // remove end of game text if necessary
        Game.UpdateEndOfGameScreen(gameResult);

        // change the turn back
        ChangeTurn();

        // Retrieve legal moves
        moveCache.Pop();
        legalMoves = moveCache.Peek();

        if (table[zobristKey] == 1)
        {
            table.Remove(zobristKey);
        }
        else
        {
            table[zobristKey]--;
        }

        // Revert the states
        states.Pop();
        LoadState(states.Peek());

        if (changeUI && inCheck)
        {
            boardUI.HighlightCheck(kingIndices[GetColourIndex(turn)]);
        }
    }

    public bool CheckForInsufficientMaterial()
    {
        int numOfPieces = 0;
        for (int i = 0; i < 64; i++)
        {
            if (GetPieceAtIndex(i) != Piece.None)
            {
                numOfPieces++;
                int pieceType = GetPieceTypeAtIndex(i);
                if (numOfPieces > 3 || pieceType == Piece.Pawn || pieceType == Piece.Rook || pieceType == Piece.Queen)
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

    private State GetCurrentState()
    {
        return new State(fiftyMoveCounter, castlingRights, enPassantTarget, zobristKey, inCheck);
    }

    private void LoadState(State state)
    {
        fiftyMoveCounter = state.fiftyMoveCounter;
        castlingRights = state.castlingRights;
        enPassantTarget = state.enPassantTarget;
        zobristKey = state.zobristKey;
        inCheck = state.inCheck;
    }

    // Takes care of captures as well (since capturedPieceID can be Piece.None)
    private void UpdateBitboardForMove(int pieceID, int capturedPieceID, int startIndex, int endIndex)
    {
        Bitboard.Move(ref pieceBitboards[pieceID], startIndex, endIndex);
        Bitboard.Move(ref colourBitboards[GetColourIndex(pieceID)], startIndex, endIndex);

        if (Piece.GetPieceType(capturedPieceID) != Piece.None)
        {
            ClearSquareFromBitboard(capturedPieceID, endIndex);
        }
    }

    private void ClearSquareFromBitboard(int pieceID, int index)
    {
        Bitboard.ClearSquare(ref pieceBitboards[pieceID], index);
        Bitboard.ClearSquare(ref colourBitboards[GetColourIndex(pieceID)], index);
    }

    private void AddPieceToBitboard(int pieceID, int index)
    {
        Bitboard.SetSquare(ref pieceBitboards[pieceID], index);
        Bitboard.SetSquare(ref colourBitboards[GetColourIndex(pieceID)], index);
    }

    public ulong GetDiagonalSlidingBitboard(int colourIndex)
    {
        int offset = colourIndex == 0 ? Piece.White : Piece.Black;
        return pieceBitboards[offset + Piece.Queen] | pieceBitboards[offset + Piece.Bishop];
    }

    public ulong GetOrthogonalSlidingBitboard(int colourIndex)
    {
        int offset = colourIndex == 0 ? Piece.White : Piece.Black;
        return pieceBitboards[offset + Piece.Queen] | pieceBitboards[offset + Piece.Rook];
    }

    // Takes Piece.[piece] as argument
    public ulong GetPieceBitboard(int pieceType, int colourIndex)
    {
        return pieceBitboards[pieceType + (colourIndex << 3)];
    }
}