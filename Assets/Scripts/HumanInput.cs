using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HumanInput : MonoBehaviour
{

    public new Camera camera;
    public GameObject boardObject;
    public Board board;

    public enum InputState
    {
        Idle, Selecting, Dragging
    }

    private InputState currentState = InputState.Idle;
    private int pieceIndex = -1;

    public float dragOffset = -0.3f;


    void Start()
    {
        boardObject = GameObject.FindGameObjectWithTag("BoardObject");
        board = boardObject.GetComponent<Board>();
    }

    void Update()
    {
        HandleInput();
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
                if (board.boardState[index] != null && board.CheckIfPieceIsTurnColour(index))
                {
                    pieceIndex = index;
                    currentState = InputState.Dragging;

                    board.HighlightSquare(pieceIndex);


                    HashSet<int> legalMoves = board.GetLegalMoves(pieceIndex);

                    board.HighlightOptions(legalMoves);

                    // Debug.Log("Set to Dragging");
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
                board.TryToPlacePiece(pieceIndex, newIndex);
            }

            currentState = InputState.Idle;
            board.ResetSquareColour(pieceIndex);

            board.UnHighlightOptionsAllSquares();

            // Debug.Log("Set to Idle");
        }
    }

    public void HandleInputDragging(Vector2 mousePosition)
    {
        board.DragPiece(pieceIndex, mousePosition, dragOffset);
        int newIndex = GetIndexFromMousePosition(mousePosition);

        if (newIndex != -1)
        {
            board.HighlightHover(newIndex);
        }

        if (Input.GetMouseButtonUp(0))
        {

            if (newIndex == pieceIndex) // Trying to place at the same square
            {
                currentState = InputState.Selecting;
                board.boardState[pieceIndex].SnapToSquare(pieceIndex);

                board.UnHighlightHover(pieceIndex);
                // Debug.Log("Set to Selecting");
            }
            else
            {
                if (newIndex != -1) // Trying to place at another square
                {
                    board.TryToPlacePiece(pieceIndex, newIndex);

                    board.UnHighlightHover(newIndex);

                }
                else // Trying to place out of bounds
                {
                    // move back to original place
                    Debug.Log("Tried to place out of bounds");
                    board.ResetPiecePosition(pieceIndex);
                }

                currentState = InputState.Idle;
                board.ResetSquareColour(pieceIndex);

                board.UnHighlightOptionsAllSquares();

                // Debug.Log("Set to Idle");
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
}
