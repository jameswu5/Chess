using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int BoardWidth = 8;
    public const int BoardHeight = 8;
    public const int NumOfSquares = 64;

    public const string startFENPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string testFENPosition = "8/8/8/8/2n5/8/8/8";

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

    public Turn turn = Turn.White;

    public enum InputState
    {
        None, Selected, Dragging
    }
    public InputState currentState = InputState.None;

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
        if (CheckIfMoveIsLegal(index, newIndex))
        {
            MakeMove(index, newIndex);
        }
        else
        {
            ResetPiecePosition(index);
        }
    }

    public bool CheckIfMoveIsLegal(int index, int newIndex)
    {
        return GetLegalMoves(index).Contains(newIndex);
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

    public void HighlightOptions(IEnumerable<int> indices)
    {
        foreach (int index in indices)
        {
            squares[index].SetOptionHighlight(true);
        }
    }

    public void TestHighlightOptions()
    {
        int[] indices = new int[] { 2, 6, 10, 14, 16 };
        HighlightOptions(indices);
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

    public HashSet<int> GetLegalMoves(int index)
    {
        Piece currentPiece = boardState[index];
        HashSet<int> legalMoves = new();

        switch (currentPiece.pieceID % 8)
        {
            case Piece.King:
                legalMoves = KingMoves(index);
                break;

            case Piece.Queen:
                legalMoves = SlideMoves(index, Directions);
                break;

            case Piece.Bishop:
                legalMoves = SlideMoves(index, Directions[4..]);
                break;

            case Piece.Knight:
                legalMoves = KnightMoves(index);
                break;

            case Piece.Rook:
                legalMoves = SlideMoves(index, Directions[0..4]);
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

    public HashSet<int> SlideMoves(int index, IEnumerable<int> offsets)
    {
        HashSet<int> legalMoves = new();

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
                        legalMoves.Add(currentSquareIndex);
                    }
                    else
                    {
                        if (boardState[currentSquareIndex].IsWhite() != boardState[index].IsWhite()) // different colour so can capture
                        {
                            legalMoves.Add(currentSquareIndex);
                        }
                        break;
                    }
                }
            }

        }

        return legalMoves;
    }

    public HashSet<int> KingMoves(int index)
    {
        HashSet<int> legalMoves = new();
        foreach (int offset in Directions)
        {
            if (!CheckIfAtEdge(index, offset))
            {
                int newIndex = index + offset;
                if (newIndex >= 0 && newIndex < NumOfSquares)
                {
                    if (boardState[newIndex] == null || boardState[newIndex].IsWhite() != boardState[index].IsWhite())
                    {
                        legalMoves.Add(newIndex);
                    }
                }
            }
        }

        return legalMoves;
    }

    public HashSet<int> KnightMoves(int index)
    {
        int[] offsets = { -15, -6, 10, 17, 15, 6, -10, -17 };
        HashSet<int> legalMoves = new();
        foreach (int offset in offsets)
        {
            if (!CheckIfAtEdgeForKnight(index, offset))
            {
                int newIndex = index + offset;
                if (newIndex >= 0 && newIndex < NumOfSquares)
                {
                    if (boardState[newIndex] == null || boardState[newIndex].IsWhite() != boardState[index].IsWhite())
                    {
                        legalMoves.Add(newIndex);
                    }
                }
            }
        }
        return legalMoves;
    }

    public HashSet<int> PawnMoves(int index)
    {
        HashSet<int> legalMoves = new();
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
            legalMoves.Add(newIndex);

            // Still in original rank
            if ((curPiece.IsWhite() && curPiece.GetRank() == 2) || (!curPiece.IsWhite() && curPiece.GetRank() == 7))
            {
                // moveforward two squares
                newIndex += offsets[0];
                if (boardState[newIndex] == null)
                {
                    legalMoves.Add(newIndex);
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
                    legalMoves.Add(newIndex);
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



    public void MakeMove(int index, int newIndex) // all checks assumed to be complete and this move is allowed
    {
        PlacePiece(index, newIndex);
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
}
