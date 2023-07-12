using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int boardWidth = 8;
    public const int boardHeight = 8;

    public Square squarePrefab;

    public Transform camera;

    // Start is called before the first frame update
    void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard() {
        for (int x = 0; x < boardWidth; x++) {
            for (int y = 0; y < boardHeight; y++) {
                Square spawnedSquare = Instantiate(squarePrefab, new Vector3(x, y), Quaternion.identity);
                spawnedSquare.name = $"{(char)(x + 97)}{y+1}";

                bool isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedSquare.SetColor(isOffset);
            }
        }

        camera.transform.position = new Vector3((float) boardWidth / 2 - 0.5f, (float) boardHeight / 2 - 0.5f, -10);
    }
}
