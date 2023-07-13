using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public const int None = 0;
    public const int King = 1;
    public const int Queen = 2;
    public const int Bishop = 3;
    public const int Knight = 4;
    public const int Rook = 5;
    public const int Pawn = 6;

    public const int White = 0;
    public const int Black = 8;

    public int pieceID;
    public int location;
    public Sprite[] pieceSprites;
    public SpriteRenderer spriteRenderer;

    public void Initialise(int pieceIDParam, int locationParam) {
        pieceID = pieceIDParam;
        location = locationParam;
        SetSprite();
    }

    public void SetSprite() {
        spriteRenderer.sprite = pieceSprites[pieceID];
    }
}
