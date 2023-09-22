using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Player
{
    public new Camera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    public enum InputState { Idle, Selecting, Dragging }
    public InputState currentState = InputState.Idle;
    private int pieceIndex = -1;
    public float dragOffset = -0.2f;
    public UI boardUI;

    public Human()
    {
        boardUI = board.boardUI;
    }

    public override void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // This is broken when you try to play a bot
            CancelMove();
            board.UndoMove(true);
        }

        if (board.gameResult == Judge.Result.Playing)
        {
            if (board.inPromotionScreen == -1)
            {
                HandleInput();
            }
            else
            {
                HandlePromotionInput(board.inPromotionScreen);
            }
        }
    }

    public override void TurnToMove() { } // Do nothing

    void HandleInput()
    {
        Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);

        switch (currentState) {
            case InputState.Idle:
                HandleInputIdle(mousePosition);
                break;
            case InputState.Dragging:
                HandleInputDragging(mousePosition);
                break;
            case InputState.Selecting:
                HandleInputSelecting(mousePosition);
                break;
            default:
                break;
        }
    }

    void HandleInputIdle(Vector2 mousePosition)
    {
        if (Input.GetMouseButtonDown(0))
        {
            int index = GetIndexFromMousePosition(mousePosition);

            if (index != -1)
            {
                if (board.GetPieceTypeAtIndex(index) != Piece.None && board.CheckIfPieceIsColour(index, board.turn))
                {
                    pieceIndex = index;
                    currentState = InputState.Dragging;

                    boardUI.HighlightSquare(pieceIndex);

                    List<int> legalMoves = board.GetLegalMoves(pieceIndex);
                    boardUI.HighlightOptions(legalMoves);
                }
            }
        }
    }

    void HandleInputSelecting(Vector2 mousePosition)
    {
        if (Input.GetMouseButtonDown(0))
        {
            int newIndex = GetIndexFromMousePosition(mousePosition);

            if (newIndex != pieceIndex && newIndex != -1)
            {
                TryToGetMove(pieceIndex, newIndex);
            }

            currentState = InputState.Idle;
            boardUI.ResetSquareColour(pieceIndex);
            boardUI.UnHighlightOptionsAllSquares();
        }
    }

    void HandleInputDragging(Vector2 mousePosition)
    {
        boardUI.DragPiece(pieceIndex, mousePosition, dragOffset);
        int newIndex = GetIndexFromMousePosition(mousePosition);

        if (newIndex != -1)
        {
            boardUI.HighlightHover(newIndex);
        }

        if (Input.GetMouseButtonUp(0))
        {

            if (newIndex == pieceIndex) // Trying to place at the same square
            {
                currentState = InputState.Selecting;

                boardUI.MovePieceToSquare(pieceIndex, pieceIndex);
                boardUI.UnHighlightHover(pieceIndex);
            }
            else
            {
                if (newIndex != -1) // Trying to place at another square
                {
                    TryToGetMove(pieceIndex, newIndex);
                    boardUI.UnHighlightHover(newIndex);
                }
                else // Trying to place out of bounds
                {
                    // move back to original place
                    Debug.Log("Tried to place out of bounds");
                    boardUI.MovePieceToSquare(pieceIndex, pieceIndex);
                }

                currentState = InputState.Idle;
                boardUI.ResetSquareColour(pieceIndex);

                boardUI.UnHighlightOptionsAllSquares();
            }
        }
    }

    void HandlePromotionInput(int promotionIndex)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);

            int index = GetIndexFromMousePosition(mousePosition);

            int indexDifference = Mathf.Abs(index - promotionIndex);
            switch (indexDifference)
            {
                case 0: // Queen
                    TryToGetMove(pieceIndex, promotionIndex, Piece.Queen);
                    break;
                case 8: // Rook
                    TryToGetMove(pieceIndex, promotionIndex, Piece.Rook);
                    break;
                case 16: // Bishop
                    TryToGetMove(pieceIndex, promotionIndex, Piece.Bishop);
                    break;
                case 24: // Knight
                    TryToGetMove(pieceIndex, promotionIndex, Piece.Knight);
                    break;
                default:
                    boardUI.MovePieceToSquare(pieceIndex, pieceIndex);
                    board.DisablePromotionScreen();
                    board.inPromotionScreen = -1;
                    break;
            }
        }
    }

    int GetIndexFromMousePosition(Vector2 mousePosition)
    {
        if (mousePosition.x >= -0.5f && mousePosition.x <= 7.5 && mousePosition.y >= -0.5f && mousePosition.y <= 7.5f)
        {
            return Mathf.RoundToInt(mousePosition.y) * 8 + Mathf.RoundToInt(mousePosition.x);
        }

        return -1; // clicked somewhere not on the board
    }

    void CancelMove()
    {
        if (currentState != InputState.Idle)
        {
            Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);

            boardUI.MovePieceToSquare(pieceIndex, pieceIndex);
            boardUI.ResetSquareColour(pieceIndex);
            boardUI.UnHighlightHover(GetIndexFromMousePosition(mousePosition));
            boardUI.UnHighlightOptionsAllSquares();
        }

        if (board.inPromotionScreen != -1)
        {
            boardUI.MovePieceToSquare(pieceIndex, pieceIndex);
            board.DisablePromotionScreen();
            board.inPromotionScreen = -1;
        }

        currentState = InputState.Idle;
    }

    void TryToGetMove(int index, int newIndex, int promotionType = 0)
    {
        int move = board.TryToPlacePiece(index, newIndex, promotionType);

        if (move != 0)
        {
            Decided(move);
        }
    }

}
