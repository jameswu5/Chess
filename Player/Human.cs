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

                game.ui.HighlightSquare(pieceIndex);

                List<int> legalMoves = board.GetLegalMoves(pieceIndex);
                game.ui.HighlightOptions(legalMoves);

                currentState = InputState.Dragging;
            }   
        }
    }

    private void HandleInputDragging()
    {
        int mouseX = GetMouseX();
        int mouseY = GetMouseY();

        game.ui.DragPiece(pieceIndex, mouseX, mouseY);

        int newIndex = GetMouseIndex();

        if (newIndex != -1)
        {
            game.ui.HighlightHover(newIndex);
        }

        if (IsMouseButtonReleased(0))
        {
            if (newIndex == pieceIndex) // Trying to place at the same square
            {
                currentState = InputState.Selecting;

                game.ui.MovePieceToSquare(pieceIndex, pieceIndex);
                game.ui.UnHighlightHover(pieceIndex);
            }
            else
            {
                if (newIndex != -1) // Trying to place at another square
                {
                    TryToGetMove(pieceIndex, newIndex);
                    game.ui.UnHighlightHover(newIndex);
                }
                else // Trying to place out of bounds
                {
                    // move back to original place
                    Console.WriteLine("Tried to place out of bounds");
                    game.ui.MovePieceToSquare(pieceIndex, pieceIndex);
                }

                currentState = InputState.Idle;
                game.ui.ResetSquareColour(pieceIndex);

                game.ui.UnHighlightOptionsAllSquares();
            }
        }
    }

    private void HandleInputSelecting()
    {
        if (IsMouseButtonDown(0))
        {
            int newIndex = GetMouseIndex();

            if (newIndex != pieceIndex && newIndex != -1)
            {
                TryToGetMove(pieceIndex, newIndex);
            }

            currentState = InputState.Idle;
            game.ui.ResetSquareColour(pieceIndex);
            game.ui.UnHighlightOptionsAllSquares();
        }
    }

    private void TryToGetMove(int index, int newIndex, int promotionType = 0)
    {
        int move = board.TryToPlacePiece(index, newIndex, promotionType);

        if (move == 0)
        {
            game.ui.ResetPiecePosition(index, true);
        }
        else
        {
            Decided(move);
        }
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