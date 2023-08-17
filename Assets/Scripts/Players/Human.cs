using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        if (board.gameResult == Board.Result.Playing)
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

    public void HandleInput()
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

    public void HandleInputIdle(Vector2 mousePosition)
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

    public void HandleInputSelecting(Vector2 mousePosition)
    {
        if (Input.GetMouseButtonDown(0))
        {
            int newIndex = GetIndexFromMousePosition(mousePosition);

            if (newIndex != pieceIndex && newIndex != -1)
            {

                if (board.CheckNeedForPromotion(pieceIndex, newIndex) && board.CheckPieceCanMoveThere(pieceIndex, newIndex))
                {
                    board.EnablePromotionScreen(newIndex);
                }
                else
                {
                    board.TryToPlacePiece(pieceIndex, newIndex);
                }
            }

            currentState = InputState.Idle;
            boardUI.ResetSquareColour(pieceIndex);
            boardUI.UnHighlightOptionsAllSquares();
        }
    }

    public void HandleInputDragging(Vector2 mousePosition)
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

                    if (board.CheckNeedForPromotion(pieceIndex, newIndex) && board.CheckPieceCanMoveThere(pieceIndex, newIndex))
                    {
                        board.EnablePromotionScreen(newIndex);
                    }
                    else
                    {
                        board.TryToPlacePiece(pieceIndex, newIndex);
                        boardUI.UnHighlightHover(newIndex);
                    }

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

    public void HandlePromotionInput(int promotionIndex)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);

            int index = GetIndexFromMousePosition(mousePosition);

            int indexDifference = Mathf.Abs(index - promotionIndex);
            switch (indexDifference)
            {
                case 0: // Queen
                    board.TryToPlacePiece(pieceIndex, promotionIndex, Piece.Queen);
                    break;
                case 8: // Rook
                    board.TryToPlacePiece(pieceIndex, promotionIndex, Piece.Rook);
                    break;
                case 16: // Bishop
                    board.TryToPlacePiece(pieceIndex, promotionIndex, Piece.Bishop);
                    break;
                case 24: // Knight
                    board.TryToPlacePiece(pieceIndex, promotionIndex, Piece.Knight);
                    break;
                default:
                    boardUI.MovePieceToSquare(pieceIndex, pieceIndex);
                    board.DisablePromotionScreen();
                    board.inPromotionScreen = -1;
                    break;
            }
            
        }
    }

    public int GetIndexFromMousePosition(Vector2 mousePosition)
    {
        if (mousePosition.x >= -0.5f && mousePosition.x <= 7.5 && mousePosition.y >= -0.5f && mousePosition.y <= 7.5f)
        {
            return Mathf.RoundToInt(mousePosition.y) * 8 + Mathf.RoundToInt(mousePosition.x);
        }
        else
        {
            return -1; // clicked somewhere not on the board
        }
    }

    public void CancelMove()
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


}
