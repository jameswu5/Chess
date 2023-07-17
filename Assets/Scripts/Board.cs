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

    public Square squarePrefab;
    public Piece piecePrefab;
    public new Camera camera;
    public AudioSource captureSound;
    public AudioSource moveSound;

    public Piece[] boardState = new Piece[NumOfSquares];
    public Square[] squares = new Square[NumOfSquares];

    public static int[] Directions = { -8, 1, 8, -1, -7, 9, 7, -9 };

    public enum Turn
    {
        White, Black
    }

    public Turn turn;

    public enum InputState
    {
        None, Selected, Dragging
    }
    public InputState currentState = InputState.None;


    public bool[] castlingRights = { false, false, false, false }; // W kingside, W queenside, B kingside, B queenside

    public List<Move> gameMoves = new();




    void Start()
    {
        MoveCamera();
        GenerateBoard();
        GenerateBoardStateFromFEN();
    }


    private void GenerateBoard() {
        for (int x = 0; x < BoardWidth; x++) {
            for (int y = 0; y < BoardHeight; y++) {
                Square spawnedSquare = Instantiate(squarePrefab, new Vector3(x, y, 0), Quaternion.identity);

                string squareName = $"{(char)(x + 97)}{y + 1}";
                int squareIndex = y * BoardHeight + x;

                squares[squareIndex] = spawnedSquare;

                bool isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);

                spawnedSquare.Initialise(squareIndex, squareName, isOffset);
            }
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

                Piece newPiece = CreatePiece(pieceColour + pieceType, index);
                boardState[index] = newPiece;

                file++;
            }
        }

        // Second section determines whose turn it is to move
        turn = sections[1] == "w" ? Turn.White : Turn.Black;


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

        // en passant targets is sections[3], not implemented yet

        // halfmove clock is sections[4], not implemented yet

    }

    Piece CreatePiece(int pieceID, int index) {
        int rank = index / BoardHeight;
        int file = index % BoardHeight;
        Piece spawnPiece = Instantiate(piecePrefab, new Vector3(file, rank, -1), Quaternion.identity);
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

    public void TryToPlacePiece(int index, int newIndex) // tries to place a piece in a new square
    {

        Move? move = TryToGetMove(index, newIndex);

        if (move != null)
        {
            MakeMove((Move)move);
        }
        else
        {
            ResetPiecePosition(index);
        }
    }

    public Move? TryToGetMove(int index, int newIndex)
    {
        foreach (Move move in GetLegalMoves(index))
        {
            if (move.endIndex == newIndex)
            {
                return move;
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
            captureSound.Play();
        }
        else
        {
            moveSound.Play();
        }

        boardState[newIndex] = selectedPiece;
        selectedPiece.index = newIndex;
        selectedPiece.SnapToSquare(newIndex);

    }

    public void DestroyPiece(int index)
    {
        Piece selectedPiece = boardState[index];
        selectedPiece.DestroyPiece();
        Debug.Log("Destroyed piece");
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


    //////////////////
    // Moving rules //
    //////////////////

    public HashSet<Move> GetLegalMoves(int index)
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
                if (castlingRights[0] == true && boardState[7].pieceID == Piece.White + Piece.Rook)
                {
                    // can castle kingside
                    legalMoves.Add(new Move(Move.Castling, index, index + 2, Piece.King, false));
                }
                if (castlingRights[1] == true && boardState[0].pieceID == Piece.White + Piece.Rook)
                {
                    // can castle queenside
                    legalMoves.Add(new Move(Move.Castling, index, index - 2, Piece.King, false));

                }
            }
            else if (!boardState[index].IsWhite() && index == 60)
            {
                if (castlingRights[2] == true && boardState[63].pieceID == Piece.Black + Piece.Rook)
                {
                    legalMoves.Add(new Move(Move.Castling, index, index + 2, Piece.King, false));

                }
                if (castlingRights[3] == true && boardState[56].pieceID == Piece.Black + Piece.Rook)
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
            legalMoves.Add(new Move(Move.Standard, index, newIndex, Piece.Pawn, false));

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
                    legalMoves.Add(new Move(Move.Standard, index, newIndex, Piece.Pawn, true));
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

    public void MakeMove(Move move) // all checks assumed to be complete and this move is allowed
    {
        if (move.moveType == Move.Standard || move.moveType == Move.PawnTwoSquares)
        {

            // if piece is a king, then disable both castling rights
            if (GetPieceTypeAtIndex(move.startIndex) == Piece.King)
            {
                if (CheckPieceIsWhite(move.startIndex))
                {
                    ChangeCastlingRight(true, true, false); // isWhite, isKingside, value
                    ChangeCastlingRight(true, false, false);
                }
                else
                {
                    ChangeCastlingRight(false, true, false);
                    ChangeCastlingRight(false, false, false);
                }
            }

            // if piece is rook and in original position, disable castling right
            if (move.startIndex == 0 && GetPieceTypeAtIndex(move.startIndex) == Piece.Rook && CheckPieceIsWhite(move.startIndex))
            {
                ChangeCastlingRight(true, false, false);
            }
            if (move.startIndex == 7 && GetPieceTypeAtIndex(move.startIndex) == Piece.Rook && CheckPieceIsWhite(move.startIndex))
            {
                ChangeCastlingRight(true, true, false);
            }
            if (move.startIndex == 56 && GetPieceTypeAtIndex(move.startIndex) == Piece.Rook && !CheckPieceIsWhite(move.startIndex))
            {
                ChangeCastlingRight(false, false, false);
            }
            if (move.startIndex == 63 && GetPieceTypeAtIndex(move.startIndex) == Piece.Rook && !CheckPieceIsWhite(move.startIndex))
            {
                ChangeCastlingRight(false, true, false);
            }



            PlacePiece(move.startIndex, move.endIndex);
        }
        if (move.moveType == Move.Castling)
        {
            bool isWhite = CheckPieceIsWhite(move.startIndex);

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
            }
            else
            {
                ChangeCastlingRight(false, true, false);
                ChangeCastlingRight(false, false, false);
            }
        }


        gameMoves.Add(move);
        Debug.Log(move.GetMoveAsString());
        ChangeTurn();
    }

    public bool CheckIfPieceIsTurnColour(int index)
    {
        return (boardState[index].IsWhite() && turn == Turn.White) || (!boardState[index].IsWhite() && turn == Turn.Black);
    }

    public void ChangeTurn()
    {
        turn = turn == Turn.White ? Turn.Black : Turn.White;
    }


    public void ChangeCastlingRight(bool isWhite, bool isKingside, bool value)
    {
        if (isWhite)
        {
            if (isKingside)
            {
                castlingRights[0] = value;
                Debug.Log($"White kingside castling set to {value}");
            }
            else
            {
                castlingRights[1] = value;
                Debug.Log($"White queenside castling set to {value}");

            }
        }
        else
        {
            if (isKingside)
            {
                castlingRights[2] = value;
                Debug.Log($"Black kingside castling set to {value}");

            }
            else
            {
                castlingRights[3] = value;
                Debug.Log($"Black queenside castling set to {value}");

            }
        }
    }

    public int GetPieceTypeAtIndex(int index)
    {
        return boardState[index].pieceID % 8;
    }

    public bool CheckPieceIsWhite(int index)
    {
        return boardState[index].IsWhite();
    }

}
