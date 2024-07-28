using Raylib_cs;
using static Raylib_cs.Raylib;
using static Chess.UI.Settings;

namespace Chess.Player;

public class Human : Player
{
    public enum InputState { Idle, Selecting, Dragging }
    public InputState currentState;
    private int pieceIndex;

    public Human(Game game)
    {
        this.game = game;
        board = game.board;
        currentState = InputState.Idle;
        pieceIndex = -1;
    }

    public override void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        switch (currentState) {
            case InputState.Idle:
                HandleInputIdle();
                break;
            case InputState.Dragging:
                HandleInputDragging();
                break;
            case InputState.Selecting:
                HandleInputSelecting();
                break;
            default:
                break;
        }
    }

    private void HandleInputIdle()
    {
        if (IsMouseButtonDown(0))
        {
            int index = GetMouseIndex();
            if (index == -1) return;

            // Check if there is a piece
            if (board.GetPieceAtIndex(index) != Core.Piece.None && board.CheckIfPieceIsColour(index, board.turn))
            {
                pieceIndex = index;
                currentState = InputState.Dragging;

                game.ui.HighlightSquare(pieceIndex);

                // List<int> legalMoves = board.GetLegalMoves(pieceIndex);
                // game.ui.HighlightOptions(legalMoves);
            }
            
        }
    }

    private void HandleInputDragging()
    {
        
    }

    private void HandleInputSelecting()
    {
        
    }


    public static int GetMouseIndex()
    {
        (int mouseX, int mouseY) = GetMousePosition();

        // Check if mouse is hovering in the board
        if (mouseX <= Board.HorOffset || mouseX >= Board.HorOffset + Square.Size * 8 ||
            mouseY <= Board.VerOffset || mouseY >= Board.VerOffset + Square.Size * 8)
        {
            return -1;
        }

        int x = (mouseX - Board.HorOffset) / Square.Size;
        int y = 7 - (mouseY - Board.VerOffset) / Square.Size;

        return UI.Square.GetIndexFromCoords(x, y);
    }

    private static (int, int) GetMousePosition() => (GetMouseX(), GetMouseY());

    public override string ToString() => "Human";
}