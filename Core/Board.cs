
namespace Chess.Core;

public class Board
{
    public int[] boardState;

    private MoveGenerator mg;
    public List<int> legalMoves;
    private Stack<List<int>> moveCache;

    public int turn;
    public int GetColour(bool isWhite) => isWhite ? Piece.White : Piece.Black;
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

    public int enPassantTarget;

    private Stack<int> gameMoves;

    private Stack<State> states;

    private ulong[] pieceBitboards;  // 0: king | 1: queen | 2: bishop | 3: knight | 4: rook | 5: pawn (white +0, black +6)
    public ulong[] colourBitboards; // 0: white | 1: black
    public ulong AllPiecesBiboard => colourBitboards[0] | colourBitboards[1];

    public ulong zobristKey;
    public Dictionary<ulong, int> table;

    public Judge.Result gameResult;

    public Board()
    {
        Initialise();
    }

    public void Initialise()
    {
        // boardState = new int[64];
        // turn = -1; // not set to any value
        // LoadPosition();

        boardState = new int[64];
        mg = new MoveGenerator();
        moveCache = new Stack<List<int>>();

        enPassantTarget = -1;

        castlingRights = 0;

        kingIndices = new int[2];
        gameMoves = new Stack<int>();

        pieceBitboards = new ulong[16];
        colourBitboards = new ulong[2];

        LoadPosition();
        legalMoves = GetAllLegalMoves();
        moveCache.Push(new List<int>(legalMoves));

        zobristKey = Zobrist.CalculateKey(this);
        table = new()
        {
            [zobristKey] = 1
        };

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

    private State GetCurrentState()
    {
        return new State(fiftyMoveCounter, castlingRights, enPassantTarget, zobristKey, inCheck);
    }


    ///////////////////
    // Making moves! //
    ///////////////////

    public int TryToGetMove(int index, int newIndex, int promotionType = Piece.None)
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
                            // EnablePromotionScreen(newIndex);
                            return -1;
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
                        Console.WriteLine($"Cannot find move with promotionType {promotionType}");
                        break;
                }
            }
        }

        return 0;
    }

    private void ChangeTurn() => turn = turn == Piece.White ? Piece.Black : Piece.White;

    private void PlacePiece(int index, int newIndex)
    {
        int selectedPiece = GetPieceAtIndex(index);
        boardState[index] = Piece.None;
        
        if (boardState[newIndex] != Piece.None)
        {
            DestroyPiece(newIndex);
        }

        boardState[newIndex] = selectedPiece;
    }

    private void DestroyPiece(int index)
    {
        boardState[index] = Piece.None;
    }

    public void MakeMove(int move)
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

            PlacePiece(startIndex, endIndex);
            UpdateBitboardForMove(movedPieceType + colour, capturedPieceType + opponentColour, startIndex, endIndex);
            Zobrist.MovePiece(ref zobristKey, movedPieceType + colour, capturedPieceType + opponentColour, startIndex, endIndex);
        }

        else if (moveType == Move.Castling)
        {
            // move the king
            PlacePiece(startIndex, endIndex);
            UpdateBitboardForMove(Piece.King + colour, Piece.None, startIndex, endIndex);
            Zobrist.MovePiece(ref zobristKey, Piece.King + colour, Piece.None, startIndex, endIndex);


            // move the rook
            if (endIndex > startIndex) // kingside
            {
                PlacePiece(startIndex + 3, startIndex + 1);
                UpdateBitboardForMove(Piece.Rook + colour, Piece.None, startIndex + 3, startIndex + 1);
                Zobrist.MovePiece(ref zobristKey, Piece.Rook + colour, Piece.None, startIndex + 3, startIndex + 1);
            }
            else // queenside
            {
                PlacePiece(startIndex - 4, startIndex - 1);
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
            PlacePiece(startIndex, endIndex);
            UpdateBitboardForMove(Piece.Pawn + colour, Piece.None, startIndex, endIndex);
            Zobrist.MovePiece(ref zobristKey, Piece.Pawn + colour, Piece.None, startIndex, endIndex);

            // destroy the piece next to it
            if (isWhite)
            {
                DestroyPiece(endIndex - 8);
                ClearSquareFromBitboard(Piece.Pawn + opponentColour, endIndex - 8);
                Zobrist.TogglePiece(ref zobristKey, Piece.Pawn + opponentColour, endIndex - 8);
            }
            else
            {
                DestroyPiece(endIndex + 8);
                ClearSquareFromBitboard(Piece.Pawn + opponentColour, endIndex + 8);
                Zobrist.TogglePiece(ref zobristKey, Piece.Pawn + opponentColour, endIndex + 8);
            }
        }

        else if (moveType == Move.PromoteToQueen || moveType == Move.PromoteToRook || moveType == Move.PromoteToBishop || moveType == Move.PromoteToKnight)
        {
            PlacePiece(startIndex, endIndex);
            DestroyPiece(endIndex);

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
                    Console.WriteLine("A problem has occurred with promoting, cannot find promotion piece.");
                    break;
            }

            AddPieceToBitboard(promotePiece + colour, endIndex);
            Zobrist.TogglePiece(ref zobristKey, promotePiece + colour, endIndex);
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
    }


    public int GetPieceTypeAtIndex(int index) => boardState[index] & 0b111;

    public int GetPieceAtIndex(int index) => boardState[index];

    public bool CheckPieceIsWhite(int index) => Piece.IsColour(boardState[index], Piece.White);

    public bool CheckIfPieceIsColour(int index, int colour) => (CheckPieceIsWhite(index) && colour == Piece.White) || (!CheckPieceIsWhite(index) && colour == Piece.Black);


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



    // King stuff

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




    // Bitboards!!

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
}