using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public const int King = 1;
    public const int Queen = 2;
    public const int Bishop = 3;
    public const int Knight = 4;
    public const int Rook = 5;
    public const int Pawn = 6;

    public const int White = 0;
    public const int Black = 8;

    public int pieceID;
    public int index;
    public Sprite[] pieceSprites;
    public SpriteRenderer spriteRenderer;


    public void Initialise(int pieceIDParam, int indexParam) {
        pieceID = pieceIDParam;
        index = indexParam;
        SetSprite();
    }

    public void SetSprite() {
        spriteRenderer.sprite = pieceSprites[pieceID];
    }

    public void Drag(Vector2 mousePosition, float dragOffset)
    {
        transform.position = new Vector3(mousePosition.x, mousePosition.y, -1 + dragOffset);

    }

    public void SnapToSquare(int newIndex) {
        int x = newIndex % 8;
        int y = newIndex / 8;
        transform.position = new Vector3(x, y, -1);
    }

    public void DestroyPiece()
    {
        Destroy(gameObject);
    }


    public bool IsWhite()
    {
        return pieceID < 8;
    }


    public int GetRank()
    {
        return (index / 8) + 1;
    }
}
