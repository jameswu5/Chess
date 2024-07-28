using Raylib_cs;
using static Raylib_cs.Raylib;
using static Chess.UI.Settings;

namespace Chess.Player;

public class Human : Player
{
    public Human(Core.Board board)
    {
        this.board = board;
    }

    public override void Update()
    {
        if (IsMouseButtonPressed(0))
        {
            Console.WriteLine(GetMouseIndex());
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