using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int BoardWidth = 8;
    public const int BoardHeight = 8;
    public const int NumOfSquares = 64;

    public const string startFENPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public Square squarePrefab;
    public Piece piecePrefab;
    public Transform camera;

    public Piece[] boardState = new Piece[NumOfSquares];

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

                int location = rank * BoardHeight + file;
                
                CreatePiece(pieceColour + pieceType, location);

                file++;
            }
        }
    }

    Piece CreatePiece(int pieceID, int location) {
        int rank = location / BoardHeight;
        int file = location % BoardHeight;
        Piece spawnPiece = Instantiate(piecePrefab, new Vector3(file, rank, -1), Quaternion.identity);
        spawnPiece.Initialise(pieceID, location);
        return spawnPiece;
    }
}
