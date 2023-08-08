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
    public int index;
    public Sprite[] pieceSprites;
    public SpriteRenderer spriteRenderer;


    public void Initialise(int pieceID, int index) {
        this.pieceID = pieceID;
        this.index = index;
        SetSprite();
    }

    private void SetSprite() {
        spriteRenderer.sprite = pieceSprites[pieceID];
    }
    
    public void Drag(Vector2 mousePosition, float dragOffset)
    {
        transform.position = new Vector3(mousePosition.x, mousePosition.y, -0.1f + dragOffset);
    }

    public void SnapToSquare(int newIndex) {
        int x = newIndex % 8;
        int y = newIndex / 8;
        transform.position = new Vector3(x, y, -0.1f);
    }

    public void DestroyPiece()
    {
        Destroy(gameObject);
    }


    public int GetPieceType()
    {
        return pieceID % 8;
    }

    public bool IsWhite()
    {
        return pieceID < 8;
    }


    public int GetRank()
    {
        return (index / 8) + 1;
    }

    public string GetCharacterFromPieceType()
    {
        switch (pieceID)
        {
            case White + King:
                return "K";
            case White + Queen:
                return "Q";
            case White + Bishop:
                return "B";
            case White + Knight:
                return "N";
            case White + Rook:
                return "R";
            case White + Pawn:
                return "P";
            case Black + King:
                return "k";
            case Black + Queen:
                return "q";
            case Black + Bishop:
                return "b";
            case Black + Knight:
                return "n";
            case Black + Rook:
                return "r";
            case Black + Pawn:
                return "p";
            default:
                return "";
        }
    }
}
