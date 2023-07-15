using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int BoardWidth = 8;
    public const int BoardHeight = 8;
    public const int NumOfSquares = 64;

    public const string startFENPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public const string testFENPosition = "8/8/8/4k3/8/8/8/8";

    public Square squarePrefab;
    public Piece piecePrefab;
    public new Camera camera;
    public AudioSource captureSound;
    public AudioSource moveSound;

    public Piece[] boardState = new Piece[NumOfSquares];


    public enum InputState
    {
        None, Selected, Dragging
    }

    public InputState currentState = InputState.None;



    void Start()
    {
        GenerateBoard();
        MoveCamera();
        GenerateBoardStateFromFEN();
    }


    void GenerateBoard() {
        for (int x = 0; x < BoardWidth; x++) {
            for (int y = 0; y < BoardHeight; y++) {
                Square spawnedSquare = Instantiate(squarePrefab, new Vector3(x, y, 0), Quaternion.identity);
                spawnedSquare.name = $"{(char)(x + 97)}{y+1}";

                bool isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedSquare.SetColor(isOffset);
            }
        }
    }

    void MoveCamera() {
        camera.transform.position = new Vector3((float) BoardWidth / 2 - 0.5f, (float) BoardHeight / 2 - 0.5f, -10);
    }

    void GenerateBoardStateFromFEN(string FENPosition = startFENPosition) {

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
}
