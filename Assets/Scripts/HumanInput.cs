using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HumanInput : MonoBehaviour
{

    public Camera camera;
    public GameObject boardObject;
    public Board board;

    public enum InputState
    {
        Idle, Selected, Dragging
    }

    public InputState currentState = InputState.Idle;

    private int pieceIndex = -1; // selected index


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
            case InputState.Selected:
                HandleInputSelected();
                break;
            default:
                break;
        }


        if (Input.GetMouseButtonDown(1))
        {
            CancelAction();
        }
    }


    public void HandleInputIdle(Vector2 mousePosition)
    {
        if (Input.GetMouseButtonDown(0))
        {
            int index = GetIndexFromMousePosition(mousePosition);

            if (index != -1)
            {
                if (board.boardState[index] != null)
                {
                    pieceIndex = index;
                    currentState = InputState.Dragging;

                    Debug.Log("Set to Dragging");
                }
            }

        }
    }

    public void HandleInputSelected()
    {

    }

    public void HandleInputDragging(Vector2 mousePosition)
    {
        board.DragPiece(pieceIndex, mousePosition);

        if (Input.GetMouseButtonUp(0))
        {
            int newIndex = GetIndexFromMousePosition(mousePosition);

            if (newIndex != -1)
            {
                board.PlacePiece(pieceIndex, newIndex);
            }
            else
            {
                // move back to original place
                Debug.Log("Tried to place out of bounds");

                board.boardState[pieceIndex].SnapToSquare(pieceIndex);

                // board.PlacePiece(pieceIndex, pieceIndex);
            }

            currentState = InputState.Idle;
            Debug.Log("Set to Idle");
        }

    }

    public void CancelAction()
    {

    }

    public int GetIndexFromMousePosition(Vector2 mousePosition)
    {
        if (mousePosition.x >= -0.5f && mousePosition.x <= 7.5 && mousePosition.y >= -0.5f && mousePosition.y <= 7.5f)
        {
            return Mathf.RoundToInt(mousePosition.y) * 8 + Mathf.RoundToInt(mousePosition.x);
        }
        else
        {
            return -1; // clicked somewhere not in the board
        }
    }
}
