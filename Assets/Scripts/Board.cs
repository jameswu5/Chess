using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int BoardWidth = 8;
    public const int BoardHeight = 8;
    public const int NumOfSquares = 64;

    public const string startFENPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string testFENPosition = "8/8/8/8/2n5/8/8/8 w - - 0 1";
    public const string testEnPassantFEN = "rnbqkbnr/ppp1p1pp/8/8/3pPp2/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
    public const string testPromotionFEN = "8/4PP2/8/3k1K2/8/8/3pp3/8 w - - 0 1";

    public Square squarePrefab;
    public Piece piecePrefab;
    public new Camera camera;
    public AudioSource captureSound;
    public AudioSource moveSound;

    public Piece[] boardState = new Piece[NumOfSquares];
    public Square[] squares = new Square[NumOfSquares];

    public static int[] Directions = { -8, 1, 8, -1, -7, 9, 7, -9 };

    public int turn;

    public bool[] castlingRights = { false, false, false, false }; // W kingside, W queenside, B kingside, B queenside

    public List<MoveInfo> gameMoves = new();


    // Promotion UI
    public GameObject boardCover;
    public int inPromotionScreen = -1; // -1 means not in promotion, any index means the position the pawn promoting is in
    public Piece[] promotionPieces = new Piece[4];
    public Square[] promotionSquares = new Square[4];


    public int[] kingIndices = new int[2];


    public bool inCheck; // index of checked piece, -1 if not in check


    void Start()
    {
        MoveCamera();
        GenerateBoard();
        GenerateBoardStateFromFEN();
    }

    private void GenerateBoard() {
        for (int i = 0; i < 64; i++)
        {
            Square newSquare = CreateSquare(i);
            squares[i] = newSquare;
        }
    }

    private void MoveCamera() {
        camera.transform.position = new Vector3((float) BoardWidth / 2 - 0.5f, (float) BoardHeight / 2 - 0.5f, -10);
    }

    private void GenerateBoardStateFromFEN(string FENPosition = startFENPosition) {

        boardState = new Piece[NumOfSquares];

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
        
        // We will only deal with the first section for now.
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
                int index = rank * BoardHeight + file;

                int pieceID = pieceColour + pieceType;

                Piece newPiece = CreatePiece(pieceID, index);
                boardState[index] = newPiece;

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
            int targetSquareIndex = GetIndexFromSquareName(sections[3]);

            Move move;

            if (sections[3][1] == '6')
            {
                move = new Move(Move.PawnTwoSquares, targetSquareIndex + 8, targetSquareIndex - 8, Piece.Pawn, false);
            }
            else
            {
                move = new Move(Move.PawnTwoSquares, targetSquareIndex - 8, targetSquareIndex + 8, Piece.Pawn, false);

            }

            MoveInfo moveInfo = new MoveInfo(move, Piece.Pawn, new bool[4]);
            gameMoves.Add(moveInfo);
        }

        // halfmove clock is sections[4], not implemented yet

    }

    Square CreateSquare(int index, float elevation = 0)
    {
        int x = index % 8;
        int y = index / 8;

        Square spawnSquare = Instantiate(squarePrefab, new Vector3(x, y, elevation), Quaternion.identity);
        string squareName = $"{(char)(x + 97)}{y + 1}";
        bool isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
        spawnSquare.Initialise(index, squareName, isOffset);

        return spawnSquare;
    }

    Piece CreatePiece(int pieceID, int index, float elevation = -1f) { // elevation is just for layering
        int rank = index / BoardHeight;
        int file = index % BoardHeight;
        Piece spawnPiece = Instantiate(piecePrefab, new Vector3(file, rank, elevation), Quaternion.identity);
        spawnPiece.Initialise(pieceID, index);

        return spawnPiece;
    }

    public void DragPiece(int index, Vector2 mousePos, float dragOffset)
    {
        boardState[index].Drag(mousePos, dragOffset);
    }

    public void ResetPiecePosition(int index)
    {
        boardState[index].SnapToSquare(index);
    }

    public void TryToPlacePiece(int index, int newIndex, int promotionType = -1) // tries to place a piece in a new square
    {
        // promotionType = -1 if the move isn't a promotion, otherwise it is Piece.[promotionPiece]

        Move? tryMove = TryToGetMove(index, newIndex, promotionType);

        if (tryMove != null)
        {
            Move move = (Move)tryMove;
            MakeMove(move);
            if (move.moveType == Move.PromoteToQueen || move.moveType == Move.PromoteToRook || move.moveType == Move.PromoteToBishop || move.moveType == Move.PromoteToKnight)
            {
                DisablePromotionScreen();
            }
        }
        else
        {
            ResetPiecePosition(index);
        }
    }

    public Move? TryToGetMove(int index, int newIndex, int promotionType)
    {
        foreach (Move move in GetLegalMoves(index))
        {
            if (move.endIndex == newIndex)
            {
                switch (promotionType)
                {
                    case -1:
                        return move;
                    case Piece.Queen:
                        if (move.moveType == Move.PromoteToQueen)
                            return move;
                        break;
                    case Piece.Rook:
                        if (move.moveType == Move.PromoteToRook)
                            return move;
                        break;
                    case Piece.Bishop:
                        if (move.moveType == Move.PromoteToBishop)
                            return move;
                        break;
                    case Piece.Knight:
                        if (move.moveType == Move.PromoteToKnight)
                            return move;
                        break;
                    default:
                        Debug.Log($"Cannot find move with promotionType {promotionType}");
                        break;
                }
            }
        }

        return null;
    }

    public void PlacePiece(int index, int newIndex) // parameters assumed to be valid, out of bounds checked in humanInput
    {
        Piece selectedPiece = boardState[index];
        boardState[index] = null;
        
        if (boardState[newIndex] != null)
        {
            DestroyPiece(newIndex);
        }

        boardState[newIndex] = selectedPiece;
        selectedPiece.index = newIndex;
        selectedPiece.SnapToSquare(newIndex);

    }

    public void DestroyPiece(int index)
    {
        Piece selectedPiece = boardState[index];
        selectedPiece.DestroyPiece();
        boardState[index] = null;
    }

    public void HighlightSquare(int index)
    {
        squares[index].Highlight();
    }

    public void ResetSquareColour(int index)
    {
        squares[index].InitialiseColor();
    }

    public void HighlightHover(int index)
    {
        // We unhighlight every single square because we don't know which square it was on before.
        // Still technically O(1) but I'm not a fan.

        for (int i = 0; i < NumOfSquares; i++)
        {
            UnHighlightHover(i);
        }

        squares[index].SetHoverHighlight(true);
    }

    public void UnHighlightHover(int index)
    {
        squares[index].SetHoverHighlight(false);
    }

    public void HighlightOptions(IEnumerable<Move> moves)
    {
        foreach (Move move in moves)
        {
            squares[move.endIndex].SetOptionHighlight(true);
        }
    }

    public void UnHighlightOptionsAllSquares()
    {
        foreach (Square square in squares)
        {
            square.SetOptionHighlight(false);
        }
    }

    public void HighlightCheck(int index)
    {
        squares[index].HighlightCheck();
    }


    //////////////////
    // Moving rules //
    //////////////////

    public HashSet<Move> GetPseudoLegalMoves(int index) // These are actually only pseudolegal
    {
        Piece currentPiece = boardState[index];
        HashSet<Move> legalMoves = new();

        switch (currentPiece.pieceID % 8)
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

    public HashSet<Move> SlideMoves(int index, IEnumerable<int> offsets, int pieceNumber)
    {
        HashSet<Move> legalMoves = new();

        foreach (int offset in offsets)
        {
            int currentSquareIndex = index;
            while (!CheckIfAtEdge(currentSquareIndex, offset))
            {
                currentSquareIndex += offset;
                if (currentSquareIndex >= 0 && currentSquareIndex < NumOfSquares)
                {
                    if (boardState[currentSquareIndex] == null)
                    {
                        legalMoves.Add(new Move(Move.Standard, index, currentSquareIndex, pieceNumber, false));
                    }
                    else
                    {
                        if (boardState[currentSquareIndex].IsWhite() != boardState[index].IsWhite()) // different colour so can capture
                        {
                            legalMoves.Add(new Move(Move.Standard, index, currentSquareIndex, pieceNumber, true));
                        }
                        break;
                    }
                }
            }

        }

        return legalMoves;
    }

    public HashSet<Move> KingMoves(int index)
    {
        HashSet<Move> legalMoves = new();
        foreach (int offset in Directions)
        {
            if (!CheckIfAtEdge(index, offset))
            {
                int newIndex = index + offset;
                if (newIndex >= 0 && newIndex < NumOfSquares)
                {
                    if (boardState[newIndex] == null)
                    {
                        legalMoves.Add(new Move(Move.Standard, index, newIndex, Piece.King, false));
                    }
                    else if (boardState[newIndex].IsWhite() != boardState[index].IsWhite())
                    {
                        legalMoves.Add(new Move(Move.Standard, index, newIndex, Piece.King, true));

                    }
                }
            }

            // Castling

            if (boardState[index].IsWhite() && index == 4) // king is in original position
            {
                if (castlingRights[0] == true && boardState[7].pieceID == Piece.White + Piece.Rook
                    && boardState[5] == null && boardState[6] == null)
                {
                    // can castle kingside
                    legalMoves.Add(new Move(Move.Castling, index, index + 2, Piece.King, false));
                }
                if (castlingRights[1] == true && boardState[0].pieceID == Piece.White + Piece.Rook
                    && boardState[1] == null && boardState[2] == null && boardState[3] == null)
                {
                    // can castle queenside
                    legalMoves.Add(new Move(Move.Castling, index, index - 2, Piece.King, false));

                }
            }
            else if (!boardState[index].IsWhite() && index == 60)
            {
                if (castlingRights[2] == true && boardState[63].pieceID == Piece.Black + Piece.Rook
                    && boardState[61] == null && boardState[62] == null)
                {
                    legalMoves.Add(new Move(Move.Castling, index, index + 2, Piece.King, false));

                }
                if (castlingRights[3] == true && boardState[56].pieceID == Piece.Black + Piece.Rook
                    && boardState[57] == null && boardState[58] == null && boardState[59] == null)
                {
                    legalMoves.Add(new Move(Move.Castling, index, index - 2, Piece.King, false));
                }
            }
        }

        return legalMoves;
    }

    public HashSet<Move> KnightMoves(int index)
    {
        int[] offsets = { -15, -6, 10, 17, 15, 6, -10, -17 };
        HashSet<Move> legalMoves = new();
        foreach (int offset in offsets)
        {
            if (!CheckIfAtEdgeForKnight(index, offset))
            {
                int newIndex = index + offset;
                if (newIndex >= 0 && newIndex < NumOfSquares)
                {
                    if (boardState[newIndex] == null)
                    {
                        legalMoves.Add(new Move(Move.Standard, index, newIndex, Piece.Knight, false));
                    }
                    else if (boardState[newIndex].IsWhite() != boardState[index].IsWhite())
                    {
                        legalMoves.Add(new Move(Move.Standard, index, newIndex, Piece.Knight, true));

                    }
                }
            }
        }
        return legalMoves;
    }

    public HashSet<Move> PawnMoves(int index)
    {
        HashSet<Move> legalMoves = new();
        Piece curPiece = boardState[index];

        int[] offsets;
        int newIndex;

        if (curPiece.IsWhite())
        {
            offsets = new int[] { 8, 7, 9 };
        }
        else
        {
            offsets = new int[] { -8, -7, -9 };
        }

        // move forward one square
        newIndex = index + offsets[0];
        if (newIndex >= 0 && newIndex < NumOfSquares && boardState[newIndex] == null)
        {

            // check if newIndex is in the final rank for promotion
            // no need to check for colour because final rank uniquely determines colour

            if (GetRank(newIndex) == 1 || GetRank(newIndex) == 8)
            {
                legalMoves.Add(new Move(Move.PromoteToQueen, index, newIndex, Piece.Pawn, false));
                legalMoves.Add(new Move(Move.PromoteToRook, index, newIndex, Piece.Pawn, false));
                legalMoves.Add(new Move(Move.PromoteToBishop, index, newIndex, Piece.Pawn, false));
                legalMoves.Add(new Move(Move.PromoteToKnight, index, newIndex, Piece.Pawn, false));
            }
            else
            {
                legalMoves.Add(new Move(Move.Standard, index, newIndex, Piece.Pawn, false));
            }

            // Still in original rank
            if ((curPiece.IsWhite() && curPiece.GetRank() == 2) || (!curPiece.IsWhite() && curPiece.GetRank() == 7))
            {
                // moveforward two squares
                newIndex += offsets[0];
                if (boardState[newIndex] == null)
                {
                    legalMoves.Add(new Move(Move.PawnTwoSquares, index, newIndex, Piece.Pawn, false));
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
                if (boardState[newIndex] != null && boardState[newIndex].IsWhite() != curPiece.IsWhite())
                {

                    // Check for promotion
                    if (GetRank(newIndex) == 1 || GetRank(newIndex) == 8)
                    {
                        legalMoves.Add(new Move(Move.PromoteToQueen, index, newIndex, Piece.Pawn, true));
                        legalMoves.Add(new Move(Move.PromoteToRook, index, newIndex, Piece.Pawn, true));
                        legalMoves.Add(new Move(Move.PromoteToBishop, index, newIndex, Piece.Pawn, true));
                        legalMoves.Add(new Move(Move.PromoteToKnight, index, newIndex, Piece.Pawn, true));
                    }
                    else
                    {
                        legalMoves.Add(new Move(Move.Standard, index, newIndex, Piece.Pawn, true));
                    }
                }

                // en passant

                if (gameMoves.Count > 0)
                {
                    Move previousMove = gameMoves[gameMoves.Count - 1].move;

                    if (previousMove.moveType == Move.PawnTwoSquares)
                    {
                        if (curPiece.IsWhite() && previousMove.endIndex == newIndex - 8)
                        {
                            legalMoves.Add(new Move(Move.EnPassant, index, newIndex, Piece.Pawn, true));
                        }
                        else if (!curPiece.IsWhite() && previousMove.endIndex == newIndex + 8)
                        {
                            legalMoves.Add(new Move(Move.EnPassant, index, newIndex, Piece.Pawn, true));
                        }
                    }
                }
            }
        }


        return legalMoves;
    }

    public bool CheckIfAtEdge(int index, int offset)
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

    public bool CheckIfAtEdgeForKnight(int index, int offset)
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

    public void MakeMove(Move move, bool makeSound = true) // all checks assumed to be complete and this move is allowed
    {
        bool isWhite = CheckPieceIsWhite(move.startIndex);
        int capturedPieceType = GetPieceTypeAtIndex(move.endIndex);
        bool[] disabledCastlingRights = new bool[4];

        if (move.moveType == Move.Standard || move.moveType == Move.PawnTwoSquares)
        {

            // if piece is a king, then disable both castling rights
            if (GetPieceTypeAtIndex(move.startIndex) == Piece.King)
            {
                if (CheckPieceIsWhite(move.startIndex))
                {
                    ChangeCastlingRight(true, true, false); // isWhite, isKingside, value
                    ChangeCastlingRight(true, false, false);

                    disabledCastlingRights[0] = true;
                    disabledCastlingRights[1] = true;
                }
                else
                {
                    ChangeCastlingRight(false, true, false);
                    ChangeCastlingRight(false, false, false);

                    disabledCastlingRights[2] = true;
                    disabledCastlingRights[3] = true;
                }
            }

            // if piece is rook and in original position, disable castling right
            if (move.startIndex == 0 && GetPieceTypeAtIndex(move.startIndex) == Piece.Rook && CheckPieceIsWhite(move.startIndex))
            {
                ChangeCastlingRight(true, false, false);
                disabledCastlingRights[1] = true;
            }
            if (move.startIndex == 7 && GetPieceTypeAtIndex(move.startIndex) == Piece.Rook && CheckPieceIsWhite(move.startIndex))
            {
                ChangeCastlingRight(true, true, false);
                disabledCastlingRights[0] = true;
            }
            if (move.startIndex == 56 && GetPieceTypeAtIndex(move.startIndex) == Piece.Rook && !CheckPieceIsWhite(move.startIndex))
            {
                ChangeCastlingRight(false, false, false);
                disabledCastlingRights[3] = true;
            }
            if (move.startIndex == 63 && GetPieceTypeAtIndex(move.startIndex) == Piece.Rook && !CheckPieceIsWhite(move.startIndex))
            {
                ChangeCastlingRight(false, true, false);
                disabledCastlingRights[2] = true;
            }

            PlacePiece(move.startIndex, move.endIndex);
        }

        if (move.moveType == Move.Castling)
        {
            // move the king
            PlacePiece(move.startIndex, move.endIndex);

            // move the rook
            if (move.endIndex > move.startIndex) // kingside
            {
                PlacePiece(move.startIndex + 3, move.startIndex + 1);
            }
            else // queenside
            {
                PlacePiece(move.startIndex - 4, move.startIndex - 1);
            }

            // Disable castling rights
            if (isWhite)
            {
                ChangeCastlingRight(true, true, false);
                ChangeCastlingRight(true, false, false);

                disabledCastlingRights[0] = true;
                disabledCastlingRights[1] = true;
            }
            else
            {
                ChangeCastlingRight(false, true, false);
                ChangeCastlingRight(false, false, false);

                disabledCastlingRights[2] = true;
                disabledCastlingRights[3] = true;
            }
        }

        if (move.moveType == Move.EnPassant)
        {
            // move the pawn
            PlacePiece(move.startIndex, move.endIndex);

            // destroy the piece next to it
            if (isWhite)
            {
                DestroyPiece(move.endIndex - 8);
            }
            else
            {
                DestroyPiece(move.endIndex + 8);
            }

            capturedPieceType = Piece.Pawn;
        }

        if (move.moveType == Move.PromoteToQueen || move.moveType == Move.PromoteToRook || move.moveType == Move.PromoteToBishop || move.moveType == Move.PromoteToKnight)
        {
            PlacePiece(move.startIndex, move.endIndex);
            DestroyPiece(move.endIndex);

            int colourOfPiece = GetRank(move.endIndex) == 8 ? Piece.White : Piece.Black;

            switch (move.moveType)
            {
                case Move.PromoteToQueen:
                    Piece queen = CreatePiece(colourOfPiece + Piece.Queen, move.endIndex);
                    boardState[move.endIndex] = queen;
                    break;
                case Move.PromoteToRook:
                    Piece rook = CreatePiece(colourOfPiece + Piece.Rook, move.endIndex);
                    boardState[move.endIndex] = rook;
                    break;
                case Move.PromoteToBishop:
                    Piece bishop = CreatePiece(colourOfPiece + Piece.Bishop, move.endIndex);
                    boardState[move.endIndex] = bishop;
                    break;
                case Move.PromoteToKnight:
                    Piece knight = CreatePiece(colourOfPiece + Piece.Knight, move.endIndex);
                    boardState[move.endIndex] = knight;
                    break;
                default:
                    Debug.Log("A problem has occurred with promoting, cannot find promotion piece.");
                    break;
            }
        }

        if (makeSound == true)
        {
            PlayMoveSound(move.isCaptureMove);
        }

        MoveInfo moveInfo = new MoveInfo(move, capturedPieceType, disabledCastlingRights);
        gameMoves.Add(moveInfo);

        // Update position of the king
        if (move.pieceType == Piece.King)
        {
            if (isWhite)
            {
                UpdateKingIndex(Piece.White, move.endIndex);
            }
            else
            {
                UpdateKingIndex(Piece.Black, move.endIndex);
            }
        }

        
        Debug.Log(move.GetMoveAsString());

        ChangeTurn();
        HandleCheck();
    }

    public bool CheckIfPieceIsTurnColour(int index)
    {
        return (boardState[index].IsWhite() && turn == Piece.White) || (!boardState[index].IsWhite() && turn == Piece.Black);
    }

    public void ChangeTurn()
    {
        turn = turn == Piece.White ? Piece.Black : Piece.White;
    }

    public void ChangeCastlingRight(bool isWhite, bool isKingside, bool value)
    {
        if (isWhite)
        {
            if (isKingside)
            {
                castlingRights[0] = value;
                // Debug.Log($"White kingside castling set to {value}");
            }
            else
            {
                castlingRights[1] = value;
                // Debug.Log($"White queenside castling set to {value}");

            }
        }
        else
        {
            if (isKingside)
            {
                castlingRights[2] = value;
                // Debug.Log($"Black kingside castling set to {value}");

            }
            else
            {
                castlingRights[3] = value;
                // Debug.Log($"Black queenside castling set to {value}");

            }
        }
    }

    public int GetPieceTypeAtIndex(int index)
    {
        Piece piece = boardState[index];
        if (piece != null)
        {
            return boardState[index].pieceID % 8;
        }
        return -1;
    }

    public bool CheckPieceIsWhite(int index)
    {
        return boardState[index].IsWhite();
    }

    public void PlayMoveSound(bool isCapture)
    {
        if (isCapture)
        {
            captureSound.Play();
        }
        else
        {
            moveSound.Play();
        }
    }

    public int GetIndexFromSquareName(string name)
    {

        int index = 0;

        foreach (char c in name)
        {
            if ("abcdefgh".Contains(c))
            {
                index += c - 'a';
            }
            else
            {
                index += (c - '1') * 8;
            }
        }

        return index;

    }

    public int GetRank(int index)
    {
        return (index / 8) + 1;
    }

    public void SetBoardCover(bool value)
    {
        boardCover.SetActive(value);
    }

    public void EnablePromotionScreen(int index)
    {
        // make the board darker
        SetBoardCover(true);

        int colourMultiplier = GetRank(index) == 1 ? 1 : -1;
        int pieceColour = GetRank(index) == 1 ? Piece.Black : Piece.White;

        // create the pieces

        Piece queen = CreatePiece(Piece.Queen + pieceColour, index, -1.7f);
        Piece rook = CreatePiece(Piece.Rook + pieceColour, index + 8 * colourMultiplier, -1.7f);
        Piece bishop = CreatePiece(Piece.Bishop + pieceColour, index + 16 * colourMultiplier, -1.7f);
        Piece knight = CreatePiece(Piece.Knight + pieceColour, index + 24 * colourMultiplier, -1.7f);

        promotionPieces[0] = queen;
        promotionPieces[1] = rook;
        promotionPieces[2] = bishop;
        promotionPieces[3] = knight;

        // create the squares

        for (int i = 0; i < 4; i++)
        {
            promotionSquares[i] = CreateSquare(index + 8 * i * colourMultiplier, -1.6f);
        }

        inPromotionScreen = index;

    }

    public void DisablePromotionScreen()
    {
        // revert the board colour
        SetBoardCover(false);

        // remove the squares and pieces icons

        foreach (Piece piece in promotionPieces)
        {
            piece.DestroyPiece();
        }
        foreach (Square square in promotionSquares)
        {
            square.DestroySquare();
        }

        Array.Clear(promotionPieces, 0, promotionPieces.Length);
        Array.Clear(promotionSquares, 0, promotionSquares.Length);

        inPromotionScreen = -1;
    }

    public bool CheckNeedForPromotion(int index, int newIndex)
    {
        return (GetRank(newIndex) == 1 || GetRank(newIndex) == 8) && GetPieceTypeAtIndex(index) == Piece.Pawn;
    }

    public bool CheckPieceCanMoveThere(int index, int newIndex)
    {
        HashSet<Move> moves = GetPseudoLegalMoves(index);
        foreach (Move move in moves)
        {
            if (move.endIndex == newIndex)
            {
                return true;
            }
        }
        return false;
    }

    // Checks //

    public HashSet<Move> GetAllPseudoLegalMoves(int colour)
    {
        HashSet<Move> pseudoLegalMoves = new();
        for (int i = 0; i < 64; i++)
        {
            Piece piece = boardState[i];
            if (piece != null && (piece.IsWhite() && colour == Piece.White || !piece.IsWhite() && colour == Piece.Black))
            {
                HashSet<Move> moves = GetPseudoLegalMoves(i);
                pseudoLegalMoves.UnionWith(moves);
            }
        }
        return pseudoLegalMoves;
    }

    public HashSet<int> FindCoverageOfColour(int colour)
    {
        HashSet<Move> pseudoLegalMoves = GetAllPseudoLegalMoves(colour);

        HashSet<int> coverage = new();
        foreach (Move move in pseudoLegalMoves)
        {
            // maybe watch out for moves like castling
            coverage.Add(move.endIndex);
        }

        return coverage;
    }

    public bool CheckIfPieceIsAttacked(int index)
    {
        HashSet<int> coverageOfOpponent = CheckPieceIsWhite(index) == true ? FindCoverageOfColour(Piece.Black) : FindCoverageOfColour(Piece.White);
        return coverageOfOpponent.Contains(index);
    }

    public bool CheckIfInCheck(int colour)
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

    public void HandleCheck()
    {
        inCheck = CheckIfInCheck(turn);
        if (inCheck)
        {
            DisplayCheck(turn);
        }

        if (turn == Piece.White)
        {
            ResetSquareColour(kingIndices[1]);
        }
        else
        {
            ResetSquareColour(kingIndices[0]);
        }
    }

    public void UpdateKingIndex(int colour, int newIndex)
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

    public void DisplayCheck(int colour)
    {
        if (colour == Piece.White)
        {
            Debug.Log($"White is in check! {kingIndices[0]}");

            HighlightCheck(kingIndices[0]);
        }
        else
        {
            Debug.Log($"Black is in check! {kingIndices[1]}");

            HighlightCheck(kingIndices[1]);

        }

    }


    // Strictly legal moves

    public HashSet<Move> GetAllLegalMoves(int colour)
    {
        HashSet<Move> pseudoLegalMoves = GetAllPseudoLegalMoves(colour);
        HashSet<Move> legalMoves = new();

        foreach (Move move in pseudoLegalMoves)
        {
            MakeMove(move, makeSound: false);
            //Debug.Log(move.GetMoveAsString());
            if (CheckIfInCheck(colour) == false)
            {
                legalMoves.Add(move);
            }
            UndoMove();
        }

        return legalMoves;

        // return pseudoLegalMoves;
    }

    public HashSet<Move> GetLegalMoves(int index)
    {

        HashSet<Move> allLegalMoves = boardState[index].IsWhite() ? GetAllLegalMoves(Piece.White) : GetAllLegalMoves(Piece.Black);
        HashSet<Move> legalMoves = new();

        foreach (Move move in allLegalMoves)
        {

            if (move.startIndex == index)
            {
                legalMoves.Add(move);
            }
        }

        return legalMoves;

    }

    public void UndoMove()
    {
        if (gameMoves.Count == 0) // no moves to undo so exit the function
        {
            return;
        }

        MoveInfo lastMoveInfo = gameMoves[gameMoves.Count - 1];
        gameMoves.RemoveAt(gameMoves.Count - 1);

        int heroColour = turn == Piece.White ? Piece.Black : Piece.White;
        int opponentColour = turn;

        Move lastMove = lastMoveInfo.move;

        // move the pieces

        if (lastMove.moveType == Move.Standard || lastMove.moveType == Move.PawnTwoSquares)
        {
            // move the piece back to its original position
            PlacePiece(lastMove.endIndex, lastMove.startIndex);

            // replace captured piece if necessary
            if (lastMoveInfo.capturedPiece != -1)
            {
                Piece capturedPiece = CreatePiece(lastMoveInfo.capturedPiece + opponentColour, lastMove.endIndex);
                boardState[lastMove.endIndex] = capturedPiece;
            }
            
        }

        else if (lastMove.moveType == Move.EnPassant)
        {
            // Move the pawn back to its original position
            PlacePiece(lastMove.endIndex, lastMove.startIndex);

            // replace the captured pawn
            int capturedPawnIndex = heroColour == Piece.White ? lastMove.endIndex - 8 : lastMove.endIndex + 8;
            Piece capturedPiece = CreatePiece(Piece.Pawn + opponentColour, capturedPawnIndex);
            boardState[capturedPawnIndex] = capturedPiece;
        }

        else if (lastMove.moveType == Move.Castling)
        {
            // Move the king back to its original position
            PlacePiece(lastMove.endIndex, lastMove.startIndex);

            // Move the rook back to its original position
            switch (lastMove.endIndex)
            {
                case 6:
                    PlacePiece(5, 7);
                    break;
                case 2:
                    PlacePiece(3, 0);
                    break;
                case 62:
                    PlacePiece(61, 63);
                    break;
                case 58:
                    PlacePiece(59, 56);
                    break;
                default:
                    Debug.Log("There has been a problem undoing the castling move.");
                    break;
            }
        }

        else if (lastMove.moveType == Move.PromoteToQueen || lastMove.moveType == Move.PromoteToRook || lastMove.moveType == Move.PromoteToBishop || lastMove.moveType == Move.PromoteToKnight)
        {
            // destroy the promoted piece
            DestroyPiece(lastMove.endIndex);

            // create a pawn on the start position
            Piece originalPiece = CreatePiece(Piece.Pawn + heroColour, lastMove.startIndex);
            boardState[lastMove.startIndex] = originalPiece;


            // replace captured piece if necessary
            if (lastMoveInfo.capturedPiece != -1)
            {
                Piece capturedPiece = CreatePiece(lastMoveInfo.capturedPiece + opponentColour, lastMove.endIndex);
                boardState[lastMove.endIndex] = capturedPiece;

                Debug.Log($"replaced captured piece is { capturedPiece.pieceID } at index {lastMove.endIndex}");
            }
        }

        else
        {
            Debug.Log("Cannot identify the nature of the previous move.");
        }

        // revert the castling rights
        for (int i = 0; i < 4; i++)
        {
            if (lastMoveInfo.disabledCastlingRights[i] == true)
            {
                castlingRights[i] = true;
            }
        }

        // revert the king index
        if (lastMove.pieceType == Piece.King)
        {
            int index = heroColour == Piece.White ? 0 : 1;
            kingIndices[index] = lastMove.startIndex;
        }

        // change the turn back
        ChangeTurn();
        HandleCheck();

        Debug.Log($"Move undone: {lastMove.GetMoveAsString()}");
    }
}
