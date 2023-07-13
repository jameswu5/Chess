using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int BoardWidth = 8;
    public const int BoardHeight = 8;
    public const int NumOfSquares = 64;

    public Square squarePrefab;
    public Piece piecePrefab;
    public Transform camera;

    public Piece[] boardState = new Piece[NumOfSquares];

    void Start()
    {
        GenerateBoard();
        MoveCamera();

        boardState[0] = CreatePiece(14, 0);
        boardState[4] = CreatePiece(4, 4);
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

    Piece CreatePiece(int pieceID, int location) {
        int rank = location / BoardHeight;
        int file = location % BoardHeight;
        Piece spawnPiece = Instantiate(piecePrefab, new Vector3(rank, file, -1), Quaternion.identity);
        spawnPiece.Initialise(pieceID, location);

        return spawnPiece;
    }
}
