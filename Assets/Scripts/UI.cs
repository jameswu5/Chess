using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    public Square[] squares;
    public SpriteRenderer[] pieceRenderers;

    public Square squarePrefab;

    public GameObject boardCover;

    public SpriteRenderer[] promotionPieces;
    public Square[] promotionSquares;

    public Sprite[] PieceSprites;

    const float pieceOffset = -0.1f;
    const float promotionPieceOffset = -0.7f;
    const float pieceSize = 0.33f;

    public void CreateUI(int[] boardState)
    {
        squares = new Square[64];
        pieceRenderers = new SpriteRenderer[64];

        promotionPieces = new SpriteRenderer[4];
        promotionSquares = new Square[4];

        for (int i = 0; i < 64; i++)
        {
            Square newSquare = CreateSquare(i);
            squares[i] = newSquare;

            // need to create the pieces based on the board state
            if (boardState[i] != Piece.None)
            {
                CreatePiece(boardState[i], i);
            }
        }


    }

    private Square CreateSquare(int index, float elevation = 0)
    {
        int x = index % 8;
        int y = index / 8;

        Square spawnSquare = Instantiate(squarePrefab, new Vector3(x, y, elevation), Quaternion.identity);
        string squareName = $"{(char)(x + 'a')}{y + 1}";
        bool isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
        spawnSquare.Initialise(index, squareName, isOffset);

        return spawnSquare;
    }

    public SpriteRenderer CreatePiece(int pieceID, int index, float elevation = pieceOffset, bool putInArray = true)
    {
        int x = index % 8;
        int y = index / 8;

        SpriteRenderer piece = new GameObject().AddComponent<SpriteRenderer>();

        piece.transform.position = new Vector3(x, y, elevation);
        piece.sprite = PieceSprites[pieceID];
        piece.transform.localScale = new Vector3(pieceSize, pieceSize);
        piece.gameObject.name = Piece.pieceDictionary[pieceID];

        if (putInArray)
        {
            pieceRenderers[index] = piece;
        }

        return piece;
    }


    public void DragPiece(int index, Vector2 mousePosition, float dragOffset)
    {
        pieceRenderers[index].transform.position = new Vector3(mousePosition.x, mousePosition.y, -0.1f + dragOffset);
    }

    public void MovePieceToSquare(int startIndex, int newIndex)
    {
        int x = newIndex % 8;
        int y = newIndex / 8;

        pieceRenderers[startIndex].transform.position = new Vector3(x, y, -0.1f);

        // update position of the piece
        pieceRenderers[newIndex] = pieceRenderers[startIndex];

        if (newIndex != startIndex)
        {
            pieceRenderers[startIndex] = null;
        }
    }


    public void DestroyPieceSprite(int index)
    {
        if (pieceRenderers[index] != null)
        {
            Destroy(pieceRenderers[index].gameObject);
            pieceRenderers[index] = null;
        }
    }


    public void ResetSquareColour(int index)
    {
        squares[index].InitialiseColor();
    }

    public void HighlightSquare(int index)
    {
        squares[index].Highlight();
    }

    public void HighlightHover(int index)
    {
        // We unhighlight every single square because we don't know which square it was on before.
        // Still technically O(1) but I'm not a fan.

        for (int i = 0; i < 64; i++)
        {
            UnHighlightHover(i);
        }

        squares[index].SetHoverHighlight(true);
    }

    public void UnHighlightHover(int index)
    {
        squares[index].SetHoverHighlight(false);
    }

    public void HighlightOptions(IEnumerable<int> moves)
    {
        foreach (int move in moves)
        {
            squares[Move.GetEndIndex(move)].SetOptionHighlight(true);
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



    // Promotion //

    private void SetBoardCover(bool value)
    {
        boardCover.SetActive(value);
    }


    public void EnablePromotionScreen(int index)
    {
        // make the board darker
        SetBoardCover(true);

        int colourMultiplier = Square.GetRank(index) == 1 ? 1 : -1;
        int pieceColour = Square.GetRank(index) == 1 ? Piece.Black : Piece.White;

        // create the pieces

        SpriteRenderer queen = CreatePiece(Piece.Queen + pieceColour, index, promotionPieceOffset, false);
        SpriteRenderer rook = CreatePiece(Piece.Rook + pieceColour, index + 8 * colourMultiplier, promotionPieceOffset, false);
        SpriteRenderer bishop = CreatePiece(Piece.Bishop + pieceColour, index + 16 * colourMultiplier, promotionPieceOffset, false);
        SpriteRenderer knight = CreatePiece(Piece.Knight + pieceColour, index + 24 * colourMultiplier, promotionPieceOffset, false);

        promotionPieces[0] = queen;
        promotionPieces[1] = rook;
        promotionPieces[2] = bishop;
        promotionPieces[3] = knight;

        // create the squares

        for (int i = 0; i < 4; i++)
        {
            promotionSquares[i] = CreateSquare(index + 8 * i * colourMultiplier, -0.6f);
        }
    }


    public void DisablePromotionScreen()
    {
        // revert the board colour
        SetBoardCover(false);

        // remove the squares and pieces icons

        foreach (SpriteRenderer piece in promotionPieces)
        {
            Destroy(piece.gameObject);
        }


        foreach (Square square in promotionSquares)
        {
            square.DestroySquare();
        }

        Array.Clear(promotionPieces, 0, promotionPieces.Length);
        Array.Clear(promotionSquares, 0, promotionSquares.Length);
    }



    public void ResetBoardUI()
    {
        for (int i = 0; i < 64; i++)
        {
            squares[i].DestroySquare();
            DestroyPieceSprite(i);
        }

        Array.Clear(squares, 0, 64);
        Array.Clear(pieceRenderers, 0, 64);
    }

}
